//==============================================================================
// Blinn LightModel
//==============================================================================

#ifndef _BIASED_PHYSICS_LIGHTMODEL_BLINN_H_
#define _BIASED_PHYSICS_LIGHTMODEL_BLINN_H_

#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/BxDFs/Blinn.cginc"
#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/Utils/Fresnel.cginc"

half4 LightingBiasedPhysics_Blinn(SurfaceOutput surface, half3 lightDir, half3 viewDir, half atten) {
    float3 surfaceColor = surface.Albedo * BlinnEvaluate(viewDir, lightDir, surface.Normal, surface.Specular);

	fixed3 lightColor = _LightColor0.rgb * max(0, dot(surface.Normal, lightDir)) * (atten * 2.0f);

    return half4(lightColor * surfaceColor, surface.Alpha);
}

half3 BiasedPhysics_Blinn_IBL(SurfaceOutput surface, half3 worldNormal, half3 worlReflection) {
    return surface.Albedo * BlinnIBL(worlReflection, surface.Specular, _GlobalConvolutedEnvironment, _GlobalConvolutedEnvironmentMipmapCount);
}

half3 BiasedPhysics_Blinn_SampledIBL(SurfaceOutput surface, half3 worldViewDir, half3 worldNormal, int sampleCount, sampler2D encodedSamples, float invTotalSampleCount) {
    float3 tangent, bitangent;
    CreateTangents(worldNormal, tangent, bitangent);

    float3 iblColor = float3(0.0f);
    // float totalPDF = 0.0f; // TODO Divide by the average PDF to try and get a summed PDF of 1? Might remove some of the energy loss that we see at low roughness values, which looks really great
    for (int i = 0; i < sampleCount; ++i) {
        half2 sampleUV = DecodeRandomUV(tex2D(encodedSamples, float2(i * invTotalSampleCount, 0.5f)));
        BlinnSample bxdfSample = SampleBlinn(sampleUV, worldViewDir, worldNormal, tangent, bitangent, surface.Specular);
        if (bxdfSample.PDF > 0.0f) {
            half3 L = texCUBElod(_GlobalEnvironment, float4(bxdfSample.Direction, 0.0f)).rgb;
            iblColor += L * (dot(bxdfSample.Direction, worldNormal) * bxdfSample.Weight / bxdfSample.PDF);
            // totalPDF += bxdfSample.PDF;
        }
    }
    
    return surface.Albedo * iblColor / sampleCount;
}

#endif // _BIASED_PHYSICS_LIGHTMODEL_BLINN_H_
