using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CATS.Pulse
{
    public class PulseWhiteBalance : IPulseEffect
    {
        [Range(-1, 1)] public float Temperature = 0f;
        [Range(-1, 1)] public float Tint = 0f;
        [Space]
        public bool useHalfPrecision;

        Material material;

        private void OnEnable()
        {
            name = "PULSE Effects/White Balance";
        }

        public override void OnSetup()
        {
            material = new Material(Shader.Find("Hidden/_Pulse_WhiteBalance"));
        }

        public override void OnRender(ref RTHandle src, ref RTHandle dst)
        {
            material.SetTexture("_BlitTexture", src);

            material.SetFloat("_Temp", Temperature);
            material.SetFloat("_Tint", Tint);

            if (useHalfPrecision)
            {
                Blit(ref src, ref dst, material, material.FindPass("Pulse_WhiteBalanceHalf_Pass"));
            }
            else
            {
                Blit(ref src, ref dst, material, material.FindPass("Pulse_WhiteBalance_Pass"));
            }
        }
    }
}