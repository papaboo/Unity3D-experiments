//==============================================================================
// Lambert-Blinn LightModel
//==============================================================================

#ifndef _BIASED_PHYSICS_LIGHTMODEL_LAMBERT_BLINN_H_
#define _BIASED_PHYSICS_LIGHTMODEL_LAMBERT_BLINN_H_

#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/BxDFs/Blinn.cginc"
#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/BxDFs/Lambert.cginc"
#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/LightModels/Globals.cginc"
#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/Utils/Fresnel.cginc"

half4 LightingBiasedPhysics_LambertBlinn(SurfaceOutput surface, half3 lightDir, half3 viewDir, half atten) {
    half fresnel = Fresnel(viewDir, surface.Normal) * surface.Gloss;

    half3 specular = _SpecColor * BlinnEvaluate(viewDir, lightDir, surface.Normal, surface.Specular);
    specular *= fresnel;

    half3 diffuse = surface.Albedo * LambertEvaluate(viewDir, lightDir, surface.Normal);
    diffuse *= (1.0f - _SpecColor * fresnel);

    half3 surfaceColor = diffuse + specular;

	half3 lightColor = _LightColor0.rgb * max(0, dot(surface.Normal, lightDir)) * (atten * 2.0f);

    return half4(lightColor * surfaceColor, surface.Alpha);
}

half3 BiasedPhysics_LambertBlinn_IBL(SurfaceOutput surface, half3 worldNormal, half3 worldReflection) {
    float fresnel = Fresnel(worldReflection, worldNormal) * surface.Gloss;
    
    half3 specular = _SpecColor * BlinnIBL(worldReflection, surface.Specular, _GlobalConvolutedEnvironment, _GlobalConvolutedEnvironmentMipmapCount);
    specular *= fresnel;

    half3 diffuse = surface.Albedo * LambertIBL(worldNormal, _GlobalConvolutedEnvironment, _GlobalConvolutedEnvironmentMipmapCount);
    diffuse *= (1.0f - _SpecColor * fresnel);
    
    return diffuse + specular;
}

half3 BiasedPhysics_LambertBlinn_SampledIBL(SurfaceOutput surface, half3 worldViewDir, half3 worldNormal, sampler2D encodedSamples, int sampleCount) {
    float3 tangent, bitangent;
    CreateTangents(worldNormal, tangent, bitangent);
    
    float3 base = float3(0.0f);
    float3 coat = float3(0.0f);
    float INV_TOTAL_SAMPLE_COUNT = 1.0f / 2048.0f; // 2048 is the width of the random samples texture
    for (int i = 0; i < sampleCount; ++i) {
        float2 sampleUV = DecodeRandomUV(tex2D(encodedSamples, float2(i * INV_TOTAL_SAMPLE_COUNT, 0.5f)));

        // TODO Try out different ways of combining the samples here (Check if the sampling from PBRT really is sound)
        base += SampleLambertIBL(sampleUV, worldNormal, tangent, bitangent, _GlobalEnvironment);        
        // coat = SampleBlinnIBL(sampleUV, worldViewDir, worldNormal, tangent, bitangent, surface.Specular, _GlobalEnvironment);
    }

    float fresnel = Fresnel(worldViewDir, worldNormal) * surface.Gloss;
    
    base *= surface.Albedo * (1.0f - _SpecColor * fresnel);
    coat *= fresnel;

    return (base + coat) / sampleCount;
}

#endif // _BIASED_PHYSICS_LIGHTMODEL_LAMBERT_BLINN_H_
