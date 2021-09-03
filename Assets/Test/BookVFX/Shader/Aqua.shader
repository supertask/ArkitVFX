Shader "ExtraPostProcess/Aqua"
{
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
