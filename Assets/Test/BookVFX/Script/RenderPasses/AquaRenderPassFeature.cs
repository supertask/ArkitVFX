using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AquaRenderPassFeature : ScriptableRendererFeature
{
    class AquaRenderPass : ScriptableRenderPass
    {
        public FilterMode filterMode = FilterMode.Bilinear;
        //public Material aquaMaterial;
        public AquaRenderPassFeature.Settings settings;
        
        private RenderTargetIdentifier source;
        private RenderTargetIdentifier destination;
        private RenderTargetIdentifier temp;

        int sourceId = Shader.PropertyToID("_SourceTexture");
        int destinationId = Shader.PropertyToID("_DestinationTexture");
        int temporaryRTId = Shader.PropertyToID("_InputTexture");
        private string m_ProfilerTag;

        static class ShaderIDs
        {
            public static int EffectParams1 = Shader.PropertyToID("_EffectParams1");
            public static int EffectParams2 = Shader.PropertyToID("_EffectParams2");
            public static int EdgeColor = Shader.PropertyToID("_EdgeColor");
            public static int FillColor = Shader.PropertyToID("_FillColor");
            public static int Iteration = Shader.PropertyToID("_Iteration");
            public static int InputTexture = Shader.PropertyToID("_InputTexture");
            public static int NoiseTexture = Shader.PropertyToID("_NoiseTexture");
        }
        
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
            blitTargetDescriptor.depthBufferBits = 0;

            var renderer = renderingData.cameraData.renderer; //NOTE: this.renderer makes a different result

            sourceId = -1;
            this.source = renderer.cameraColorTarget;

            //destinationId = temporaryRTId;
            buffer.GetTemporaryRT(destinationId, blitTargetDescriptor, filterMode);
            this.destination = new RenderTargetIdentifier(destinationId);
            
            //buffer.GetTemporaryRT(temporaryRTId, blitTargetDescriptor, filterMode);
            //this.temp = new RenderTargetIdentifier(temporaryRTId);
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer buffer = CommandBufferPool.Get(m_ProfilerTag);
            this.Render(buffer, ref renderingData);
            context.ExecuteCommandBuffer(buffer); //実行
            CommandBufferPool.Release(buffer); //解放
        }

        private void Render(CommandBuffer buffer, ref RenderingData renderingData)
        {
            var material = settings.aquaMaterial;
            /*
            settings.aquaMaterial.SetVector(ShaderIDs.EffectParams1,
                new Vector4(settings.opacity, settings.interval, settings.blurWidth, bfreq));

            settings.aquaMaterial.SetVector(ShaderIDs.EffectParams2,
                new Vector2(settings.edgeContrast, settings.hueShift));

            settings.aquaMaterial.SetColor(ShaderIDs.EdgeColor, settings.edgeColor);
            settings.aquaMaterial.SetColor(ShaderIDs.FillColor, settings.fillColor);
            settings.aquaMaterial.SetInt(ShaderIDs.Iteration, settings.iteration);
            settings.aquaMaterial.SetTexture(ShaderIDs.NoiseTexture, settings.NoiseTexture);
            */

            buffer.Blit(source, destination, material, -1);
            buffer.Blit(destination, source);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
        
        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (destinationId != -1)
                cmd.ReleaseTemporaryRT(destinationId);

            if (source == destination && sourceId != -1)
                cmd.ReleaseTemporaryRT(sourceId);
        }
    }

    public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    private AquaRenderPass m_ScriptablePass;

    [System.Serializable]
    public class Settings
    {
        public Material aquaMaterial;
        public Texture2D NoiseTexture;

        [SerializeField, Range(0, 1)] public float opacity = 0f;
        [Space]
        [SerializeField] public Color edgeColor = Color.black;
        [SerializeField, Range(0.01f, 4)] public float edgeContrast = 1.2f;
        [Space]
        [SerializeField] public Color fillColor = Color.white;
        [SerializeField, Range(0, 2)] public float blurWidth = 1f;
        [SerializeField, Range(0, 1)] public float blurFrequency = 0.5f;
        [SerializeField, Range(0, 0.3f)] public float hueShift = 0.1f;
        [Space]
        [SerializeField, Range(0.1f, 5)] public float interval = 1f;
        [SerializeField, Range(0, 32)] public int iteration = 20;
    }
    public Settings settings = new Settings();

    /// <inheritdoc/>
    public override void Create()
    {
        //Debug.Log("Create Aqua Renderer Feature.");
        m_ScriptablePass = new AquaRenderPass(this.name);
        //m_ScriptablePass.aquaMaterial = this.aquaMaterial; //DO NOT FORGET
        m_ScriptablePass.filterMode = FilterMode.Point;
        m_ScriptablePass.settings = this.settings;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        //Debug.Log("Add Aqua Render Passes.");
        m_ScriptablePass.renderPassEvent = this.renderPassEvent;
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


