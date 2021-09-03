using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AquaRenderPassFeature : ScriptableRendererFeature
{
    class AquaRenderPass : ScriptableRenderPass
    {
        public Material aquaMaterial;
        public float opacity;
        
        private const string passName = nameof(AquaRenderPass);
        private RenderTargetIdentifier source;
        private RenderTargetIdentifier destination;

        static readonly int sourceID = Shader.PropertyToID("_SourceTexture");
        static readonly int destinationID = Shader.PropertyToID("_DestinationTexture");
        private string m_ProfilerTag;
        
        public AquaRenderPass(string tag)
        {
            m_ProfilerTag = tag;
        }

        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer buffer, ref RenderingData renderingData)
        {
            RenderTextureDescriptor blitTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;

            buffer.GetTemporaryRT(sourceID, blitTargetDescriptor, FilterMode.Point);
            this.source = new RenderTargetIdentifier(sourceID);
            buffer.GetTemporaryRT(destinationID, blitTargetDescriptor, FilterMode.Point);
            this.destination = new RenderTargetIdentifier(destinationID);

        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            Debug.LogFormat("Execute '{0}'!", m_ProfilerTag);
            CommandBuffer buffer = CommandBufferPool.Get(m_ProfilerTag);
            
            buffer.Blit(source, destination, aquaMaterial);
            buffer.Blit(destination, source);

            //buffer.Blit(this.cameraColorTexture, sourceID);
            //buffer.Blit(sourceID, this.cameraColorTexture, aquaMaterial);

            context.ExecuteCommandBuffer(buffer); //実行
            CommandBufferPool.Release(buffer); //解放
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
        
        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (destinationID != -1)
                cmd.ReleaseTemporaryRT(destinationID);

            if (source == destination && sourceID != -1)
                cmd.ReleaseTemporaryRT(sourceID);
        }
    }


    public Material aquaMaterial;
    [SerializeField, Range(0, 1)] public float opacity = 0f;
    public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;


    private AquaRenderPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        //Debug.Log("Create Aqua Renderer Feature.");

        m_ScriptablePass = new AquaRenderPass(this.name);
        m_ScriptablePass.opacity = this.opacity;
        m_ScriptablePass.renderPassEvent = this.renderPassEvent;

        // Configures where the render pass should be injected.
        //m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        //Debug.Log("Add Aqua Render Passes.");
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


