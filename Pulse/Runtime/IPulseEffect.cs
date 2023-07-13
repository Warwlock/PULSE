using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class IPulseEffect : ScriptableObject
{
    [HideInInspector, SerializeField] public bool enabled = true;
    [HideInInspector, SerializeField] public bool expanded = true;

    [HideInInspector, SerializeField] public CommandBuffer cmd;

    public virtual void OnTextureInitialize(CameraData cameraData)
    {

    }

    public virtual void Dispose()
    {

    }

    public virtual void OnSetup()
    {

    }

    public virtual void OnRender(ref RTHandle src, ref RTHandle dest)
    {

    }

    /// <summary>
    /// Blit the texture from source to destination with a material and desired material pass.
    /// </summary>
    /// <param name="src">Source texture.</param>
    /// <param name="dst">Destination texture.</param>
    /// <param name="material">Material that will be effect the texture.</param>
    /// <param name="MaterialPass">Material pass.</param>
    public void Blit(ref RTHandle src, ref RTHandle dst, Material material, int MaterialPass = 0)
    {
        Blitter.BlitCameraTexture(cmd, src, dst, material, MaterialPass);
    }

    public void Blit(ref RTHandle src, ref RTHandle dst, float mipLevel = 0, bool bilinear = false)
    {
        Blitter.BlitCameraTexture(cmd, src, dst, mipLevel, bilinear);
    }

    public void AllocateTemporaryTexture(
            ref RTHandle handle,
            in RenderTextureDescriptor descriptor,
            FilterMode filterMode = FilterMode.Point,
            TextureWrapMode wrapMode = TextureWrapMode.Repeat,
            bool isShadowMap = false,
            int anisoLevel = 1,
            float mipMapBias = 0,
            string name = "")
    {
        RenderingUtils.ReAllocateIfNeeded(ref handle, descriptor, filterMode, wrapMode, isShadowMap, anisoLevel, mipMapBias, name);
    }

    public void AllocateTemporaryTexture(
            ref RTHandle handle,
            Vector2 scaleFactor,
            in RenderTextureDescriptor descriptor,
            FilterMode filterMode = FilterMode.Point,
            TextureWrapMode wrapMode = TextureWrapMode.Repeat,
            bool isShadowMap = false,
            int anisoLevel = 1,
            float mipMapBias = 0,
            string name = "")
    {
        RenderingUtils.ReAllocateIfNeeded(ref handle, scaleFactor, descriptor, filterMode, wrapMode, isShadowMap, anisoLevel, mipMapBias, name);
    }

    public void AllocateTemporaryTexture(
            ref RTHandle handle,
            ScaleFunc scaleFunc,
            in RenderTextureDescriptor descriptor,
            FilterMode filterMode = FilterMode.Point,
            TextureWrapMode wrapMode = TextureWrapMode.Repeat,
            bool isShadowMap = false,
            int anisoLevel = 1,
            float mipMapBias = 0,
            string name = "")
    {
        RenderingUtils.ReAllocateIfNeeded(ref handle, scaleFunc, descriptor, filterMode, wrapMode, isShadowMap, anisoLevel, mipMapBias, name);
    }
}
