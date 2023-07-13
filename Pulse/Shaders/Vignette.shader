Shader "Hidden/_Pulse_Vignette"
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

        half _Intensity;
        half _Roudness;
        half _Smoothness;
        half2 _Offset;
        half2 _Size;
        half3 _Color;

        // The fragment shader definition.            
        half4 frag(Varyings input) : SV_Target
        {
            half4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, _sampler_Linear_Clamp, input.texcoord);

            float2 pos = input.texcoord - 0.5;
            pos *= _Size;
            pos += 0.5;

            half2 d = abs(pos - 0.5 + _Offset) * _Intensity;
            d = pow(saturate(d), _Roudness);
            half vignetteFactor = pow(saturate(1 - dot(d, d)), _Smoothness);

            return half4(lerp(_Color, col.xyz, vignetteFactor), col.a);
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
            Name "Pulse_Vignette_Pass"
        
            HLSLPROGRAM

            // Pragmas
            //#pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            ENDHLSL
        }
    }
}
