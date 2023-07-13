#ifndef _PULSE_HASH_UTIL_INCLUDED
#define _PULSE_HASH_UTIL_INCLUDED

    // Hash Functions:
    // (From: https://github.com/blender/blender/blob/main/source/blender/gpu/shaders/common/gpu_shader_common_hash.glsl)

    #define rot(x, k) (((x) << (k)) | ((x) >> (32 - (k))))

    #define final(a, b, c) \
      { \
        c ^= b; \
        c -= rot(b, 14); \
        a ^= c; \
        a -= rot(c, 11); \
        b ^= a; \
        b -= rot(a, 25); \
        c ^= b; \
        c -= rot(b, 16); \
        a ^= c; \
        a -= rot(c, 4); \
        b ^= a; \
        b -= rot(a, 14); \
        c ^= b; \
        c -= rot(b, 24); \
      }

    uint hashutil_uint2(uint kx, uint ky)
    {
        uint a, b, c;
        a = b = c = 0xdeadbeefu + (2u << 2u) + 13u;

        b += ky;
        a += kx;
        final(a, b, c);

        return c;
    }

    ////////////////////////////////////////

    half hashutil_uint2_to_half(uint kx, uint ky)
    {
      return half(hashutil_uint2(kx, ky)) / half(0xFFFFFFFFu);
    }

    float hashutil_uint2_to_float(uint kx, uint ky)
    {
      return float(hashutil_uint2(kx, ky)) / float(0xFFFFFFFFu);
    }

    ////////////////////////////////////////

    float hashutil_half2_to_half(half2 k)
    {
      return hashutil_uint2_to_half(asuint(k.x), asuint(k.y));
    }

    float hashutil_float2_to_float(float2 k)
    {
      return hashutil_uint2_to_float(asuint(k.x), asuint(k.y));
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Noise Functions: 
    // (From: https://github.com/blender/blender/blob/main/source/blender/gpu/shaders/material/gpu_shader_material_tex_white_noise.glsl)

    void white_noise_2d_Half(half2 vec, out half value)
    {
      value = hashutil_half2_to_half(vec.xy);
    }

    void white_noise_2d_Float(float2 vec, out float value)
    {
      value = hashutil_float2_to_float(vec.xy);
    }

#endif