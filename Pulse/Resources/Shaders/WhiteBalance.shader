Shader "Hidden/_Pulse_WhiteBalance"
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

        half _Temp;
        half _Tint;

        // The fragment shader definition.            
        float4 frag(Varyings input) : SV_Target
        {
            float4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, _sampler_Linear_Clamp, input.texcoord);
            float3 Cout;

            WhiteBalanceFloat(col.xyz, _Temp, _Tint, Cout);
            Cout = Cout < 0 ? 0 : Cout;

            return float4(Cout, col.a);
        }

        half4 fragHalf(Varyings input) : SV_Target
        {
            half4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, _sampler_Linear_Clamp, input.texcoord);
            half3 Cout;

            WhiteBalanceHalf(col.xyz, _Temp, _Tint, Cout);
            Cout = Cout < 0 ? 0 : Cout;

            return half4(Cout, col.a);
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
            Name "Pulse_WhiteBalance_Pass"
        
            HLSLPROGRAM

            // Pragmas
            //#pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            ENDHLSL
        }

        Pass
        {
            Name "Pulse_WhiteBalanceHalf_Pass"
        
            HLSLPROGRAM

            // Pragmas
            //#pragma target 3.0
            #pragma vertex vert
            #pragma fragment fragHalf

            ENDHLSL
        }
    }
}
