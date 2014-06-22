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

half3 BiasedPhysics_LambertBlinn_IBL(SurfaceOutput surface, half3 worldViewDir, half3 worldNormal, sampler2D blinnRhomap) {
    float fresnel = Fresnel(worldViewDir, worldNormal) * surface.Gloss;
    
    half3 specular = _SpecColor * BlinnIBL(surface.Specular, worldViewDir, worldNormal, blinnRhomap, _GlobalConvolutedEnvironment, _GlobalConvolutedEnvironmentMipmapCount);
    specular *= fresnel;

    half3 diffuse = surface.Albedo * LambertIBL(worldNormal, _GlobalConvolutedEnvironment, _GlobalConvolutedEnvironmentMipmapCount);
    diffuse *= (1.0f - _SpecColor * fresnel);
    
    return diffuse + specular;
}

half3 BiasedPhysics_LambertBlinn_SampledIBL(SurfaceOutput surface, half3 worldViewDir, half3 worldNormal, int samplesDrawn, sampler2D encodedSamples, float invTotalSampleCount) {
    float3 tangent, bitangent;
    CreateTangents(worldNormal, tangent, bitangent);

    // TODO Try out different ways of combining the samples here (Check if the sampling from PBRT really is sound)
    
    float3 base = float3(0.0f);
    float3 coat = float3(0.0f);

    for (int i = 0; i < samplesDrawn; ++i) {
        float2 sampleUV = DecodeRandomUV(tex2D(encodedSamples, float2(i * invTotalSampleCount, 0.5f)));

        base += SampleLambertIBL(sampleUV, worldNormal, tangent, bitangent, _GlobalEnvironment);
        coat += SampleBlinnIBL(sampleUV, worldViewDir, worldNormal, tangent, bitangent, surface.Specular, _GlobalEnvironment);
    }

    float fresnel = Fresnel(worldViewDir, worldNormal) * surface.Gloss;
    base *= surface.Albedo * (1.0f - _SpecColor * fresnel);
    coat *= _SpecColor * fresnel;

    /*
    float fresnel = Fresnel(worldViewDir, worldNormal) * surface.Gloss;
    half3 coatScale = _SpecColor * fresnel;
    half3 baseScale = surface.Albedo * (1.0f - coatScale);
    
    float coatIntensity = (coatScale.x + coatScale.y + coatScale.z) / 3.0f;
    float baseIntensity = (baseScale.x + baseScale.y + baseScale.z) / 3.0f;
    half baseContribution = baseIntensity / (baseIntensity + coatIntensity);

    int sampleNumber = 0;
    while (sampleNumber < samplesDrawn * baseContribution) {
        float2 sampleUV = DecodeRandomUV(tex2D(encodedSamples, float2(sampleNumber * invTotalSampleCount, 0.5f)));
        
        BxDFSample bxdfSample = SampleLambert(sampleUV, normal, tangent, bitangent);
        

        sampleNumber
    }
    */    

    return (base + coat) / samplesDrawn;
}

#endif // _BIASED_PHYSICS_LIGHTMODEL_LAMBERT_BLINN_H_
