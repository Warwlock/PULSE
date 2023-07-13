Shader "Hidden/_Pulse_ToneMapping"
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

        // Tumblin Rushmeier Properties
        half _Ldmax;
        half _Cmax;
        half _lumChangeRate;

        // Reinhard Extended Properties
        half _Pwhite;

        // Hable Properties
        half _A, _B, _C, _D, _E, _F, _W;

        //Uchimura Porperties
        half _P, _a, _m, _l, _c, _b;

        ////////////////////////////////////////////////////////////////////////

        TEXTURE2D_X(_AvrLumTexture);

        // Luminance based tonemappers

        half4 toneMap_TumblinRushmeier(Varyings input) : SV_Target
        {
            half4 col;
            col = SAMPLE_TEXTURE2D_X(_AvrLumTexture, _sampler_Linear_Clamp, input.texcoord);
            half Lin = Luminance(col);

            half Lavg = Luminance(SAMPLE_TEXTURE2D_LOD(_AvrLumTexture, _sampler_Linear_Clamp, input.texcoord, 10));
            half newLavg = Lavg / 100 + 0.2;
            Lavg = lerp(newLavg, Lavg, _lumChangeRate);

            half logLrw = log10(Lavg) + 0.84;
            half alphaRw = 0.4 * logLrw + 2.92;
            half betaRw = -0.4 * logLrw * logLrw - 2.584 * logLrw + 2.0208;
            half Lwd = _Ldmax / sqrt(_Cmax);
            half logLd = log10(Lwd) + 0.84;
            half alphaD = 0.4 * logLd + 2.92;
            half betaD = -0.4 * logLd * logLd - 2.584 * logLd + 2.0208;
            half Lout = pow(abs(Lin), alphaRw / alphaD) / _Ldmax * pow(10.0, (betaRw - betaD) / alphaD) - (1.0 / _Cmax);

            half3 Cout = (col / Lin * Lout).xyz;

            return half4(saturate(Cout), col.a);
        }

        half4 toneMap_Reinhard(Varyings input) : SV_Target
        {
            half4 col;
            col = SAMPLE_TEXTURE2D_X(_BlitTexture, _sampler_Linear_Clamp, input.texcoord);

            half LumIn = Luminance(col);
            half LumOut = LumIn / (1 + LumIn);

            half3 Cout = (col / LumIn * LumOut).xyz;

            return half4(saturate(Cout), col.a);
        }

        half4 toneMap_ReinhardExtended(Varyings input) : SV_Target
        {
            half4 col;
            col = SAMPLE_TEXTURE2D_X(_BlitTexture, _sampler_Linear_Clamp, input.texcoord);

            half LumIn = Luminance(col);
            half LumOut = (LumIn * (1.0 + LumIn / (_Pwhite * _Pwhite))) / (1.0 + LumIn);

            half3 Cout = (col / LumIn * LumOut).xyz;

            return half4(saturate(Cout), col.a);
        }

        // Hable (Uncharted 2) Tonemap

        half HableTonemap(half x)
        {
            return ((x * (_A * x + _C * _B) + _D * _E) / (x * (_A * x + _B) + _D * _F)) - _E / _F;
        }

        half3 HableTonemap3(half3 x)
        {
            return ((x * (_A * x + _C * _B) + _D * _E) / (x * (_A * x + _B) + _D * _F)) - _E / _F;
        }

        // Color based tonemappers (filmic tonemappers)

        half4 toneMap_Hable(Varyings input) : SV_Target
        {
            half4 col;
            col = SAMPLE_TEXTURE2D_X(_BlitTexture, _sampler_Linear_Clamp, input.texcoord);
            
            half Cexposure = 2;
            
            half whiteScale = 1 / HableTonemap(_W);
            half3 current = Cexposure * HableTonemap3(col.xyz);

            half3 Cout = current * whiteScale;

            return half4(saturate(Cout), col.a);
        }

        half4 toneMap_Uchimura(Varyings input) : SV_Target
        {
            half4 col;
            col = SAMPLE_TEXTURE2D_X(_BlitTexture, _sampler_Linear_Clamp, input.texcoord);
            
            half l0 = ((_P - _m) * _l) / _a;
            half S0 = _m + l0;
            half S1 = _m + _a * l0;
            half C2 = (_a * _P) / (_P - S1);
            half CP = -C2 / _P;

            half3 w0 = 1.0f - smoothstep(half3(0.0f, 0.0f, 0.0f), half3(_m, _m, _m), col.xyz);
            half3 w2 = step(half3(_m + l0, _m + l0, _m + l0), col.xyz);
            half3 w1 = half3(1.0f, 1.0f, 1.0f) - w0 - w2;

            half3 T = _m * pow(abs(col.xyz / _m), _c) + _b;
            half3 S = _P - (_P - S1) * exp(CP * (col.xyz - S0));
            half3 L = _m + _a * (col.xyz - _m);

            half3 Cout = T * w0 + L * w1 + S * w2;

            return half4(saturate(Cout), col.a);
        }

        half4 toneMap_NarkowiczACES(Varyings input) : SV_Target
        {
            half4 col;
            col = SAMPLE_TEXTURE2D_X(_BlitTexture, _sampler_Linear_Clamp, input.texcoord);

            half3 Cout = ((col * (2.51f * col + 0.03f)) / (col * (2.43f * col + 0.59f) + 0.14f)).xyz;

            return half4(saturate(Cout), col.a);
        }

        // HillAces (used float precision beacuse ong deciaml place, half probably wont work)
        // Not suitable for mobile environent because of high precision

        float4 toneMap_HillACES(Varyings input) : SV_Target
        {
            float4 col;
            col = SAMPLE_TEXTURE2D_X(_BlitTexture, _sampler_Linear_Clamp, input.texcoord);

            float3 Cout = mul(ACESInputMat, col.xyz);
            Cout = RRTAndODTFit(Cout);

            Cout = mul(ACESOutputMat, Cout);

            return float4(saturate(Cout), col.a);
        }

        // Low precision for mobile platforms, not accurate and probably results high error rate

        half4 toneMap_HillACESHalf(Varyings input) : SV_Target
        {
            half4 col;
            col = SAMPLE_TEXTURE2D_X(_BlitTexture, _sampler_Linear_Clamp, input.texcoord);

            half3 Cout = mul(ACESInputMatHalf, col.xyz);
            Cout = RRTAndODTFitHalf(Cout);

            Cout = mul(ACESOutputMatHalf, Cout);

            return half4(saturate(Cout), col.a);
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
            Name "Pulse_TumblinRushmeier_Pass"
        
            HLSLPROGRAM

            // Pragmas
            //#pragma target 3.0
            #pragma vertex vert
            #pragma fragment toneMap_TumblinRushmeier

            ENDHLSL
        }

        Pass
        {
            Name "Pulse_Reinhard_Pass"
        
            HLSLPROGRAM

            // Pragmas
            //#pragma target 3.0
            #pragma vertex vert
            #pragma fragment toneMap_Reinhard

            ENDHLSL
        }

        Pass
        {
            Name "Pulse_ReinhardExtended_Pass"
        
            HLSLPROGRAM

            // Pragmas
            //#pragma target 3.0
            #pragma vertex vert
            #pragma fragment toneMap_ReinhardExtended

            ENDHLSL
        }

        Pass
        {
            Name "Pulse_Hable_Pass"
        
            HLSLPROGRAM

            // Pragmas
            //#pragma target 3.0
            #pragma vertex vert
            #pragma fragment toneMap_Hable

            ENDHLSL
        }

        Pass
        {
            Name "Pulse_Uchimura_Pass"
        
            HLSLPROGRAM

            // Pragmas
            //#pragma target 3.0
            #pragma vertex vert
            #pragma fragment toneMap_Uchimura

            ENDHLSL
        }

        Pass
        {
            Name "Pulse_NarkowiczACES_Pass"
        
            HLSLPROGRAM

            // Pragmas
            //#pragma target 3.0
            #pragma vertex vert
            #pragma fragment toneMap_NarkowiczACES

            ENDHLSL
        }

        Pass
        {
            Name "Pulse_HillACES_Pass"
        
            HLSLPROGRAM

            // Pragmas
            //#pragma target 3.0
            #pragma vertex vert
            #pragma fragment toneMap_HillACES

            ENDHLSL
        }

        Pass
        {
            Name "Pulse_HillACESHalf_Pass"
        
            HLSLPROGRAM

            // Pragmas
            //#pragma target 3.0
            #pragma vertex vert
            #pragma fragment toneMap_HillACESHalf

            ENDHLSL
        }
    }
}
