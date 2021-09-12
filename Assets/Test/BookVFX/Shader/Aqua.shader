Shader "ExtraPostProcess/Aqua"
{
    Properties
    {
        [HideInInspector]_MainTex ("Base (RGB)", 2D) = "white" {}
        _NoiseTexture ("Noise", 2D) = "white" {}
        
        _Opacity("Opacity", Range(0, 1)) = 1
        _EdgeColor ("Edge Color", Color) = (0,0,0,1)
        _EdgeContrast("Edge Contrast", Range(0.01, 4)) = 1.2
        _FillColor ("Fill Color", Color) = (1,1,1,1)

        _BlurWidth("Blur Width", Range(0, 2)) = 1.35
        _BlurFrequency("Blur Frequency", Range(0, 1)) = 0.5
        _HueShift("Hue Shift", Range(0, 0.3)) = 0.1

        _Interval("Interval", Range(0.1, 5)) = 1.25
        _Iteration("Iteration", Int) = 20 //[Range(0, 32)] 

    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment
            #include "Aqua.hlsl"
            ENDHLSL
        }
    }
    Fallback Off
}
