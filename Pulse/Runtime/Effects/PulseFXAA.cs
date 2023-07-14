using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CATS.Pulse
{
    public class PulseFXAA : IPulseEffect
    {
        Material material;

        private void OnEnable()
        {
            name = "PULSE Effects/FXAA";
        }

        public override void OnSetup()
        {
            material = new Material(Shader.Find("Hidden/_Pulse_PulseFXAA"));
        }

        public override void OnRender(ref RTHandle src, ref RTHandle dst)
        {
            material.SetTexture("_BlitTexture", src);

            Vector4 texelSize = new Vector4(1f / src.rt.width, 1f / src.rt.height, src.rt.width, src.rt.height);
            material.SetVector("_Blit_TexelSize", texelSize);

            Blit(ref src, ref dst, material);
        }
    }
}