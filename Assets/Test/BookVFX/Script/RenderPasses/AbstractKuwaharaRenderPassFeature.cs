using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AbstractKuwaharaRenderPassFeature : ScriptableRendererFeature
{
    class AbstractKuwaharaRenderPass : ScriptableRenderPass
    {
        public FilterMode filterMode = FilterMode.Bilinear;
        //public Material aquaMaterial;
        public AbstractKuwaharaRenderPassFeature.Settings settings;
        
        private RenderTargetIdentifier source;
        private RenderTargetIdentifier destination;
        private RenderTargetIdentifier temp;

        int sourceId = Shader.PropertyToID("_SourceTexture");
        int destinationId = Shader.PropertyToID("_DestinationTexture");
        int temporaryRTId = Shader.PropertyToID("_InputTexture");
        private string m_ProfilerTag;
        
        private Matrix4x4 colorMatrix;
        private Material abstractKuwaharaMaterial;
        private Material colorfulFractalMaterial;
        private RenderTexture colorfulFractalTex;

        static class ShaderIDs
        {
            public static int EffectParams1 = Shader.PropertyToID("_EffectParams1");
            public static int EffectParams2 = Shader.PropertyToID("_EffectParams2");
            public static int EdgeColor = Shader.PropertyToID("_EdgeColor");
            public static int FillColor = Shader.PropertyToID("_FillColor");
            public static int Iteration = Shader.PropertyToID("_Iteration");
            public static int InputTexture = Shader.PropertyToID("_InputTexture");
            public static int NoiseTexture = Shader.PropertyToID("_NoiseTexture");

            // Kuwahara Filter
			public static int ColorfulFractalTex = Shader.PropertyToID("_ColorfulFractalTex");
			public static int SobelLineColor = Shader.PropertyToID("_SobelLineColor");
			public static int KuwaharaRadius = Shader.PropertyToID("_KuwaharaRadius");
			public static int SobelDeltaX = Shader.PropertyToID("_SobelDeltaX");
			public static int SobelDeltaY = Shader.PropertyToID("_SobelDeltaY");

            // Colorful Fractal
			public static int ColorMatrix = Shader.PropertyToID("_ColorMatrix");
			public static int FractalTiling = Shader.PropertyToID("_FractalTiling");
			public static int FractalOffsetX = Shader.PropertyToID("_OffsetX");
			public static int FractalOffsetY = Shader.PropertyToID("_OffsetY");
			public static int FractalGain = Shader.PropertyToID("_Gain");
			public static int ScalingTime = Shader.PropertyToID("_ScalingTime");
        }
        
        public AbstractKuwaharaRenderPass(string tag)
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
            this.RenderColorfulFractal(buffer, ref renderingData);

            if (this.abstractKuwaharaMaterial == null) {
                this.abstractKuwaharaMaterial = new Material(this.settings.abstractKuwaharaShader);
            }
			abstractKuwaharaMaterial.SetTexture(ShaderIDs.ColorfulFractalTex, this.colorfulFractalTex);
			if (this.settings.debugColorfulFractal)
				abstractKuwaharaMaterial.EnableKeyword("DEBUG_COLORFUL_FRACTAL");
			else
				abstractKuwaharaMaterial.DisableKeyword("DEBUG_COLORFUL_FRACTAL");

			this.abstractKuwaharaMaterial.SetColor(ShaderIDs.SobelLineColor, settings.sobelLineColor);
			this.abstractKuwaharaMaterial.SetInt(ShaderIDs.KuwaharaRadius, settings.kuwaharaRadius);
			this.abstractKuwaharaMaterial.SetFloat(ShaderIDs.SobelDeltaX, settings.sobelDeltaX);
			this.abstractKuwaharaMaterial.SetFloat(ShaderIDs.SobelDeltaY, settings.sobelDeltaY);

			buffer.Blit(source, destination, abstractKuwaharaMaterial, -1);
            buffer.Blit(destination, source);
        }

        private void RenderColorfulFractal(CommandBuffer buffer, ref RenderingData renderingData)
        {
			var width = Screen.width >> this.settings.colorfulFractalLod;
			var height = Screen.height >> this.settings.colorfulFractalLod;
			if (this.colorfulFractalTex == null ||
                this.colorfulFractalTex.width != width ||
                this.colorfulFractalTex.height != height)
			{
				Debug.Log(string.Format("Init RenderTexture {0}x{1}", width, height));
				Release();
				this.colorfulFractalTex = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
				this.colorfulFractalTex.filterMode = FilterMode.Bilinear;
				this.colorfulFractalTex.wrapMode = TextureWrapMode.Clamp;
				this.colorfulFractalTex.name = "GenerativeFractal RTex";
			}

            if (this.colorfulFractalMaterial == null) {
                this.colorfulFractalMaterial = new Material(this.settings.colorfulFractalShader);
            }
            for(int i = 0; i < 4; i++)
            {
                this.colorMatrix.SetColumn(i, this.settings.colors[i]);
            }
			this.colorfulFractalMaterial.SetMatrix(ShaderIDs.ColorMatrix, colorMatrix);
            this.colorfulFractalMaterial.SetFloat(ShaderIDs.ScalingTime, Time.time * settings.timeShiftScale);
            this.colorfulFractalMaterial.SetVector(ShaderIDs.FractalTiling, this.settings.tiling);
            this.colorfulFractalMaterial.SetVector(ShaderIDs.FractalOffsetX, this.settings.offsetX);
            this.colorfulFractalMaterial.SetVector(ShaderIDs.FractalOffsetY, this.settings.offsetY);
            this.colorfulFractalMaterial.SetVector(ShaderIDs.FractalGain, this.settings.gain);

			Graphics.Blit(null, this.colorfulFractalTex, this.colorfulFractalMaterial);
		}


        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }

		public void Release()
		{
			if (colorfulFractalTex != null)
			{
				colorfulFractalTex.Release();
				colorfulFractalTex = null;
			}
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
    private AbstractKuwaharaRenderPass m_ScriptablePass;

    [System.Serializable]
    public class Settings
    {
        [Header("Abstract Kuwahara")]
        //public Material aquaMaterial;
        public Shader abstractKuwaharaShader;
		public Color sobelLineColor = Color.black;
		public int kuwaharaRadius = 10;
		public float sobelDeltaX = 0.003f;
		public float sobelDeltaY = 0.015f;
        
        
        [Header("Coloful Fractal")]
        //public Material colorfulFractalMaterial;
        public Shader colorfulFractalShader;
        public bool debugColorfulFractal = false;
        //public RenderTexture colorfulFractalTex;
        public int colorfulFractalLod = 7;
		public Color[] colors = new Color[] { new Color(1, 0, 0, 0), new Color(0, 1, 0, 0), new Color(0, 0, 1, 0), new Color(0, 0, 0, 1) };
		public float timeShiftScale = 0.03f;
		public Vector4 tiling = new Vector4(5, 5, 60, 0);
		public Vector4 offsetX = new Vector4(3, 13, 29, 43);
		public Vector4 offsetY = new Vector4(7, 19, 37, 53);
		public Vector4 gain = new Vector4(2, 0.5f, 0, 0);
    }
    public Settings settings = new Settings();

    /// <inheritdoc/>
    public override void Create()
    {
        //Debug.Log("Create Aqua Renderer Feature.");
        m_ScriptablePass = new AbstractKuwaharaRenderPass(this.name);
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


