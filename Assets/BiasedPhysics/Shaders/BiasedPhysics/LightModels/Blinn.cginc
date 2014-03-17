//==============================================================================
// Blinn LightModel
//==============================================================================

#ifndef _BIASED_PHYSICS_LIGHTMODEL_BLINN_H_
#define _BIASED_PHYSICS_LIGHTMODEL_BLINN_H_

#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/BxDFs/Blinn.cginc"
#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/LightModels/Globals.cginc"
#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/Utils/Fresnel.cginc"

half4 LightingBiasedPhysics_Blinn(SurfaceOutput surface, half3 lightDir, half3 viewDir, half atten) {
    float3 surfaceColor = surface.Albedo * BlinnEvaluate(viewDir, lightDir, surface.Normal, surface.Specular);

	fixed3 lightColor = _LightColor0.rgb * max(0, dot(surface.Normal, lightDir)) * (atten * 2.0f);

    return half4(lightColor * surfaceColor, surface.Alpha);
}

half3 BiasedPhysics_Blinn_IBL(SurfaceOutput surface, half3 worldNormal, half3 worlReflection) {
    return surface.Albedo * BlinnIBL(worlReflection, surface.Specular, _GlobalConvolutedEnvironment, _GlobalConvolutedEnvironmentMipmapCount);
}

half3 BiasedPhysics_Blinn_SampledIBL(SurfaceOutput surface, half3 worldViewDir, half3 worldNormal, int samplesDrawn, sampler2D encodedSamples, float invTotalSampleCount) {
    float3 tangent, bitangent;
    CreateTangents(worldNormal, tangent, bitangent);

    float3 iblColor = float3(0.0f);
    for (int i = 0; i < samplesDrawn; ++i) {
        half2 sampleUV = DecodeRandomUV(tex2D(encodedSamples, float2(i * invTotalSampleCount, 0.5f)));
        iblColor += SampleBlinnIBL(sampleUV, worldViewDir, worldNormal, tangent, bitangent, surface.Specular, _GlobalEnvironment);
    }
    
    return surface.Albedo * iblColor / samplesDrawn;
}

#endif // _BIASED_PHYSICS_LIGHTMODEL_BLINN_H_
