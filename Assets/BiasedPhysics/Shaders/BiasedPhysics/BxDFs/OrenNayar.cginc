//==============================================================================
// Oren-Nayar BRDF methods
//==============================================================================

#ifndef _BIASED_PHYSICS_OREN_NAYAR_BRDF_H_
#define _BIASED_PHYSICS_OREN_NAYAR_BRDF_H_

#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/BxDFs/BxDFSample.cginc"
#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/Utils/DistributionSamplers.cginc"
#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/Utils/LatLong.cginc"

//==============================================================================
// Evaluate the Oren-Nayar BRDF http://en.wikipedia.org/wiki/Oren-Nayar_reflectance_model 
// Fast version found at http://shaderjvo.blogspot.de/2011/08/van-ouwerkerks-rewrite-of-oren-nayar.html
//==============================================================================
float OrenNayarEvaluate(float3 view, float3 light, float3 normal, float roughness) {

    // Roughness, A and B
    float roughnessSqrd = roughness * roughness;
    float A = 1.0f - 0.5f * roughnessSqrd / (roughnessSqrd + 0.33f);
    float B = 0.45f * roughnessSqrd / (roughnessSqrd + 0.09f);

    // Theta and phi
    float2 cosTheta = saturate(float2(dot(normal, light), dot(normal, view)));
    float2 cosTheta2 = cosTheta * cosTheta;
    float sinTheta = sqrt((1.0f - cosTheta2.x) * (1.0f - cosTheta2.y));
    float3 lightPlane = normalize(light - cosTheta.x * normal);
    float3 viewPlane = normalize(view - cosTheta.y * normal);
    float cosPhi = saturate(dot(lightPlane, viewPlane));

    // Composition
    return (A + B * cosPhi * sinTheta / max(cosTheta.x, cosTheta.y)) / PI;
}

float OrenNayarSpecularity(float roughness) {
    return roughness * 0.1f;
}

BxDFSample SampleOrenNayar(float2 sampleUV, float3 view, float3 normal, float3 tangent, float3 bitangent, float roughness) {
    DistributionSample distSample = CosineDistribution_Sample(sampleUV);
    
    // Return direction in .xyz and weight/PDF in .w
    BxDFSample bxdfSample;
    if (distSample.PDF > 0.00001f) {
        bxdfSample.Direction = tangent * distSample.Direction.x + normal * distSample.Direction.y + bitangent * distSample.Direction.z;
        bxdfSample.Weight = OrenNayarEvaluate(view, bxdfSample.Direction, normal, roughness);
        bxdfSample.PDF = distSample.PDF;
    } else
        bxdfSample.PDF = 0.0f;
    return bxdfSample;
}

half3 SampleOrenNayarIBL(float2 sampleUV, float3 view, float3 normal, float3 tangent, float3 bitangent, float roughness, samplerCUBE ibl) {
    BxDFSample bxdfSample = SampleOrenNayar(sampleUV, view, normal, tangent, bitangent, roughness);
    if (bxdfSample.PDF > 0.0f) {
        half3 L = texCUBElod(ibl, float4(bxdfSample.Direction, 0.0f)).rgb;
        return L * (dot(bxdfSample.Direction, normal) * bxdfSample.Weight / bxdfSample.PDF);
    } else
        return half3(0.0f);
}

float3 OrenNayarIBL(float3 normal, float roughness, sampler2D environmentMap, float environmentMapMipmapCount) {
    float2 uv = Utils_LatLong_DirectionToSphericalUV(normal);

    // Compute glossy miplevel bias from exponent
    float glossyBias = (1.0f - OrenNayarSpecularity(roughness)) * (environmentMapMipmapCount-1.0f);
    
    return tex2Dlod(environmentMap, float4(uv, 0.0f, glossyBias)).rgb;
}

float3 OrenNayarIBL(float roughness, half3 viewDir, half3 normal, 
                    sampler2D rhomap, sampler2D environmentMap, float environmentMapMipmapCount) {
    float u = dot(viewDir, normal);
    float rho = tex2D(rhomap, float2(u, roughness)).r;
    float3 lookupDirection = normal;
    return rho * OrenNayarIBL(lookupDirection, roughness, environmentMap, environmentMapMipmapCount);
}


#endif // _BIASED_PHYSICS_OREN_NAYAR_BRDF_H_
