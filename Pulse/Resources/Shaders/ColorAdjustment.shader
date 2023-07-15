Shader "Hidden/_Pulse_ColorAdjustment"
{
    HLSLINCLUDE
        #pragma exclude_renderers gles

        // Include neccessary libraries

        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "PulseCommon.hlsl"
        //#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        // Porperties from renderer feature

        half _Contrast;
        half _Saturation;
        float _Brightness;
        half _Exposure;
        half _Hue;
        half4 _ColorFilter;

        // The fragment shader definition.            
        half4 frag(Varyings input) : SV_Target
        {
            half4 customCol;
            customCol = SAMPLE_TEXTURE2D_X(_BlitTexture, _sampler_Linear_Clamp, input.texcoord);

            customCol = _Contrast * (customCol - 0.5) + 0.5 + _Brightness;
            customCol = lerp(Luminance(customCol), customCol, _Saturation);

            customCol = customCol < 0 ? 0 : customCol;

            customCol = customCol * _ColorFilter;

            half3 hsl = RgbToHsv(customCol.rgb); // Computationally expensive, cheaper version: https://www.vagrearg.org/content/hsvrgb
            hsl.r += _Hue;
            customCol = half4(HsvToRgb(hsl), customCol.a);

            customCol *= _Exposure;

            return customCol;
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
            Name "Pulse_ColorAdjustment_Pass"
        
            HLSLPROGRAM

            // Pragmas
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            ENDHLSL
        }
    }
}
