//==============================================================================
// BxDF Sample
//==============================================================================

#ifndef _BIASED_PHYSICS_BXDF_SAMPLE_H_
#define _BIASED_PHYSICS_BXDF_SAMPLE_H_

struct BxDFSample {
    float3 Direction;
    float WeightOverPDF;
};

#endif // _BIASED_PHYSICS_BXDF_SAMPLE_H_
