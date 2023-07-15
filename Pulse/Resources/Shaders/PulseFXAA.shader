Shader "Hidden/_Pulse_PulseFXAA"
{
    HLSLINCLUDE
        #pragma exclude_renderers gles

        // Include neccessary libraries

        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "PulseCommon.hlsl"
        //#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

       // From : https://www.shadertoy.com/view/stlSzf
       /* pixel index in 3*3 kernel
            +---+---+---+
            | 0 | 1 | 2 |
            +---+---+---+
            | 3 | 4 | 5 |
            +---+---+---+
            | 6 | 7 | 8 |
            +---+---+---+
        */
        #define UP_LEFT      0
        #define UP           1
        #define UP_RIGHT     2
        #define LEFT         3
        #define CENTER       4
        #define RIGHT        5
        #define DOWN_LEFT    6
        #define DOWN         7
        #define DOWN_RIGHT   8
        static const half2 KERNEL_STEP_MAT[9] = {
            half2(-1.0, 1.0), half2(0.0, 1.0), half2(1.0, 1.0),
            half2(-1.0, 0.0), half2(0.0, 0.0), half2(1.0, 0.0),
            half2(-1.0, -1.0), half2(0.0, -1.0), half2(1.0, -1.0)
        };

        /* in order to accelerate exploring along tangent bidirectional, step by an increasing amount of pixels QUALITY(i) 
           the max step count is 12
            +-----------------+---+---+---+---+---+---+---+---+---+---+---+---+
            |step index       | 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 |10 |11 |
            +-----------------+---+---+---+---+---+---+---+---+---+---+---+---+
            |step pixels count|1.0|1.0|1.0|1.0|1.0|1.5|2.0|2.0|2.0|2.0|4.0|8.0|
            +-----------------+---+---+---+---+---+---+---+---+---+---+---+---+
        */
        #define STEP_COUNT_MAX   12
        float QUALITY(int i) {
            if (i < 5) return 1.0;
            if (i == 5) return 1.5;
            if (i < 10) return 2.0;
            if (i == 10) return 4.0;
            if (i == 11) return 8.0;
            return 1;
        }

        #define EDGE_THRESHOLD_MIN  0.0312
        #define EDGE_THRESHOLD_MAX  0.125
        #define SUBPIXEL_QUALITY    0.75
        #define GRADIENT_SCALE      0.25

        half4 fxaa_3_11(half2 uv, half2 uv_step) {
            // get luma of kernel
            half luma_mat[9];
            for (int i = 0; i < 9; i++) {
                luma_mat[i] = Luminance(SAMPLE_TEXTURE2D_X(_BlitTexture, _sampler_Linear_Clamp, uv + uv_step * KERNEL_STEP_MAT[i]).xyz);
            }
    
            // detecting where to apply FXAA, return the pixel color if not
            half luma_min = min(luma_mat[CENTER], min(min(luma_mat[UP], luma_mat[DOWN]), min(luma_mat[LEFT], luma_mat[RIGHT])));
            half luma_max = max(luma_mat[CENTER], max(max(luma_mat[UP], luma_mat[DOWN]), max(luma_mat[LEFT], luma_mat[RIGHT])));
            half luma_range = luma_max - luma_min;
            if(luma_range < max(EDGE_THRESHOLD_MIN, luma_max * EDGE_THRESHOLD_MAX)) return SAMPLE_TEXTURE2D_X(_BlitTexture, _sampler_Linear_Clamp, uv);
    
            // choosing edge tangent
            // horizontal: |(upleft-left)-(left-downleft)|+2*|(up-center)-(center-down)|+|(upright-right)-(right-downright)|
            // vertical: |(upright-up)-(up-upleft)|+2*|(right-center)-(center-left)|+|(downright-down)-(down-downleft)|
            half luma_horizontal = 
                abs(luma_mat[UP_LEFT] + luma_mat[DOWN_LEFT] - 2.0 * luma_mat[LEFT])
                + 2.0 * abs(luma_mat[UP] + luma_mat[DOWN] - 2.0 * luma_mat[CENTER])
                + abs(luma_mat[UP_RIGHT] + luma_mat[DOWN_RIGHT] - 2.0 * luma_mat[RIGHT]);
            half luma_vertical = 
                abs(luma_mat[UP_LEFT] + luma_mat[UP_RIGHT] - 2.0 * luma_mat[UP])
                + 2.0 * abs(luma_mat[LEFT] + luma_mat[RIGHT] - 2.0 * luma_mat[CENTER])
                + abs(luma_mat[DOWN_LEFT] + luma_mat[DOWN_RIGHT] - 2.0 * luma_mat[DOWN]);
            bool is_horizontal = luma_horizontal > luma_vertical;
    
            // choosing edge normal 
            half gradient_down_left = (is_horizontal ? luma_mat[DOWN] : luma_mat[LEFT]) - luma_mat[CENTER];
            half gradient_up_right = (is_horizontal ? luma_mat[UP] : luma_mat[RIGHT]) - luma_mat[CENTER];
            bool is_down_left = abs(gradient_down_left) > abs(gradient_up_right);
    
            // get the tangent uv step vector and the normal uv step vector
            half2 step_tangent = (is_horizontal ? half2(1.0, 0.0) : half2(0.0, 1.0)) * uv_step;
            half2 step_normal =  (is_down_left ? -1.0 : 1.0) * (is_horizontal ? half2(0.0, 1.0) : half2(1.0, 0.0)) * uv_step;
    
            // get the change rate of gradient in normal per pixel
            half gradient = is_down_left ? gradient_down_left : gradient_up_right;
    
            // start at middle point of tangent edge
            half2 uv_start = uv + 0.5 * step_normal;
            half luma_average_start = luma_mat[CENTER] + 0.5 * gradient;    
            //return vec4(luma_average_start, luma_average_start,luma_average_start, 1.0);
    
            // explore along tangent bidirectional until reach the edge both
            half2 uv_pos = uv_start + step_tangent;
            half2 uv_neg = uv_start - step_tangent;
    
            //return texture(iChannel0, uv_neg);
    
            half delta_luma_pos = Luminance(SAMPLE_TEXTURE2D_X(_BlitTexture, _sampler_Linear_Clamp, uv_pos).rgb) - luma_average_start;
            half delta_luma_neg = Luminance(SAMPLE_TEXTURE2D_X(_BlitTexture, _sampler_Linear_Clamp, uv_neg).rgb) - luma_average_start;
    
            bool reached_pos = abs(delta_luma_pos) > GRADIENT_SCALE * abs(gradient);
            bool reached_neg = abs(delta_luma_neg) > GRADIENT_SCALE * abs(gradient);
            bool reached_both = reached_pos && reached_neg;
    
            if (!reached_pos) uv_pos += step_tangent;
            if (!reached_neg) uv_neg -= step_tangent;
    
            if (!reached_both) {
                for(int i = 2; i < STEP_COUNT_MAX; i++){
                    if(!reached_pos) delta_luma_pos = Luminance(SAMPLE_TEXTURE2D_X(_BlitTexture, _sampler_Linear_Clamp, uv_pos).rgb) - luma_average_start;
                    if(!reached_neg) delta_luma_neg = Luminance(SAMPLE_TEXTURE2D_X(_BlitTexture, _sampler_Linear_Clamp, uv_neg).rgb) - luma_average_start;
            
                    bool reached_pos = abs(delta_luma_pos) > GRADIENT_SCALE * abs(gradient);
                    bool reached_neg = abs(delta_luma_neg) > GRADIENT_SCALE * abs(gradient);
                    bool reached_both = reached_pos && reached_neg;
            
                    if (!reached_pos) uv_pos += (QUALITY(i) * step_tangent);
                    if (!reached_neg) uv_neg -= (QUALITY(i) * step_tangent);
            
                    if (reached_both) break;
                }
            }
    
            //return texture(iChannel0, uv_neg);
    
            // estimating offset
            half length_pos = max(abs(uv_pos - uv_start).x, abs(uv_pos - uv_start).y);
            half length_neg = max(abs(uv_neg - uv_start).x, abs(uv_neg - uv_start).y);
            bool is_pos_near = length_pos < length_neg;
    
            half pixel_offset = -1.0 * (is_pos_near ? length_pos : length_neg) / (length_pos + length_neg) + 0.5;
    
            // no offset if the bidirectional point is too far
            if(((is_pos_near ? delta_luma_pos : delta_luma_neg) < 0.0) == (luma_mat[CENTER] < luma_average_start)) pixel_offset = 0.0;
    
            // subpixel antialiasing
            half luma_average_center = 0.0;
            half average_weight_mat[9] = {
                1.0, 2.0, 1.0,
                2.0, 0.0, 2.0,
                1.0, 2.0, 1.0
            };
            for (int i = 0; i < 9; i++) luma_average_center += average_weight_mat[i] * luma_mat[i];
            luma_average_center /= 12.0;
    
            half subpixel_luma_range = clamp(abs(luma_average_center - luma_mat[CENTER]) / luma_range, 0.0, 1.0);
            half subpixel_offset = (-2.0 * subpixel_luma_range + 3.0) * subpixel_luma_range * subpixel_luma_range;
            subpixel_offset = subpixel_offset * subpixel_offset * SUBPIXEL_QUALITY;
    
            // use the max offset between subpixel offset with before
            pixel_offset = max(pixel_offset, subpixel_offset);
    
    
            return SAMPLE_TEXTURE2D_X(_BlitTexture, _sampler_Linear_Clamp, uv + pixel_offset * step_normal);
        }

        // The fragment shader definition.
        half4 frag(Varyings input) : SV_Target
        {
            half4 col = fxaa_3_11(input.texcoord, _Blit_TexelSize);
            return col;
        }

    ENDHLSL

    SubShader
    {
        Tags
        { 
            "RenderType" = "Opaque" "RenderPipeline"="UniversalPipeline"
        }
        LOD 100
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            Name "Pulse_PulseFXAA_Pass"
        
            HLSLPROGRAM

            // Pragmas
            //#pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            ENDHLSL
        }
    }
}
