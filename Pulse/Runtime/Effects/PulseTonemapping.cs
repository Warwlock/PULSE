using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PulseTonemapping : IPulseEffect
{
    public enum ToneMappingTypes
    {
        RGBClamping,
        TumblinRushmeier,
        Reinhard,
        ReinhardExtended,
        _,
        Hable,
        Uchimura,
        NarkowiczACES,
        HillACES,
        HillACESHalf
    }

    public ToneMappingTypes toneMapType = ToneMappingTypes.HillACESHalf;
    // Add your variables here
    [Range(0.01f, 200)] public float MaximumDisplayLuminance = 100f;
    [Range(0.01f, 100)] public float MaximumContrast = 50f;
    [Range(0, 1)] public float LuminanceChangeRate = 0.1f;

    [Range(1, 50)] public float WhitePoint = 2f;

    [Range(0, 1)] public float ShoulderStrength = 0.15f;
    [Range(0, 1)] public float LinearStrength = 0.5f;
    [Range(0, 1)] public float LinearAngle = 0.1f;
    [Range(0, 1)] public float ToeStrength = 0.2f;
    [Range(0, 1)] public float ToeNumerator = 0.02f;
    [Range(0, 1)] public float ToeDenominator = 0.3f;
    [Range(0, 50)] public float LinearWhitePoint = 12f;

    [Range(1, 100)] public float MaxBrightness = 1f;
    [Range(0, 5)] public float Contrast = 1f;
    [Range(0, 1)] public float LinearStart = 0.22f;
    [Range(0.01f, 0.99f)] public float LinearLength = 0.4f;
    [Range(1, 3)] public float BlackTightnessShape = 1.33f;
    [Range(0, 1)] public float BlackTightnessOffset = 0f;

    Material material;

    RTHandle avrLumTexture;
    
    private void OnEnable()
    {
        name = "PULSE Effects/Tonemapping";
    }

    public override void OnTextureInitialize(CameraData cameraData)
    {
        if (toneMapType == ToneMappingTypes.TumblinRushmeier)
        {
            var colorCopyDescriptor = cameraData.cameraTargetDescriptor;

            RenderTextureDescriptor desc = new RenderTextureDescriptor();
            desc.depthBufferBits = (int)DepthBits.None;
            desc.msaaSamples = 1;
            desc.width = colorCopyDescriptor.width;
            desc.height = colorCopyDescriptor.height;
            desc.graphicsFormat = colorCopyDescriptor.graphicsFormat;
            desc.colorFormat = colorCopyDescriptor.colorFormat;
            desc.dimension = colorCopyDescriptor.dimension;
            desc.useMipMap = true;
            desc.autoGenerateMips = true;
            desc.mipCount = 10;

            AllocateTemporaryTexture(ref avrLumTexture, desc, name: "_AverageLuminanceTexture");
        }
    }

    public override void Dispose()
    {
        avrLumTexture?.Release();
    }

    public override void OnSetup()
    {
        material = new Material(Shader.Find("Hidden/_Pulse_ToneMapping"));
    }

    public override void OnRender(ref RTHandle src, ref RTHandle dst)
    {
        if (toneMapType == ToneMappingTypes.TumblinRushmeier)
        {
            material.SetFloat("_Ldmax", MaximumDisplayLuminance);
            material.SetFloat("_Cmax", MaximumContrast);
            material.SetFloat("_lumChangeRate", LuminanceChangeRate);
            material.SetTexture("_AvrLumTexture", avrLumTexture);

            Blit(ref src, ref avrLumTexture);
            Blit(ref avrLumTexture, ref dst, material, material.FindPass("Pulse_TumblinRushmeier_Pass"));
        }

        if (toneMapType == ToneMappingTypes.Reinhard)
        {
            material.SetTexture("_BlitTexture", src);

            Blit(ref src, ref dst, material, material.FindPass("Pulse_Reinhard_Pass"));
        }

        if (toneMapType == ToneMappingTypes.ReinhardExtended)
        {
            material.SetTexture("_BlitTexture", src);
            material.SetFloat("_Pwhite", WhitePoint);

            Blit(ref src, ref dst, material, material.FindPass("Pulse_ReinhardExtended_Pass"));
        }

        if (toneMapType == ToneMappingTypes.Hable)
        {
            material.SetTexture("_BlitTexture", src);
            material.SetFloat("_A", ShoulderStrength);
            material.SetFloat("_B", LinearStrength);
            material.SetFloat("_C", LinearAngle);
            material.SetFloat("_D", ToeStrength);
            material.SetFloat("_E", ToeNumerator);
            material.SetFloat("_F", ToeDenominator);
            material.SetFloat("_W", LinearWhitePoint);

            Blit(ref src, ref dst, material, material.FindPass("Pulse_Hable_Pass"));
        }

        if (toneMapType == ToneMappingTypes.Uchimura)
        {
            material.SetTexture("_BlitTexture", src);
            material.SetFloat("_P", MaxBrightness);
            material.SetFloat("_a", Contrast);
            material.SetFloat("_m", LinearStart);
            material.SetFloat("_l", LinearLength);
            material.SetFloat("_c", BlackTightnessShape);
            material.SetFloat("_b", BlackTightnessOffset);

            Blit(ref src, ref dst, material, material.FindPass("Pulse_Uchimura_Pass"));
        }

        if (toneMapType == ToneMappingTypes.NarkowiczACES)
        {
            material.SetTexture("_BlitTexture", src);

            Blit(ref src, ref dst, material, material.FindPass("Pulse_NarkowiczACES_Pass"));
        }

        if (toneMapType == ToneMappingTypes.HillACES)
        {
            material.SetTexture("_BlitTexture", src);

            Blit(ref src, ref dst, material, material.FindPass("Pulse_HillACES_Pass"));
        }

        if (toneMapType == ToneMappingTypes.HillACESHalf)
        {
            material.SetTexture("_BlitTexture", src);

            Blit(ref src, ref dst, material, material.FindPass("Pulse_HillACESHalf_Pass"));
        }
    }
}