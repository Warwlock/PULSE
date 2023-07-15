#ifndef _PULSE_COMMON_INCLUDED
#define _PULSE_COMMON_INCLUDED

    // Blit texture
    SAMPLER(_sampler_Linear_Clamp);
    SAMPLER(_sampler_Linear_Repeate);
    TEXTURE2D_X(_BlitTexture);
    TEXTURE2D(_MotionVectorTexture);

    half4 _Blit_TexelSize;

    // Common definitions:

    struct Attributes
    {
        uint vertexID : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 texcoord   : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };
            
    Varyings vert(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

        #if SHADER_API_GLES
            float4 pos = input.positionOS;
            float2 uv  = input.uv;
        #else
            float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
            float2 uv  = GetFullScreenTriangleTexCoord(input.vertexID);
        #endif

        output.positionCS = pos;
        output.texcoord   = uv; //* _BlitScaleBias.xy + _BlitScaleBias.zw; //Because of "_BlitScaleBias" cant use Blit.hlsl
        return output;
    }

    // White balance from Unity Shadergraph

    void WhiteBalanceFloat(float3 In, float Temperature, float Tint, out float3 Out)
    {
        // Range ~[-1.67;1.67] works best
        float t1 = Temperature * 10 / 6;
        float t2 = Tint * 10 / 6;

        // Get the CIE xy chromaticity of the reference white point.
        // Note: 0.31271 = x value on the D65 white point
        float x = 0.31271 - t1 * (t1 < 0 ? 0.1 : 0.05);
        float standardIlluminantY = 2.87 * x - 3 * x * x - 0.27509507;
        float y = standardIlluminantY + t2 * 0.05;

        // Calculate the coefficients in the LMS space.
        float3 w1 = float3(0.949237, 1.03542, 1.08728); // D65 white point

        // CIExyToLMS
        float Y = 1;
        float X = Y * x / y;
        float Z = Y * (1 - x - y) / y;
        float L = 0.7328 * X + 0.4296 * Y - 0.1624 * Z;
        float M = -0.7036 * X + 1.6975 * Y + 0.0061 * Z;
        float S = 0.0030 * X + 0.0136 * Y + 0.9834 * Z;
        float3 w2 = float3(L, M, S);

        float3 balance = float3(w1.x / w2.x, w1.y / w2.y, w1.z / w2.z);

        float3x3 LIN_2_LMS_MAT = {
            3.90405e-1, 5.49941e-1, 8.92632e-3,
            7.08416e-2, 9.63172e-1, 1.35775e-3,
            2.31082e-2, 1.28021e-1, 9.36245e-1
        };

        float3x3 LMS_2_LIN_MAT = {
            2.85847e+0, -1.62879e+0, -2.48910e-2,
            -2.10182e-1,  1.15820e+0,  3.24281e-4,
            -4.18120e-2, -1.18169e-1,  1.06867e+0
        };

        float3 lms = mul(LIN_2_LMS_MAT, In);
        lms *= balance;
        Out = mul(LMS_2_LIN_MAT, lms);
    }

    void WhiteBalanceHalf(half3 In, half Temperature, half Tint, out half3 Out)
    {
        // Range ~[-1.67;1.67] works best
        half t1 = Temperature * 10 / 6;
        half t2 = Tint * 10 / 6;

        // Get the CIE xy chromaticity of the reference white point.
        // Note: 0.31271 = x value on the D65 white point
        half x = 0.31271 - t1 * (t1 < 0 ? 0.1 : 0.05);
        half standardIlluminantY = 2.87 * x - 3 * x * x - 0.27509507;
        half y = standardIlluminantY + t2 * 0.05;

        // Calculate the coefficients in the LMS space.
        half3 w1 = half3(0.949237, 1.03542, 1.08728); // D65 white point

        // CIExyToLMS
        half Y = 1;
        half X = Y * x / y;
        half Z = Y * (1 - x - y) / y;
        half L = 0.7328 * X + 0.4296 * Y - 0.1624 * Z;
        half M = -0.7036 * X + 1.6975 * Y + 0.0061 * Z;
        half S = 0.0030 * X + 0.0136 * Y + 0.9834 * Z;
        half3 w2 = half3(L, M, S);

        half3 balance = half3(w1.x / w2.x, w1.y / w2.y, w1.z / w2.z);

        half3x3 LIN_2_LMS_MAT = {
            3.90405e-1, 5.49941e-1, 8.92632e-3,
            7.08416e-2, 9.63172e-1, 1.35775e-3,
            2.31082e-2, 1.28021e-1, 9.36245e-1
        };

        half3x3 LMS_2_LIN_MAT = {
            2.85847e+0, -1.62879e+0, -2.48910e-2,
            -2.10182e-1,  1.15820e+0,  3.24281e-4,
            -4.18120e-2, -1.18169e-1,  1.06867e+0
        };

        half3 lms = mul(LIN_2_LMS_MAT, In);
        lms *= balance;
        Out = mul(LMS_2_LIN_MAT, lms);
    }

    // ACES tone map functions

    static const float3x3 ACESInputMat =
    {
        {0.59719, 0.35458, 0.04823},
        {0.07600, 0.90834, 0.01566},
        {0.02840, 0.13383, 0.83777}
    };

    static const float3x3 ACESOutputMat =
    {
        { 1.60475, -0.53108, -0.07367},
        {-0.10208,  1.10813, -0.00605},
        {-0.00327, -0.07276,  1.07602}
    };

    float3 RRTAndODTFit(float3 v) {
        float3 a = v * (v + 0.0245786) - 0.000090537;
        float3 b = v * (0.983729 * v + 0.4329510) + 0.238081;
        return a / b;
    }

    // ACES tonemap functions for mobile, less accurate

    static const half3x3 ACESInputMatHalf =
    {
        {0.59719, 0.35458, 0.04823},
        {0.07600, 0.90834, 0.01566},
        {0.02840, 0.13383, 0.83777}
    };

    static const half3x3 ACESOutputMatHalf =
    {
        { 1.60475, -0.53108, -0.07367},
        {-0.10208,  1.10813, -0.00605},
        {-0.00327, -0.07276,  1.07602}
    };

    half3 RRTAndODTFitHalf(half3 v) {
        half3 a = v * (v + 0.0245786) - 0.000090537;
        half3 b = v * (0.983729 * v + 0.4329510) + 0.238081;
        return a / b;
    }

#endif