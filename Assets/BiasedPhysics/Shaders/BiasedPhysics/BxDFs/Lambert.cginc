//==============================================================================
// Lambert BRDF methods
//==============================================================================

#ifndef _PHYSICALLY_BASED_LAMBERT_BRDF_H_
#define _PHYSICALLY_BASED_LAMBERT_BRDF_H_

#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/BxDFs/BxDFSample.cginc"
#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/Utils/DistributionSamplers.cginc"
#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/Utils/LatLong.cginc"

float EvaluateLambert() {
    return 1.0f / PI;
}

float LambertEvaluate(float3 wIn, float3 wOut, float3 wNorm) {
    return 1.0f / PI;
}

float LambertSpecularity() {
    return 0;
}

BxDFSample SampleLambert(float2 sampleUV, float3 normal, float3 tangent, float3 bitangent) {
    DistributionSample distSample = CosineDistribution_Sample(sampleUV);
    
    BxDFSample bxdfSample;
    if (distSample.PDF > 0.0001f) {
        bxdfSample.Direction = tangent * distSample.Direction.x + normal * distSample.Direction.y + bitangent * distSample.Direction.z;
        bxdfSample.Weight = EvaluateLambert();
        bxdfSample.PDF = distSample.PDF;
    } else
        bxdfSample.PDF = 0.0f;
        
    return bxdfSample;
}

half3 SampleLambertIBL(float2 sampleUV, float3 normal, float3 tangent, float3 bitangent, samplerCUBE ibl) {
    BxDFSample bxdfSample = SampleLambert(sampleUV, normal, tangent, bitangent);
    if (bxdfSample.PDF > 0.0f)
        // Multiplying the weight, cos(Light) and 1/PDF cancels out, so we can simplify to just the environment lookup
        // return texCUBElod(_GlobalEnvironment, float4(bxdfSample.Direction, 0.0f)).rgb * bxdfSample.Weight * dot(bxdfSample.Direction, worldNormal) / bxdfSample.PDF;
        return texCUBElod(ibl, float4(bxdfSample.Direction, 0.0f)).rgb;
    else
        return half3(0.0f);
}

float3 LambertIBL(float3 normal, sampler2D environmentMap, float environmentMapMipmapCount) {
    float2 uv = Utils_LatLong_DirectionToSphericalUV(normal);

    float baseMipmap = environmentMapMipmapCount-1.0f;
    
    return tex2Dlod(environmentMap, float4(uv, 0.0f, baseMipmap)).rgb;
}

float3 LambertIBL(float3 viewDir, float3 normal, sampler2D environmentMap, float environmentMapMipmapCount) {
    return LambertIBL(normal, environmentMap, environmentMapMipmapCount);
}

#endif // _PHYSICALLY_BASED_LAMBERT_BRDF_H_
