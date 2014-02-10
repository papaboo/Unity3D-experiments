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
    float norm = (exponent + 1.0f) * INV_TWO_PI;
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
    DistributionSample distSample = PowerCosineDistribution_Sample(sampleUV, specularity);
    
    // Return direction in .xyz and weight/PDF in .w
    BxDFSample bxdfSample;
    if (distSample.PDF > 0.00001f) {
        float3 sampleDir = tangent * distSample.Direction.x + normal * distSample.Direction.y + bitangent * distSample.Direction.z;
        bxdfSample.Direction = reflect(view, sampleDir);
        float pdf = distSample.PDF / (4.0f * dot(bxdfSample.Direction, normal));
        bxdfSample.WeightOverPDF = BlinnEvaluate(view, bxdfSample.Direction, normal, specularity) / pdf;
    } else
        bxdfSample.WeightOverPDF = 0.0f;

    return bxdfSample;
}

float3 BlinnIBL(float3 wOut, float specularity, sampler2D environmentMap, float environmentMapMipmaps) {
    float2 uv = Utils_LatLong_DirectionToSphericalUV(wOut);

    // Compute glossy miplevel bias from exponent
    float glossyBias = (1.0f - specularity) * (environmentMapMipmaps-1.0f);
    
    //return tex2Dbias(environmentMap, float4(uv, 0.0f, glossyBias-2.0f)).rgb;
    return tex2Dlod(environmentMap, float4(uv, 0.0f, glossyBias)).rgb;
}

float3 BlinnIBL(float3 viewDir, float3 normal, float specularity, sampler2D environmentMap, float environmentMapMipmaps) {
    float3 wOut = reflect(viewDir, normal);
    return BlinnIBL(wOut, specularity, environmentMap, environmentMapMipmaps);
}

#endif // _BIASED_PHYSICS_BXDF_BLINN_BRDF_H_
