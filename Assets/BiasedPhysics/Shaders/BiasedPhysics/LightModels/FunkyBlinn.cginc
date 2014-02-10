//==============================================================================
// Funky Blinn LightModel.
// Simulates a rough base with glossy coat by varying the specularity based on
// viewing angle.
//==============================================================================

#ifndef _BIASED_PHYSICS_LIGHTMODEL_FUNKY_BLINN_H_
#define _BIASED_PHYSICS_LIGHTMODEL_FUNKY_BLINN_H_

#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/BxDFs/Blinn.cginc"
#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/Utils/Fresnel.cginc"

half4 LightingBiasedPhysics_FunkyBlinn(SurfaceOutput surface, half3 lightDir, half3 viewDir, half atten) {
    float fresnel = Fresnel(viewDir, surface.Normal) * surface.Gloss;

    // Use fresnel to modify surface specularity to fake a layered surface. Funky stuff
    surface.Specular *= fresnel;

    float3 surfaceColor = surface.Albedo * BlinnEvaluate(viewDir, lightDir, surface.Normal, surface.Specular);

	fixed3 lightColor = _LightColor0.rgb * max(0, dot(surface.Normal, lightDir)) * (atten * 2.0f);

    return half4(lightColor * surfaceColor, surface.Alpha);
}

half3 BiasedPhysics_FunkyBlinn_IBL(SurfaceOutput surface, half3 worldNormal, half3 worlReflection) {
    float fresnel = Fresnel(worlReflection, worldNormal) * surface.Gloss;
    surface.Specular *= fresnel;

    return surface.Albedo * BlinnIBL(worlReflection, surface.Specular, _GlobalConvolutedEnvironment, _GlobalConvolutedEnvironmentMipmapCount);
}


#endif // _BIASED_PHYSICS_LIGHTMODEL_FUNKY_BLINN_H_
