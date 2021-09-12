#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
//#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"

/*
struct Attributes
{
    uint vertexID : SV_VertexID;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 texcoord   : TEXCOORD0;
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings Vertex(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
    output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
    output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
    return output;
}
*/

//TEXTURE2D_X(_InputTexture);
TEXTURE2D_X(_MainTex);
TEXTURE2D(_NoiseTexture);
//SAMPLER(sampler_InputTexture);
SAMPLER(sampler_MainTex);
SAMPLER(sampler_NoiseTexture);

float4 _NoiseTexture_ST;

//sampler2D _MainTex;

float4 _RTHandleScale;
float4 _EffectParams1;
float2 _EffectParams2;
float4 _EdgeColor;
float4 _FillColor;


float _Opacity;
float _EdgeContrast;
float _BlurWidth;
float _BlurFrequency;

float _HueShift;
float _Interval;

uint _Iteration;

//#define OPACITY         _EffectParams1.x
//#define INTERVAL        _EffectParams1.y
//#define BLUR_WIDTH      _EffectParams1.z
//#define BLUR_FREQ       _EffectParams1.w
//#define EDGE_CONTRAST   _EffectParams2.x
//#define HUE_SHIFT       _EffectParams2.y

#define OPACITY         _Opacity
#define INTERVAL        _Interval
#define BLUR_WIDTH      _BlurWidth
#define BLUR_FREQ       exp( (_BlurFrequency - 0.5) * 6 )
//#define BLUR_FREQ       (_BlurFrequency > 0 ? 1 : 0)
#define EDGE_CONTRAST   _EdgeContrast
#define HUE_SHIFT       _HueShift

#define SCREEN_SIZE_ZW float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y)


//
// https://github.com/Unity-Technologies/Graphics/blob/master/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl
//
float2 ClampAndScaleUV(float2 UV, float2 texelSize, float numberOfTexels, float2 scale)
{
    float2 maxCoord = 1.0f - numberOfTexels * texelSize;
    return min(UV, maxCoord) * scale;
}

float2 ClampAndScaleUV(float2 UV, float2 texelSize, float numberOfTexels)
{
    return ClampAndScaleUV(UV, texelSize, numberOfTexels, _RTHandleScale.xy);
}

// This is assuming full screen buffer and half a texel offset for the clamping.
float2 ClampAndScaleUVForBilinear(float2 UV)
{
    return ClampAndScaleUV(UV, SCREEN_SIZE_ZW, 0.5f);
}

//
// Basic math functions
//

float2 Rotate90(float2 v)
{
    return v.yx * float2(-1, 1);
}

//
// Coordinate system conversion
//

// UV to vertically normalized screen coordinates
float2 UV2SC(float2 uv)
{
    float2 p = uv - 0.5;
    p.x *= _ScreenParams.x / _ScreenParams.y;
    return p;
}

// Vertically normalized screen coordinates to UV
float2 SC2UV(float2 p)
{
    p.x *= _ScreenParams.y / _ScreenParams.x;
    return p + 0.5;
}

//
// Texture sampling functions
//

float3 SampleColor(float2 p)
{
    float2 uv = p;
    //float2 uv = ClampAndScaleUVForBilinear(SC2UV(p));
    //float2 uv = ClampAndScaleUVForBilinear(p);
    //return SAMPLE_TEXTURE2D_X(_InputTexture, sampler_InputTexture, uv).rgb;
    return SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, uv).rgb;
    //return tex2D(_MainTex, uv);
}

float SampleLuminance(float2 p)
{
    return Luminance(SampleColor(p));
}

float3 SampleNoise(float2 p)
{
    return SAMPLE_TEXTURE2D(_NoiseTexture, sampler_NoiseTexture, p * _NoiseTexture_ST.xy + _NoiseTexture_ST.zw).rgb;
}

//
// Gradient function
//

