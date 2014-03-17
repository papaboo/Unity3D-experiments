//==============================================================================
// BxDF Sample
//==============================================================================

#ifndef _BIASED_PHYSICS_BXDF_SAMPLE_H_
#define _BIASED_PHYSICS_BXDF_SAMPLE_H_

struct BxDFSample {
    float3 Direction;
    float Weight; 
    float PDF;
    // TODO Performance wise it would be better to have Weight / PDF as a
    // memeber, but as this project is mostly about experiments, we return the 
    // weight and PDF separately.
};

#endif // _BIASED_PHYSICS_BXDF_SAMPLE_H_
