using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CATS.Pulse
{
    public class PulseChromaticAberration : IPulseEffect
    {
        public enum ChromaticAberrationTypes
        {
            Basic,
            Advanced
        }

        public ChromaticAberrationTypes ChromaticAberrationType = ChromaticAberrationTypes.Basic;

        [Range(0.01f, 5)] public float intensity = 1f;
        [Range(0.01f, 5)] public float hardness = 3f;
        public Vector2 focalOffset = new Vector2(0f, 0f);
        public Vector2 radius = new Vector2(8f, 5f);
        public Vector3 channelOffsets = new Vector3(1f, 0f, -1f);

        Material material;

        private void OnEnable()
        {
            name = "PULSE Effects/Chromatic Aberration";
        }

        public override void OnSetup()
        {
            material = new Material(Shader.Find("Hidden/_Pulse_ChromaticAberration"));
        }

        public override void OnRender(ref RTHandle src, ref RTHandle dst)
        {
            if (ChromaticAberrationType == ChromaticAberrationTypes.Basic)
            {
                material.SetTexture("_BlitTexture", src);
                material.SetFloat("_Intensity", intensity);

                Blit(ref src, ref dst, material, material.FindPass("Pulse_BasicCA_Pass"));
            }

            if (ChromaticAberrationType == ChromaticAberrationTypes.Advanced)
            {
                material.SetTexture("_BlitTexture", src);

                Vector4 texelSize = new Vector4(1f / src.rt.width, 1f / src.rt.height, src.rt.width, src.rt.height);
                material.SetVector("_Blit_TexelSize", texelSize);

                material.SetFloat("_Intensity", intensity);
                material.SetFloat("_Hardness", hardness);
                material.SetVector("_FocalOffset", focalOffset);
                material.SetVector("_Radius", radius);
                material.SetVector("_ColorOffsets", channelOffsets);

                Blit(ref src, ref dst, material, material.FindPass("Pulse_AdvancedCA_Pass"));
            }
        }
    }
}