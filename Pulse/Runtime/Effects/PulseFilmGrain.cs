using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using UnityEngine;
using UnityEngine.Rendering;

public class PulseFilmGrain : IPulseEffect
{
    [Range(0, 1)] public float Intensity = 0.5f;
    [Range(0, 1)] public float ShadowIntensity = 0.2f;

    Material material;
    
    private void OnEnable()
    {
        name = "PULSE Effects/Film Grain";
    }

    public override void OnSetup()
    {
        material = new Material(Shader.Find("Hidden/_Pulse_FilmGrain"));
    }

    public override void OnRender(ref RTHandle src, ref RTHandle dst)
    {
        material.SetTexture("_BlitTexture", src);

        material.SetFloat("_Ibright", Intensity / 4);
        material.SetFloat("_Ishadow", ShadowIntensity / 40);

        Blit(ref src, ref dst, material);
    }
}