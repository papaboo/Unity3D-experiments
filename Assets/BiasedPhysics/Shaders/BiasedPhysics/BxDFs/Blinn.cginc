//==============================================================================
// Blinn microfacet BRDF methods
//==============================================================================

#ifndef _BIASED_PHYSICS_BXDF_BLINN_BRDF_H_
#define _BIASED_PHYSICS_BXDF_BLINN_BRDF_H_

#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/BxDFs/BxDFSample.cginc"
#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/Utils/DistributionSamplers.cginc"
#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/Utils/LatLong.cginc"

float CookTorranceGeometric(float3 wIn, float3 wOut, float3 wHalf, float3 wNorm) {
    float NdotX = min(dot(wIn, wNorm), dot(wOut, wNorm));
    return min(1.0f, 2.0f * dot(wNorm, wHalf) * NdotX / dot(wIn, wHalf));
}

float BlinnShininess(float specularity) {
    // Same implementation as in Convoluter.ExponentFromMipLevel
    float shininess = pow(2.0f, specularity * 14.0f) - 1;
    return shininess < 0 ? 0 : shininess; // To ensure valid exponents for miplevels below diffuse
}

float BlinnSpecularity(float shininess) {
    // Clamp to 16383 as that is 'perfectly specular'
    // shininess = shininess < 16383.0f ? shininess : 16383.0f;
            
    return log2(shininess + 1.0f) / 14.0f; // divide by 14 because 2^14-1 = 16383 = 'perfectly specular'
}

float Blinn(float3 wHalf, float3 wNorm, float exponent) {
    const float INV_TWO_PI = 1.0f / (2.0f * 3.141592653589793f);
    float absCosTheta = abs(dot(wHalf, wNorm));
    float norm = (exponent + 2.0f) * INV_TWO_PI;
    return norm * pow(absCosTheta, exponent);
}   

float BlinnEvaluate(float3 wIn, float3 wOut, float3 wNorm, float specularity) {
    float3 wHalf = normalize(wIn + wOut);
    float f = 1.0f; // No fresnel
    float g = CookTorranceGeometric(wIn, wOut, wHalf, wNorm);
    float d = Blinn(wHalf, wNorm, BlinnShininess(specularity));
    return (f * d * g) / (4.0f * dot(wIn, wNorm) * dot(wOut, wNorm));
}

BxDFSample SampleBlinn(float2 sampleUV, float3 view, float3 normal, float3 tangent, float3 bitangent, float specularity) {
    float shininess = BlinnShininess(specularity);

    DistributionSample distSample = PowerCosineDistribution_Sample(sampleUV, shininess);
    
    BxDFSample bxdfSample;
    if (distSample.PDF > 0.00001f) {
        float3 halfway = tangent * distSample.Direction.x + normal * distSample.Direction.y + bitangent * distSample.Direction.z;
        bxdfSample.Direction = -reflect(view, halfway);

        if (dot(bxdfSample.Direction, normal) < 0.0f) {
            bxdfSample.PDF = 0.0f;
        } else {
            bxdfSample.PDF = distSample.PDF / abs(4.0f * dot(view, halfway));
            bxdfSample.Weight = BlinnEvaluate(view, bxdfSample.Direction, normal, specularity); // Optimization. We already know the halfway vector here, so there is no point in evaluating it again.
        }
    } else // PDF is very close to zero. Bail out to avoid huge numbers and precision errors
        bxdfSample.PDF = 0.0f;

    return bxdfSample;
}

half3 SampleBlinnIBL(float2 sampleUV, float3 view, float3 normal, float3 tangent, float3 bitangent, float specularity, samplerCUBE ibl) {
    BxDFSample bxdfSample = SampleBlinn(sampleUV, view, normal, tangent, bitangent, specularity);
    if (bxdfSample.PDF > 0.0f) {
        half3 L = texCUBElod(ibl, float4(bxdfSample.Direction, 0.0f)).rgb;
        return L * (dot(bxdfSample.Direction, normal) * bxdfSample.Weight / bxdfSample.PDF);
    } else
        return half3(0.0f, 0.0f, 0.0f);
}

half3 BlinnIBL(float3 wOut, float specularity, sampler2D environmentMap, float environmentMapMipmaps) {
    float2 uv = Utils_LatLong_DirectionToSphericalUV(wOut);

    // Compute glossy miplevel bias from exponent
    float glossyBias = (1.0f - specularity) * (environmentMapMipmaps-1.0f);
    
    return tex2Dlod(environmentMap, float4(uv, 0.0f, glossyBias)).rgb;
}

half3 BlinnIBL(float3 viewDir, float3 normal, float specularity, sampler2D environmentMap, float environmentMapMipmaps) {
    float3 wOut = reflect(viewDir, normal);
    return BlinnIBL(wOut, specularity, environmentMap, environmentMapMipmaps);
}

#endif // _BIASED_PHYSICS_BXDF_BLINN_BRDF_H_
