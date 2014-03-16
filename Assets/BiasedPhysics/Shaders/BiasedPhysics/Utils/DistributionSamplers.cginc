//==============================================================================
// Distribution Samplers.
//==============================================================================

#ifndef _PHYSICALLY_BIASED_UTILS_DISTRIBUTION_SAMPLERS_H_
#define _PHYSICALLY_BIASED_UTILS_DISTRIBUTION_SAMPLERS_H_

#ifndef PI
#define PI 3.14159265359f
#endif // PI

struct DistributionSample {
    float3 Direction;
    float PDF;
};

////////////////////////////////////////////////////////////////////////////////
// Cosine distribution. Useful for mostly diffuse BRDFs such as Lambert 
// and Oren-Nayar.
////////////////////////////////////////////////////////////////////////////////
DistributionSample CosineDistribution_Sample(float2 sampleUV) {
    float cosTheta = sampleUV.x;
    float sinTheta = sqrt(1.0f - cosTheta * cosTheta);
    float phi = 2.0f * PI * sampleUV.y;
    
    DistributionSample dSample;
    dSample.Direction = float3(cos(phi) * sinTheta, cosTheta, sin(phi) * sinTheta);
    dSample.PDF = cosTheta / PI;
    return dSample;
}

float CosineDistribution_PDF(float3 wOut) {
    return abs(wOut.y) / PI;
}

////////////////////////////////////////////////////////////////////////////////
// Power cosine distribution. Useful for most isotropic glossy distributions
////////////////////////////////////////////////////////////////////////////////
float PowerCosineDistribution_PDF(float absCosTheta, float exponent) {
    float norm = (exponent+1.0f) / (2.0f * PI);
    return norm * pow(absCosTheta, exponent);
}

float PowerCosineDistribution_PDF(float3 wHalf, float3 wNorm, float exponent) {
    float absCosTheta = abs(dot(wHalf, wNorm));
    return PowerCosineDistribution_PDF(absCosTheta, exponent);
}

DistributionSample PowerCosineDistribution_Sample(float2 sampleUV, float exponent) {
    float cosTheta = pow(sampleUV.x, 1.0f / (exponent + 1.0f));
    float sinTheta = sqrt(1.0f - cosTheta * cosTheta);
    float phi = 2.0f * PI * sampleUV.y;
    
    DistributionSample distSample;
    distSample.Direction = float3(cos(phi) * sinTheta, cosTheta, sin(phi) * sinTheta);
    distSample.PDF = PowerCosineDistribution_PDF(cosTheta, exponent);
    return distSample;
}

//==============================================================================
// Utils for distribution samplers.
//==============================================================================

////////////////////////////////////////////////////////////////////////////////
// Decode encoded low discrepancy numbers. 
// See LowDiscrepancyTexture.cs for random creation and encoding.
////////////////////////////////////////////////////////////////////////////////
float2 DecodeRandomUV(float4 encodedRandom) {
    float x = (encodedRandom.r * 256.0f + encodedRandom.g) * 255.0f;
    float y = (encodedRandom.b * 256.0f + encodedRandom.a) * 255.0f;
    
    return float2(x, y) / 65536.0f;
}

void CreateTangents(float3 normal, out float3 tangent, out float3 bitangent) {
    float c = abs(normal.y) < 0.5f ? 0.0f : 1.0f;
    float3 t = float3(0.0f, 1.0f - c, c);
    // TODO Normalize shouldn't be needed as normal and t are normalized
    tangent = normalize(cross(t, normal));
    bitangent = normalize(cross(tangent, normal));
}

#endif // _PHYSICALLY_BIASED_UTILS_DISTRIBUTION_SAMPLERS_H_
