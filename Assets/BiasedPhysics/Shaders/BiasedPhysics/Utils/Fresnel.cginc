//==============================================================================
// Fresnel models
// The fresnel model used is chosen by setting a define. If no define is set a
// default 50/50 fresnel model is used that always returns 0.5.
//
// Models are:
//
// #define NO_FRESNEL
//  - The default 50/50 fresnel
//
// #define SCHLICK_FRESNEL
// - Schlick's approximation to the fresnel factor. Requires the float4 shader
//   property _fresnel_bias_scale_exponent.
// _Fresnel_bias_scale_exponent ("Schlick fresnel bias, scale and exponent", Vector) = (0.06, 0.94, 5, 0)
//
// #define UE4_FRESNEL
// - Unreal Engine's approximation to Schlick's approximation to the fresnel 
//   factor. See Real Shading in Unreal Engine 4 by Brian Karis.
//  Requires the float shader property _fresnel_bias.
// _UE4_Fresnel_bias ("UE4 fresnel bias", Float) = 0.06
//==============================================================================

#ifndef _PHYSICALLY_BIASED_UTILS_FRESNEL_H_
#define _PHYSICALLY_BIASED_UTILS_FRESNEL_H_

#if defined (SCHLICK_FRESNEL)

float4 _Fresnel_bias_scale_exponent;

float Fresnel(float3 wHalf, float3 wNorm) {
    // Schlick's fresnel approximation
    float cosTheta = dot(wNorm, wHalf);
    float bias = _Fresnel_bias_scale_exponent.x;
    float scale = _Fresnel_bias_scale_exponent.y;
    float exponent = _Fresnel_bias_scale_exponent.z;
    return saturate(bias + scale * pow(1.0f - abs(cosTheta), exponent));
}

#elif defined (UE4_FRESNEL)

float _UE4_Fresnel_bias;

float Fresnel(float3 wHalf, float3 wNorm) {
    // UE4's approximation to Schlick's fresnel approximation
    float cosTheta = dot(wNorm, wHalf);
    float bias = _UE4_Fresnel_bias;
    float scale = 1.0f - bias;
    float exponent = ((-5.55473f * cosTheta) - 6.98316f) * cosTheta;
    return saturate(bias + scale * pow(2.0f, exponent));
}

#else

float Fresnel(float3 wHalf, float3 wNorm) {
    return 0.5f;
}

#endif

float Fresnel(float3 wIn, float3 wOut, float3 wNorm) {
    float3 wHalf = normalize(wIn + wOut);
    return Fresnel(wHalf, wNorm);
}

#endif // _PHYSICALLY_BIASED_UTILS_FRESNEL_H_
