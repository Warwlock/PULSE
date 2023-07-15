Shader "Hidden/_Pulse_DirectBlit"
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

        TEXTURE2D(_UnrenderedTexture);
        float _BlendDistance;

        // The fragment shader definition.            
        half4 frag(Varyings input) : SV_Target
        {
            half4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, _sampler_Linear_Clamp, input.texcoord);
            half4 col2 = SAMPLE_TEXTURE2D_X(_UnrenderedTexture, _sampler_Linear_Clamp, input.texcoord);
            return lerp(col2, col, _BlendDistance);
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
            Name "Pulse_DirectBlit_Pass"
        
            HLSLPROGRAM

            // Pragmas
            //#pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            ENDHLSL
        }
    }
}
