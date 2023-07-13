using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PulseMultipleCamera : ScriptableRendererFeature
{
    PulseClearCopyColor clearCopyColorPass;
    PulseCopyMask copyMaskPass;
    PulseApplyMask applyMaskPass;

    RenderPassEvent cccpEvent = RenderPassEvent.BeforeRendering;
    RenderPassEvent cmEvent = RenderPassEvent.AfterRenderingPostProcessing - 10;
    RenderPassEvent amEvent = RenderPassEvent.AfterRenderingPostProcessing + 10;

    [HideInInspector] public Material material;

    RTHandle globalTempTex;
    RTHandle tempTex;
    RTHandle maskTex;

    public override void Create()
    {
        clearCopyColorPass = new PulseClearCopyColor();
        clearCopyColorPass.renderPassEvent = cccpEvent;

        copyMaskPass = new PulseCopyMask();
        copyMaskPass.renderPassEvent = cmEvent;

        applyMaskPass = new PulseApplyMask();
        applyMaskPass.renderPassEvent = amEvent;

        //colorAdjustmentPass.Setup(passData);
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.renderType == CameraRenderType.Overlay)
        {
            renderer.EnqueuePass(clearCopyColorPass);
            renderer.EnqueuePass(copyMaskPass);
            renderer.EnqueuePass(applyMaskPass);
        }
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        var colorCopyDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        colorCopyDescriptor.depthBufferBits = (int)DepthBits.None;

        RenderingUtils.ReAllocateIfNeeded(ref globalTempTex, colorCopyDescriptor, name: "_GlobalTemporaryColorTexture");
        RenderingUtils.ReAllocateIfNeeded(ref tempTex, colorCopyDescriptor, name: "_TemporaryColorTexture");
        RenderingUtils.ReAllocateIfNeeded(ref maskTex, colorCopyDescriptor, name: "_MaskTexture");

        material = new Material(Shader.Find("Hidden/_Pulse_MultipleCamera"));
        clearCopyColorPass.Setup(globalTempTex, material);
        copyMaskPass.Setup(tempTex, maskTex, material);
        applyMaskPass.Setup(tempTex, maskTex, material);

        //base.SetupRenderPasses(renderer, renderingData);
    }

    protected override void Dispose(bool disposing)
    {
        globalTempTex?.Release();
        tempTex?.Release();
        maskTex?.Release();
    }

    ////////////////////////////////////////////////////////////////////////
    // Render Passes:

    class PulseClearCopyColor : ScriptableRenderPass
    {
        private Material material;
        private RTHandle tempTex;

        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            
        }

        public void Setup(RTHandle tempTex, Material material)
        {
            this.material = material;
            this.tempTex = tempTex;
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null)
            {
                return;
            }

            if (renderingData.cameraData.isPreviewCamera)
            {
                return;
            }

            CommandBuffer commandBuffer = CommandBufferPool.Get("PULSE/PulseMultipleCamera");

            RTHandle currentTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;

            //material.SetFloat("_EXAMPLE", passData._Example);

            Blitter.BlitCameraTexture(commandBuffer, currentTarget, tempTex);
            material.SetTexture("_GlobalBlitTexture", tempTex);

            bool isHDR = renderingData.cameraData.isHdrEnabled;
            int passID = isHDR ? material.FindPass("Pulse_CameraWhiteColor_Pass") : material.FindPass("Pulse_CameraWhiteColorNoHDR_Pass");

            CoreUtils.SetRenderTarget(commandBuffer, renderingData.cameraData.renderer.cameraColorTargetHandle);
            CoreUtils.DrawFullScreen(commandBuffer, material, shaderPassId: passID);

            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);
        }
    }

    class PulseCopyMask : ScriptableRenderPass
    {
        private Material material;
        private RTHandle tempTex;
        private RTHandle maskTex;

        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {

        }

        public void Setup(RTHandle tempTex, RTHandle maskTex, Material material)
        {
            this.material = material;
            this.tempTex = tempTex;
            this.maskTex = maskTex;
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null)
            {
                return;
            }

            if (renderingData.cameraData.isPreviewCamera)
            {
                return;
            }

            CommandBuffer commandBuffer = CommandBufferPool.Get("PULSE/PulseMultipleCamera");

            RTHandle currentTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;

            //material.SetFloat("_EXAMPLE", passData._Example);

            Blitter.BlitCameraTexture(commandBuffer, currentTarget, tempTex);
            material.SetTexture("_BlitTexture", tempTex);

            bool isHDR = renderingData.cameraData.isHdrEnabled;
            int passID = isHDR ? material.FindPass("Pulse_CameraMaskColor_Pass") : material.FindPass("Pulse_CameraMaskColorNoHDR_Pass");

            CoreUtils.SetRenderTarget(commandBuffer, maskTex);
            CoreUtils.DrawFullScreen(commandBuffer, material, shaderPassId: passID);

            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);
        }
    }

    class PulseApplyMask : ScriptableRenderPass
    {
        private Material material;
        private RTHandle tempTex;
        private RTHandle maskTex;

        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {

        }

        public void Setup(RTHandle tempTex, RTHandle maskTex, Material material)
        {
            this.material = material;
            this.tempTex = tempTex;
            this.maskTex = maskTex;
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null)
            {
                return;
            }

            if (renderingData.cameraData.isPreviewCamera)
            {
                return;
            }

            CommandBuffer commandBuffer = CommandBufferPool.Get("PULSE/PulseMultipleCamera");

            RTHandle currentTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;

            //material.SetFloat("_EXAMPLE", passData._Example);

            //Blitter.BlitCameraTexture(commandBuffer, currentTarget, tempTex);
            Blitter.BlitCameraTexture(commandBuffer, currentTarget, tempTex);
            material.SetTexture("_BlitTexture", tempTex);
            material.SetTexture("_MaskTexture", maskTex);

            CoreUtils.SetRenderTarget(commandBuffer, renderingData.cameraData.renderer.cameraColorTargetHandle);
            CoreUtils.DrawFullScreen(commandBuffer, material, shaderPassId: material.FindPass("Pulse_CameraMainColor_Pass"));

            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);
        }
    }
}


