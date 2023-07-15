#ifndef _PULSE_HASH_UTIL_INCLUDED
#define _PULSE_HASH_UTIL_INCLUDED

    // common GLSL hash
    float hash_FractSin(float2 p)
    {
	    return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
    }

    void white_noise_2d(float2 vec, out float value)
    {
      value = hash_FractSin(vec.xy);
    }

#endif