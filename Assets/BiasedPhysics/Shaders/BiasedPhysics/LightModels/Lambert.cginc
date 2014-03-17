//==============================================================================
// Lambert LightModel
//==============================================================================

#ifndef _BIASED_PHYSICS_LIGHTMODEL_LAMBERT_H_
#define _BIASED_PHYSICS_LIGHTMODEL_LAMBERT_H_

#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/BxDFs/Lambert.cginc"
#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/LightModels/Globals.cginc"

half4 LightingBiasedPhysics_Lambert(SurfaceOutput surface, half3 lightDir, half3 viewDir, half atten) {
    half3 diffuse = surface.Albedo * LambertEvaluate(viewDir, lightDir, surface.Normal);

	half3 lightColor = _LightColor0.rgb * max(0, dot(surface.Normal, lightDir)) * (atten * 2.0f);

    return half4(lightColor * diffuse, surface.Alpha);
}

half3 BiasedPhysics_Lambert_IBL(SurfaceOutput surface, half3 worldNormal) {
    return surface.Albedo * LambertIBL(worldNormal, _GlobalConvolutedEnvironment, _GlobalConvolutedEnvironmentMipmapCount);
}

half3 BiasedPhysics_Lambert_SampledIBL(SurfaceOutput surface, half3 worldNormal, int samplesDrawn, sampler2D encodedSamples, float invTotalSampleCount) {
    float3 tangent, bitangent;
    CreateTangents(worldNormal, tangent, bitangent);
    
    float3 iblColor = float3(0.0f);
    float INV_TOTAL_SAMPLE_COUNT = 1.0f / 2048.0f; // 2048 is the width of the random samples texture
    for (int i = 0; i < samplesDrawn; ++i) {
        float2 sampleUV = DecodeRandomUV(tex2D(encodedSamples, float2(i * INV_TOTAL_SAMPLE_COUNT, 0.5f)));
        iblColor += SampleLambertIBL(sampleUV, worldNormal, tangent, bitangent, _GlobalEnvironment);
    }

    return surface.Albedo * iblColor / samplesDrawn;
}

#endif // _BIASED_PHYSICS_LIGHTMODEL_LAMBERT_H_
