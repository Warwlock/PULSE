using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PulseVignette : IPulseEffect
{
    [Range(0.0f, 5.0f)] public float intensity = 1f;
    [Range(0.0f, 10.0f)] public float roudness = 1f;
    [Range(0.0f, 10.0f)] public float smoothness = 2f;
    public Vector2 offset = Vector2.zero;
    public Vector2 size = Vector2.one;
    public Color color;

    Material material;
    
    private void OnEnable()
    {
        name = "PULSE Effects/Vignette";
    }

    public override void OnSetup()
    {
        material = new Material(Shader.Find("Hidden/_Pulse_Vignette"));
    }

    public override void OnRender(ref RTHandle src, ref RTHandle dst)
    {
        material.SetTexture("_BlitTexture", src);

        material.SetFloat("_Intensity", intensity);
        material.SetFloat("_Roudness", roudness);
        material.SetFloat("_Smoothness", smoothness);
        material.SetVector("_Offset", offset);
        material.SetVector("_Size", size);
        material.SetColor("_Color", color);

        Blit(ref src, ref dst, material);
    }
}