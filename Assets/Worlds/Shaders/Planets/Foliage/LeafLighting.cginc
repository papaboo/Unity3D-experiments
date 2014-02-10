//==============================================================================
// Leaf lighting
//==============================================================================

#ifndef _PLANET_FOLIAGE_LEAF_LIGHTING_H_
#define _PLANET_FOLIAGE_LEAF_LIGHTING_H_

#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/BxDFs/Blinn.cginc"
#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/BxDFs/Lambert.cginc"

half4 LightingLeaf(SurfaceOutput surface, half3 lightDir, half3 viewDir, half atten) {
    float NdotV = dot(surface.Normal, viewDir);
    float NdotL = dot(surface.Normal, lightDir);
    
    // Leafs reflect 90% of the light on the frontside and 10% is transmitted to the backside
    float lightScale = NdotL * NdotV >= 0.0f ? 0.75f : 0.25f;
    if (NdotV < 0.0f)
        viewDir *= -1.0f;
        //viewDir = -reflect(viewDir, surface.Normal);
    if (NdotL < 0.0f)
        lightDir *= -1.0f;
        //lightDir = -reflect(lightDir, surface.Normal);

    // half fresnel = Fresnel(viewDir, surface.Normal) * surface.Gloss;
    half fresnel = 0.02f * surface.Gloss;

    half3 specular = surface.Albedo * BlinnEvaluate(viewDir, lightDir, surface.Normal, surface.Specular);
    specular *= fresnel;

    half3 diffuse = surface.Albedo * LambertEvaluate(viewDir, lightDir, surface.Normal);
    diffuse *= (1.0f - _SpecColor * fresnel);
    
    half3 surfaceColor = diffuse + specular;

	half3 lightColor = _LightColor0.rgb * abs(dot(surface.Normal, lightDir)) * (atten * 2.0f);

    return half4(lightScale * lightColor * surfaceColor, surface.Alpha);
}

#endif // _PLANET_FOLIAGE_LEAF_LIGHTING_H_
