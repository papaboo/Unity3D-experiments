//==============================================================================
// Oren-Nayar LightModel
// Requires the float _roughness uniform. 
// TODO Add to surface description? Or not as unity's surface description is a
// hack.
//==============================================================================

#ifndef _BIASED_PHYSICS_LIGHTMODEL_OREN_NAYAR_H_
#define _BIASED_PHYSICS_LIGHTMODEL_OREN_NAYAR_H_

#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/BxDFs/OrenNayar.cginc"
#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/LightModels/Globals.cginc"

float _Roughness; // Roughness of the surface.

half4 LightingBiasedPhysics_OrenNayar(SurfaceOutput surface, half3 lightDir, half3 viewDir, half atten) {
    half3 diffuse = surface.Albedo * OrenNayarEvaluate(viewDir, lightDir, surface.Normal, _Roughness);

	half3 lightColor = _LightColor0.rgb * max(0, dot(surface.Normal, lightDir)) * (atten * 2.0f);

    return half4(lightColor * diffuse, surface.Alpha);
}

half3 BiasedPhysics_OrenNayar_IBL(SurfaceOutput surface, half3 worldNormal) {
    return surface.Albedo * OrenNayarIBL(worldNormal, _Roughness, _GlobalConvolutedEnvironment, _GlobalConvolutedEnvironmentMipmapCount);
}

half3 BiasedPhysics_OrenNayar_IBL(SurfaceOutput surface, half3 worldViewDir, half3 worldNormal, sampler2D rhomap) {
    return surface.Albedo * OrenNayarIBL(_Roughness, worldViewDir, worldNormal, rhomap, _GlobalConvolutedEnvironment, _GlobalConvolutedEnvironmentMipmapCount);
}

half3 BiasedPhysics_OrenNayar_SampledIBL(SurfaceOutput surface, half3 worldViewDir, half3 worldNormal, int samplesDrawn, sampler2D encodedSamples) {
    float3 tangent, bitangent;
    CreateTangents(worldNormal, tangent, bitangent);
    
    float3 iblColor = float3(0.0f);
    float INV_TOTAL_SAMPLE_COUNT = 1.0f / 2048.0f; // 2048 is the width of the random samples texture
    for (int i = 0; i < samplesDrawn; ++i) {
        float2 sampleUV = DecodeRandomUV(tex2D(encodedSamples, float2(i * INV_TOTAL_SAMPLE_COUNT, 0.5f)));

        iblColor += SampleOrenNayarIBL(sampleUV, worldViewDir, worldNormal, tangent, bitangent, _Roughness, _GlobalEnvironment);
    }

    return surface.Albedo * iblColor / samplesDrawn;
}

#endif // _BIASED_PHYSICS_LIGHTMODEL_OREN_NAYAR_H_
