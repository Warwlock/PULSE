Shader "Hidden/_Pulse_ChromaticAberration"
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
        half _Hardness;
        half2 _FocalOffset;
        half2 _Radius;
        half3 _ColorOffsets;

        // The fragment shader definition.            
        half4 fragBasic(Varyings input) : SV_Target
        {
            half4 customCol;

            customCol = SAMPLE_TEXTURE2D_X(_BlitTexture, _sampler_Linear_Clamp, input.texcoord);

            half2 direction = input.texcoord - 0.5;
            direction = _Intensity * abs(direction * direction * direction) * 0.1;

            half r = SAMPLE_TEXTURE2D_X(_BlitTexture, _sampler_Linear_Clamp, input.texcoord - direction).r;
            half b = SAMPLE_TEXTURE2D_X(_BlitTexture, _sampler_Linear_Clamp, input.texcoord + direction).b;

            return half4(r, customCol.g, b, customCol.a);
        }

        half4 fragAdvanced(Varyings input) : SV_Target
        {
            half2 pos = input.texcoord - 0.5;
            pos -= _FocalOffset;
            pos *= _Radius;
            pos += 0.5f;

            half2 d = pos - 0.5f;
            half intensity = saturate(pow(abs(length(pos - 0.5f)), _Hardness)) * _Intensity;

            half4 col = 1.0f;
            half2 redUV = input.texcoord + (d * _ColorOffsets.r * _Blit_TexelSize.xy) * intensity;
            half2 greenUV = input.texcoord + (d * _ColorOffsets.g * _Blit_TexelSize.xy) * intensity;
            half2 blueUV = input.texcoord + (d * _ColorOffsets.b * _Blit_TexelSize.xy) * intensity;

            col.r = SAMPLE_TEXTURE2D_X(_BlitTexture, _sampler_Linear_Clamp, redUV).r;
            col.g = SAMPLE_TEXTURE2D_X(_BlitTexture, _sampler_Linear_Clamp, greenUV).g;
            col.b = SAMPLE_TEXTURE2D_X(_BlitTexture, _sampler_Linear_Clamp, blueUV).b;

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
            Name "Pulse_BasicCA_Pass"
        
            HLSLPROGRAM

            // Pragmas
            //#pragma target 3.0
            #pragma vertex vert
            #pragma fragment fragBasic

            ENDHLSL
        }

        Pass
        {
            Name "Pulse_AdvancedCA_Pass"
        
            HLSLPROGRAM

            // Pragmas
            //#pragma target 3.0
            #pragma vertex vert
            #pragma fragment fragAdvanced

            ENDHLSL
        }
    }
}
