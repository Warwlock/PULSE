using System.Drawing;
using System.Drawing.Drawing2D;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PulsePostProcessing : ScriptableRendererFeature
{
    PulseRenderPass renderPass;

    RenderPassEvent passEvent = RenderPassEvent.AfterRenderingPostProcessing;

    RTHandle firstTempTex;
    RTHandle secondTempTex;
    RTHandle finalTempTex;

    public override void Create()
    {
        renderPass = new PulseRenderPass();
        renderPass.renderPassEvent = passEvent;

        //colorAdjustmentPass.Setup(passData);

        //passData.material = new Material(Shader.Find("Hidden/_PulsePostProcessing"));
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(renderPass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        var colorCopyDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        colorCopyDescriptor.depthBufferBits = (int)DepthBits.None;

        RenderingUtils.ReAllocateIfNeeded(ref firstTempTex, colorCopyDescriptor, name: "_FirstTempTexture");
        RenderingUtils.ReAllocateIfNeeded(ref secondTempTex, colorCopyDescriptor, name: "_SecondTempTexture");
        RenderingUtils.ReAllocateIfNeeded(ref finalTempTex, colorCopyDescriptor, name: "_FinalTempTexture");

        PulseVolume[] pulseVolumes = FindObjectsOfType(typeof(PulseVolume)) as PulseVolume[];

        for(int i = 0; i < pulseVolumes.Length; i++)
        {
            pulseVolumes[i].OnTextureInitializeEffects(renderingData.cameraData);
        }

        renderPass.Setup(firstTempTex, secondTempTex, finalTempTex, pulseVolumes, renderingData.cameraData);

        //base.SetupRenderPasses(renderer, renderingData);
    }

    protected override void Dispose(bool disposing)
    {
        firstTempTex?.Release();
        secondTempTex?.Release();
        finalTempTex?.Release();

        PulseVolume[] pulseVolumes = FindObjectsOfType(typeof(PulseVolume)) as PulseVolume[];
        for(int i = 0; i < pulseVolumes.Length; i++)
        {
            pulseVolumes[i].OnDisposeEffects();
        }
    }

    ////////////////////////////////////////////////////////////////////////
    // Render Passes:

    class PulseRenderPass : ScriptableRenderPass
    {
        private Material[] materials;
        private RTHandle firstTempTex;
        private RTHandle secondTempTex;
        private RTHandle finalTempTex;

        PulseVolume[] pulseVolumes;

        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            
        }

        public void Setup(RTHandle firstTempTex, RTHandle secondTempTex, RTHandle finalTempTex,
                            PulseVolume[] pulseVolumes, CameraData cameraData)
        {
            this.pulseVolumes = pulseVolumes;

            for (int i = 0; i < pulseVolumes.Length; i++)
            {
                PulseVolume pv = pulseVolumes[i];
                pv.OnSetupEffects(cameraData);
            }

            this.firstTempTex = firstTempTex;
            this.secondTempTex = secondTempTex;
            this.finalTempTex = finalTempTex;
        }


        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (pulseVolumes == null)
                return;

            int countI = 0;

            for (int i = 0; i < pulseVolumes.Length; i++)
            {
                if (!pulseVolumes[i].AnyEffectActive())
                    continue;
                countI++;
                break;
            }

            if (countI == 0)
                return;

            /*if (materials == null)
            {
                return;
            }*/

            if (renderingData.cameraData.isPreviewCamera)
            {
                return;
            }

            CommandBuffer commandBuffer = CommandBufferPool.Get("PULSE/PulsePostProcessing");

            RTHandle currentTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;

            materials = new Material[pulseVolumes.Length];

            bool useSecondTetxure = false;
            for (int i = 0; i < pulseVolumes.Length; i++)
            {
                if (pulseVolumes[i].disableCamera.Count > 0)
                {
                    if (pulseVolumes[i].disableCamera.Contains(renderingData.cameraData.camera))
                        continue;
                }

                if (!pulseVolumes[i].enabled)
                    continue;
                if (!pulseVolumes[i].IsInsideInBox(renderingData.cameraData))
                    continue;

                if (pulseVolumes[i].Mode != PulseVolume.modeList.Global)
                    Blitter.BlitCameraTexture(commandBuffer, currentTarget, finalTempTex);

                Blitter.BlitCameraTexture(commandBuffer, currentTarget, firstTempTex);
                materials[i] = new Material(Shader.Find("Hidden/_Pulse_DirectBlit"));
                
                materials[i].SetTexture("_UnrenderedTexture", finalTempTex);
                materials[i].SetTexture("_BlitTexture", pulseVolumes[i].EffectNumber() ? firstTempTex : secondTempTex);

                pulseVolumes[i].OnRenderEffects(renderingData.cameraData, commandBuffer, ref firstTempTex, ref secondTempTex, materials[i]);

                CoreUtils.SetRenderTarget(commandBuffer, renderingData.cameraData.renderer.cameraColorTargetHandle);
                CoreUtils.DrawFullScreen(commandBuffer, materials[i]);
            }

            if (useSecondTetxure)
            {
                //Blitter.BlitCameraTexture(commandBuffer, currentTarget, secondTempTex);
                //material.SetTexture("_UnrenderedTexture", secondTempTex);
            }

            //material.SetTexture("_BlitTexture", firstTempTex);

            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);
        }
    }
}


