Shader "Hidden/_Pulse_MultipleCamera"
{
    HLSLINCLUDE
        //#pragma exclude_renderers gles

        // Include neccessary libraries

        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "PulseCommon.hlsl"
        //#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        // Porperties from renderer feature

        TEXTURE2D_X(_GlobalBlitTexture);
        TEXTURE2D_X(_MaskTexture);

        // The fragment shader definition.            
        half4 fragWhite(Varyings input) : SV_Target
        {
            //half4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, _sampler_Linear_Clamp, input.texcoord);

            return 1000;
        }

        half4 fragMask(Varyings input) : SV_Target
        {
            half4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, _sampler_Linear_Clamp, input.texcoord);
            //half4 oldCol = SAMPLE_TEXTURE2D_X(_GlobalBlitTexture, _sampler_Linear_Clamp, input.texcoord);

            return col > 990 ? 0 : 1;
        }

        half4 fragColor(Varyings input) : SV_Target
        {
            half4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, _sampler_Linear_Clamp, input.texcoord);
            half4 oldCol = SAMPLE_TEXTURE2D_X(_GlobalBlitTexture, _sampler_Linear_Clamp, input.texcoord);
            half4 mask = SAMPLE_TEXTURE2D_X(_MaskTexture, _sampler_Linear_Clamp, input.texcoord);

            return mask > 0.5 ? col : oldCol;
        }

        // NO HDR!!!

        half4 fragWhiteNoHDR(Varyings input) : SV_Target
        {
            //half4 col = SAMPLE_TEXTURE2D_X(_GlobalBlitTexture, _sampler_Linear_Clamp, input.texcoord);

            return half4(1, 1, 1, 0);
        }

        half4 fragMaskNoHDR(Varyings input) : SV_Target
        {
            half4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, _sampler_Linear_Clamp, input.texcoord);
            //half4 oldCol = SAMPLE_TEXTURE2D_X(_GlobalBlitTexture, _sampler_Linear_Clamp, input.texcoord);

            return col.a;
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
            Name "Pulse_CameraWhiteColor_Pass"
        
            HLSLPROGRAM

            // Pragmas
            //#pragma target 3.0
            #pragma vertex vert
            #pragma fragment fragWhite

            ENDHLSL
        }

        Pass
        {
            Name "Pulse_CameraMaskColor_Pass"
        
            HLSLPROGRAM

            // Pragmas
            //#pragma target 3.0
            #pragma vertex vert
            #pragma fragment fragMask

            ENDHLSL
        }

        Pass
        {
            Name "Pulse_CameraMainColor_Pass"
        
            HLSLPROGRAM

            // Pragmas
            //#pragma target 3.0
            #pragma vertex vert
            #pragma fragment fragColor

            ENDHLSL
        }

        Pass
        {
            Name "Pulse_CameraWhiteColorNoHDR_Pass"
        
            HLSLPROGRAM

            // Pragmas
            //#pragma target 3.0
            #pragma vertex vert
            #pragma fragment fragWhiteNoHDR

            ENDHLSL
        }

        Pass
        {
            Name "Pulse_CameraMaskColorNoHDR_Pass"
        
            HLSLPROGRAM

            // Pragmas
            //#pragma target 3.0
            #pragma vertex vert
            #pragma fragment fragMaskNoHDR

            ENDHLSL
        }
    }
}