float2 GetGradient(float2 p, float freq)
{
    const float2 dx = float2(INTERVAL / 200, 0);
    float ldx = SampleLuminance(p + dx.xy) - SampleLuminance(p - dx.xy);
    float ldy = SampleLuminance(p + dx.yx) - SampleLuminance(p - dx.yx);
    float2 n = (SampleNoise(p * 0.4 * freq).gb - 0.5);
    return float2(ldx, ldy) + n * 0.05;
}

//
// Edge / fill processing functions
//

float ProcessEdge(inout float2 p, float stride)
{
    float2 grad = GetGradient(p, 1);
    float edge = saturate(length(grad) * 10);
    float pattern = SampleNoise(p * 0.8).r;
    p += normalize(Rotate90(grad)) * stride;
    return pattern * edge;
}

float3 ProcessFill(inout float2 p, float stride)
{
    float2 grad = GetGradient(p, BLUR_FREQ);
    p += normalize(grad) * stride;
    float shift = SampleNoise(p * 0.1).r * 2;
    return SampleColor(p) * HsvToRgb(float3(shift, HUE_SHIFT, 1));
}

//
// Fragment shader implementation
//

float4 Fragment(Varyings input) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    // Gradient oriented blur effect
    //return float4(1,0,0,1);
    //return float4(SampleColor(input.uv), 1);
    //return float4(SampleNoise(input.uv), 1);
    //return float4(_ScreenParams.z > 1.0001 ? 1 : 0, 0,0,1);
    //return float4(_ScreenParams.zw,0,1);

    //float2 p = UV2SC(input.texcoord);
    //float2 p = UV2SC(input.uv);
    float2 p = input.uv;

    float2 p_e_n = p;
    float2 p_e_p = p;
    float2 p_c_n = p;
    float2 p_c_p = p;

    const float Stride = 0.04 / _Iteration;

    float  acc_e = 0;
    float3 acc_c = 0;
    float  sum_e = 0;
    float  sum_c = 0;

    for (uint i = 0; i < _Iteration; i++)
    {
        float w_e = 1.5 - (float)i / _Iteration;
        acc_e += ProcessEdge(p_e_n, -Stride) * w_e;
        acc_e += ProcessEdge(p_e_p, +Stride) * w_e;
        sum_e += w_e * 2;

        float w_c = 0.2 + (float)i / _Iteration;
        acc_c += ProcessFill(p_c_n, -Stride * BLUR_WIDTH) * w_c;
        acc_c += ProcessFill(p_c_p, +Stride * BLUR_WIDTH) * w_c * 0.3;
        sum_c += w_c * 1.3;
    }


    // Normalization and contrast

    acc_e /= sum_e;
    acc_c /= sum_c;

    acc_e = saturate((acc_e - 0.5) * EDGE_CONTRAST + 0.5);

    //return float4(sum_e, 0, 0, 1);
    //return float4(sum_c, 0, 0, 1);
    //return float4(acc_c, 1);
    //return float4(acc_e, 0, 0, 1);

    //return float4( (OPACITY >= 1.0 ? 1.0 : 0), 0, 0, 1);

    // Color blending

    float3 rgb_e = lerp(1, _EdgeColor.rgb, _EdgeColor.a * acc_e);
    float3 rgb_f = lerp(1, acc_c, _FillColor.a) * _FillColor.rgb;

    //return float4(rgb_f, 1);

    //uint2 positionSS = input.texcoord * _ScreenParams.xy;
    uint2 positionSS = input.uv * _ScreenParams.xy;
    //uint2 positionSS = input.uv;

    //float4 src = LOAD_TEXTURE2D_X(_InputTexture, positionSS);
    //float4 src = LOAD_TEXTURE2D_X(_MainTex, positionSS);
    float4 src = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, positionSS);
    //return float4(lerp(src.rgb, rgb_e * rgb_f, OPACITY), 1);
    //return float4(OPACITY, 0, 0, 1);

    return float4(lerp(src.rgb, rgb_e * rgb_f, OPACITY), src.a);
}
