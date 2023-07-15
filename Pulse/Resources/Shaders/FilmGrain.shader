Shader "Hidden/_Pulse_FilmGrain"
{
    HLSLINCLUDE
        #pragma exclude_renderers gles

        // Include neccessary libraries

        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "PulseCommon.hlsl"
        #include "HashUtils.hlsl"
        //#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        // Porperties from renderer feature

        half _Ibright;
        half _Ishadow;

        // The fragment shader definition.            
        half4 frag(Varyings input) : SV_Target
        {
            half4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, _sampler_Linear_Clamp, input.texcoord);

            half noise = 0;
            half noiseShadow = 0;
            white_noise_2d(input.texcoord.xy * _SinTime.x, noise);
            white_noise_2d(input.texcoord.xy * _SinTime.y, noiseShadow);
            noise = noise * 2 - 1;
            noiseShadow = noiseShadow * 2 - 1;

            // Bright Areas
            noise *= _Ibright;
            col += (col * noise);

            // Shadows

            noiseShadow *= _Ishadow;
            col += noiseShadow;

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
            Name "Pulse_FilmGrain_Pass"
        
            HLSLPROGRAM

            // Pragmas
            //#pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            ENDHLSL
        }
    }
}
