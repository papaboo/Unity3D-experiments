//==============================================================================
// Physically-Biased lighting
//==============================================================================

#ifndef _BIASED_PHYSICS_LIGHT_MODELS_GLOBALS_H_
#define _BIASED_PHYSICS_LIGHT_MODELS_GLOBALS_H_

sampler2D _EnvironmentMap;
float _EnvironmentMapMipmapCount;

samplerCUBE _GlobalEnvironment;
sampler2D _GlobalConvolutedEnvironment;
float _GlobalConvolutedEnvironmentMipmapCount;

#endif // _BIASED_PHYSICS_LIGHT_MODELS_GLOBALS_H_
