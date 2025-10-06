void MainLight_half(float3 WorldPos, out float3 Direction, out float3 Color, out float DistanceAtten, out float ShadowAtten)
{
    // Default initialization for all output parameters
    Direction = float3(0, 0, 0);
    Color = float3(1, 1, 1);
    DistanceAtten = 1.0;
    ShadowAtten = 1.0;
#if SHADERGRAPH_PREVIEW
    // Keep outputs initialized for preview mode
#else
    Light mainLight = GetMainLight();
    Direction = mainLight.direction;
    Color = mainLight.color;
    DistanceAtten = mainLight.distanceAttenuation;
    
    float4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
    
    // Get the cascade index for this world position
    int cascadeIndex = ComputeCascadeIndex(WorldPos);
    
    // Get shadow sampling data with proper cascade handling
    ShadowSamplingData shadowSamplingData = GetMainLightShadowSamplingData();
    half shadowStrength = GetMainLightShadowStrength();
    
    // Use the cascade-aware shadow sampling
    ShadowAtten = SampleShadowmapFiltered(
        TEXTURE2D_ARGS(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture),
        shadowCoord,
        shadowSamplingData
    );
    
    // Apply shadow strength
    ShadowAtten = lerp(1.0, ShadowAtten, shadowStrength);
#endif
}