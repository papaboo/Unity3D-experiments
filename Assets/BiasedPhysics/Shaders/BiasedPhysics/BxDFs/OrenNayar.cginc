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
    float2 cos_theta = saturate(float2(dot(normal, light), dot(normal, view)));
    float2 cos_theta2 = cos_theta * cos_theta;
    float sin_theta = sqrt((1.0f - cos_theta2.x) * (1.0f - cos_theta2.y));
    float3 light_plane = normalize(light - cos_theta.x * normal);
    float3 view_plane = normalize(view - cos_theta.y * normal);
    float cos_phi = saturate(dot(light_plane, view_plane));

    // Composition
    return (A + B * cos_phi * sin_theta / max(cos_theta.x, cos_theta.y)) / PI;
}

float OrenNayarSpecularity(float roughness) {
    return roughness * 0.1f;
}

BxDFSample SampleOrenNayer(float2 sampleUV, float3 view, float3 normal, float3 tangent, float3 bitangent, float roughness) {
    DistributionSample distSample = CosineDistribution_Sample(sampleUV);
    
    // Return direction in .xyz and weight/PDF in .w
    BxDFSample bxdfSample;
    bxdfSample.Direction = tangent * distSample.Direction.x + normal * distSample.Direction.y + bitangent * distSample.Direction.z;
    if (distSample.PDF > 0.00001f)
        bxdfSample.WeightOverPDF = OrenNayarEvaluate(view, bxdfSample.Direction, normal, roughness) / distSample.PDF;
    else
        bxdfSample.WeightOverPDF = 0.0f;
    return bxdfSample;
}

half3 SampleOrenNayerIBL(float2 sampleUV, float3 view, float3 normal, float3 tangent, float3 bitangent, float roughness, samplerCUBE ibl) {
    BxDFSample bxdfSample = SampleOrenNayer(sampleUV, view, normal, tangent, bitangent, roughness);
    half3 L = texCUBElod(ibl, float4(bxdfSample.Direction, 0.0f)).rgb;
    return L * (dot(bxdfSample.Direction, normal) * bxdfSample.WeightOverPDF);
}

float3 OrenNayarIBL(float3 normal, float roughness, sampler2D environmentMap, float environmentMapMipmaps) {
    float2 uv = Utils_LatLong_DirectionToSphericalUV(normal);

    // Compute glossy miplevel bias from exponent
    float glossyBias = (1.0f - OrenNayarSpecularity(roughness)) * (environmentMapMipmaps-1.0f);
    
    return tex2Dlod(environmentMap, float4(uv, 0.0f, glossyBias)).rgb;
}

#endif // _BIASED_PHYSICS_OREN_NAYAR_BRDF_H_
