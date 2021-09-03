

void MainLight_float (
    float3 WorldPos, out float3 Direction, out float3 Color,
    out float DistanceAtten, out float ShadowAtten){

#ifdef SHADERGRAPH_PREVIEW
    Direction = normalize(float3(1,1,-0.4));
    Color = float4(1,1,1,1);
    DistanceAtten = 1;
    ShadowAtten = 1;
#else
    //float4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
    //Light mainLight = GetMainLight(shadowCoord);
    Light mainLight = GetMainLight();
	Direction = mainLight.direction;
	Color = mainLight.color;
	DistanceAtten = mainLight.distanceAttenuation;
	ShadowSamplingData shadowSamplingData = GetMainLightShadowSamplingData();
	half4 shadowParams = GetMainLightShadowParams();
	ShadowAtten = SampleShadowmap(TEXTURE2D_ARGS(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture), TransformWorldToShadowCoord(WorldPos), shadowSamplingData, shadowParams, false);

    /*
    Direction = mainLight.direction;
    Color = mainLight.color;
    DistanceAtten = mainLight.distanceAttenuation;
    ShadowAtten = mainLight.shadowAttenuation;
    */
#endif

}