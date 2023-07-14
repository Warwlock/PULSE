using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace CATS.Pulse
{
    public class PulseGammaCorrection : IPulseEffect
    {
        public float GammaValue = 1;

        Material material;

        private void OnEnable()
        {
            name = "PULSE Effects/Gamma Correction";
        }

        public override void OnSetup()
        {
            material = new Material(Shader.Find("Hidden/_Pulse_GammaCorrection"));
        }

        public override void OnRender(ref RTHandle src, ref RTHandle dst)
        {
            material.SetTexture("_BlitTexture", src);

            material.SetFloat("_Gamma", GammaValue);

            Blit(ref src, ref dst, material);
        }
    }
}