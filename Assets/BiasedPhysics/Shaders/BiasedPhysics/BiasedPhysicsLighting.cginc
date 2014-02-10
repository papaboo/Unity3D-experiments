//==============================================================================
// Physically-Biased lighting
//==============================================================================

#ifndef _BIASED_PHYSICS_LIGHTING_H_
#define _BIASED_PHYSICS_LIGHTING_H_

sampler2D _EnvironmentMap;
float _EnvironmentMapMipmapCount;

samplerCUBE _GlobalEnvironment;
sampler2D _GlobalConvolutedEnvironment;
float _GlobalConvolutedEnvironmentMipmapCount;

#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/LightModels/Blinn.cginc"
#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/LightModels/FunkyBlinn.cginc"
#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/LightModels/LambertBlinn.cginc"
#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/LightModels/Lambert.cginc"
#include "Assets/BiasedPhysics/Shaders/BiasedPhysics/LightModels/OrenNayar.cginc"

#endif // _BIASED_PHYSICS_LIGHTING_H_
