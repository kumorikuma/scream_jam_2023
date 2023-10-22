
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TestFSFeature : ScriptableRendererFeature {
    class TestFSPass : ScriptableRenderPass {
        private RenderTargetIdentifier source { get; set; }
        private RenderTargetHandle destination { get; set; }
        public Material outlineMaterial = null;
        RenderTargetHandle temporaryColorTexture;

        public void Setup(RenderTargetIdentifier source, RenderTargetHandle destination) {
            this.source = source;
            this.destination = destination;
        }

        public TestFSPass(Material outlineMaterial) {
            this.outlineMaterial = outlineMaterial;
        }



        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in an performance manner.
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {

        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            CommandBuffer cmd = CommandBufferPool.Get("_OutlinePass");

            RenderTextureDescriptor opaqueDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDescriptor.depthBufferBits = 0;

            if (destination == RenderTargetHandle.CameraTarget) {
                cmd.GetTemporaryRT(temporaryColorTexture.id, opaqueDescriptor, FilterMode.Point);
                Blit(cmd, source, temporaryColorTexture.Identifier(), outlineMaterial, 0);
                Blit(cmd, temporaryColorTexture.Identifier(), source);

            } else Blit(cmd, source, destination.Identifier(), outlineMaterial, 0);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        /// Cleanup any allocated resources that were created during the execution of this render pass.
        public override void FrameCleanup(CommandBuffer cmd) {

            if (destination == RenderTargetHandle.CameraTarget)
                cmd.ReleaseTemporaryRT(temporaryColorTexture.id);
        }
    }

    [System.Serializable]
    public class OutlineSettings {
        public Material outlineMaterial = null;
    }

    public OutlineSettings settings = new OutlineSettings();
    TestFSPass testFSPass;
    RenderTargetHandle outlineTexture;
    /// <summary>
    /// An injection point for the full screen pass.
    /// </summary>
    public RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingTransparents;
    /// <summary>
    /// One or more requirements for pass. Based on chosen flags certain passes will be added to the pipeline.
    /// </summary>
    public ScriptableRenderPassInput requirements = ScriptableRenderPassInput.Color;
    private bool requiresColor;
    private bool injectedBeforeTransparents;

    public override void Create() {
        testFSPass = new TestFSPass(settings.outlineMaterial) {
            renderPassEvent = injectionPoint
        };
        outlineTexture.Init("_OutlineTexture");


        // This copy of requirements is used as a parameter to configure input in order to avoid copy color pass
        ScriptableRenderPassInput modifiedRequirements = requirements;
        requiresColor = (requirements & ScriptableRenderPassInput.Color) != 0;
        injectedBeforeTransparents = injectionPoint <= RenderPassEvent.BeforeRenderingTransparents;
        if (requiresColor && !injectedBeforeTransparents)
        {
            // Removing Color flag in order to avoid unnecessary CopyColor pass
            // Does not apply to before rendering transparents, due to how depth and color are being handled until
            // that injection point.
            modifiedRequirements ^= ScriptableRenderPassInput.Color;
        }
        testFSPass.ConfigureInput(modifiedRequirements);
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        if (settings.outlineMaterial == null) {
            Debug.LogWarningFormat("Missing Outline Material");
            return;
        }
        renderer.EnqueuePass(testFSPass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData) {
        testFSPass.Setup(renderer.cameraColorTargetHandle, RenderTargetHandle.CameraTarget);
    }
}