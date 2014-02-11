//==============================================================================
// Oren-Nayar LightModel
// Requires the float _roughness uniform. 
// TODO Add to surface description? Or not as unity's surface description is a
// hack.
//==============================================================================

#ifndef _BIASED_PHYSICS_LIGHTMODEL_OREN_NAYAR_BLINN_H_
#define _BIASED_PHYSICS_LIGHTMODEL_OREN_NAYAR_BLINN_H_

#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/BxDFs/OrenNayar.cginc"
#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/BxDFs/Blinn.cginc"

half4 LightingBiasedPhysics_OrenNayarBlinn(SurfaceOutput surface, half3 lightDir, half3 viewDir, half atten) {
    half fresnel = Fresnel(viewDir, surface.Normal) * surface.Gloss;

    half3 coat = _SpecColor * BlinnEvaluate(viewDir, lightDir, surface.Normal, surface.Specular);
    coat *= fresnel;

    half3 base = surface.Albedo * OrenNayarEvaluate(viewDir, lightDir, surface.Normal, _Roughness);
    base *= (1.0f - _SpecColor * fresnel);

    half3 surfaceColor = base + coat;

	half3 lightColor = _LightColor0.rgb * max(0, dot(surface.Normal, lightDir)) * (atten * 2.0f);

    return half4(lightColor * surfaceColor, surface.Alpha);
}

half3 BiasedPhysics_OrenNayarBlinn_IBL(SurfaceOutput surface, half3 worldNormal, half3 worldReflection) {
    float fresnel = Fresnel(worldReflection, worldNormal) * surface.Gloss;
    
    half3 coat = _SpecColor * BlinnIBL(worldReflection, surface.Specular, _GlobalConvolutedEnvironment, _GlobalConvolutedEnvironmentMipmapCount);
    coat *= fresnel;

    half3 base = surface.Albedo * OrenNayarIBL(worldNormal, _Roughness, _GlobalConvolutedEnvironment, _GlobalConvolutedEnvironmentMipmapCount);
    base *= (1.0f - _SpecColor * fresnel);
    
    return base + coat;
}

#endif // _BIASED_PHYSICS_LIGHTMODEL_OREN_NAYAR_BLINN_H_
