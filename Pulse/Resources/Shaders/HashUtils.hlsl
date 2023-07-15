#ifndef _PULSE_HASH_UTIL_INCLUDED
#define _PULSE_HASH_UTIL_INCLUDED

    // Hash Functions:
    // (From: https://www.shadertoy.com/view/XlGcRh)

    // Hash without Sine
    // https://www.shadertoy.com/view/4djSRW

    half hashwithoutsine12_half(half2 p)
    {
	    half3 p3  = frac(half3(p.xyx) * .1031);
        p3 += dot(p3, p3.yzx + 33.33);
        return frac((p3.x + p3.y) * p3.z);
    }

    float hashwithoutsine12_float(float2 p)
    {
	    float3 p3  = frac(float3(p.xyx) * .1031);
        p3 += dot(p3, p3.yzx + 33.33);
        return frac((p3.x + p3.y) * p3.z);
    }

    void white_noise_2d_Half(half2 vec, out half value)
    {
      value = hashwithoutsine12_half(vec.xy);
    }

    void white_noise_2d_Float(float2 vec, out float value)
    {
      value = hashwithoutsine12_float(vec.xy);
    }

#endif