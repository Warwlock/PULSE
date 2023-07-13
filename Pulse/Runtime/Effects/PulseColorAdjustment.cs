using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PEColorAdjustment : IPulseEffect
{
    [Range(0, 2)] public float contrast = 1f;
    [Range(-1, 1)] public float brightness = 0f;
    [Range(0, 4)] public float saturation = 1f;
    [Range(-1, 1)] public float hue = 0;
    public Color colorFilter = Color.white;
    public float exposure = 1f;

    Material material;
    
    private void OnEnable()
    {
        name = "PULSE Effects/Color Adjustment";
    }

    public override void OnSetup()
    {
        material = new Material(Shader.Find("Hidden/_Pulse_ColorAdjustment"));
    }

    public override void OnRender(ref RTHandle src, ref RTHandle dst)
    {
        material.SetTexture("_BlitTexture", src);

        material.SetFloat("_Contrast", contrast);
        material.SetFloat("_Saturation", saturation);
        material.SetFloat("_Brightness", brightness);
        material.SetFloat("_Hue", hue);
        material.SetFloat("_Exposure", exposure);
        material.SetColor("_ColorFilter", colorFilter);

        Blit(ref src, ref dst, material);
    }
}
