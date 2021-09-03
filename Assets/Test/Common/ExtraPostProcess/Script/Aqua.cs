using UnityEngine;
using UnityEngine.Rendering;
using SerializableAttribute = System.SerializableAttribute;

namespace ExtraPostProcessing {

public class Aqua : ImageEffectBase
{
    //public ClampedFloatParameter opacity = new ClampedFloatParameter(0, 0, 1);
    //[Space]
    //public ColorParameter edgeColor = new ColorParameter(Color.black);
    //public ClampedFloatParameter edgeContrast = new ClampedFloatParameter(1.2f, 0.01f, 4);
    //[Space]
    //public ColorParameter fillColor = new ColorParameter(Color.white);
    //public ClampedFloatParameter blurWidth = new ClampedFloatParameter(1, 0, 2);
    //public ClampedFloatParameter blurFrequency = new ClampedFloatParameter(0.5f, 0, 1);
    //public ClampedFloatParameter hueShift = new ClampedFloatParameter(0.1f, 0, 0.3f);
    //[Space]
    //public ClampedFloatParameter interval = new ClampedFloatParameter(1, 0.1f, 5);
    //public ClampedIntParameter iteration = new ClampedIntParameter(20, 4, 32);

    [SerializeField] public Texture2D noiseTexture;
    [Space]
    
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
    [SerializeField, Range(0.1f, 5)] public float interval = 1;
    [SerializeField, Range(4, 32)] public int iteration = 20;

    public static class ShaderIDs
    {
        public static int RTHandleScale = Shader.PropertyToID("_RTHandleScale");
        public static int EffectParams1 = Shader.PropertyToID("_EffectParams1");
        public static int EffectParams2 = Shader.PropertyToID("_EffectParams2");
        public static int EdgeColor = Shader.PropertyToID("_EdgeColor");
        public static int FillColor = Shader.PropertyToID("_FillColor");
        public static int Iteration = Shader.PropertyToID("_Iteration");
        public static int InputTexture = Shader.PropertyToID("_InputTexture");
        public static int NoiseTexture = Shader.PropertyToID("_NoiseTexture");
    }

    private Material _material;

    protected override void Start() {
        base.Start();
        this._material = new Material(this.material);
    }


    protected override void OnRenderImage(RenderTexture src, RenderTexture dst)
    { 

        var bfreq = Mathf.Exp((this.blurFrequency - 0.5f) * 6);

        _material.SetVector(ShaderIDs.RTHandleScale,
            RTHandles.rtHandleProperties.rtHandleScale);

        _material.SetVector(ShaderIDs.EffectParams1,
            new Vector4(opacity, interval,blurWidth, bfreq));
        _material.SetVector(ShaderIDs.EffectParams2,
            new Vector2(edgeContrast, hueShift));
        _material.SetColor(ShaderIDs.EdgeColor, edgeColor);
        _material.SetColor(ShaderIDs.FillColor, fillColor);
        _material.SetInt(ShaderIDs.Iteration, iteration);
        _material.SetTexture(ShaderIDs.InputTexture, src);
        _material.SetTexture(ShaderIDs.NoiseTexture, noiseTexture);

        Graphics.Blit(src, dst, this._material);
    }
}

}