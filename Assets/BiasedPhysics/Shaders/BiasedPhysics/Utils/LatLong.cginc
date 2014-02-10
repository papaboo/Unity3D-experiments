//==============================================================================
// Latlong utils
//==============================================================================

#ifndef _PHYSICALLY_BIASED_UTILS_LATLONG_H_
#define _PHYSICALLY_BIASED_UTILS_LATLONG_H_

#ifndef PI
#define PI 3.14159265359f
#endif // PI

////////////////////////////////////////////////////////////////////////////
// Converts from a normalized direction to spherical coordinates (theta, phi).
// Theta is the angle from the up vector to the direction in the range [0; PI] 
// Phi is the angle from the azimuth and is in the range [0; 2 * PI]
// We use +Y as the up vector and +Z as the forward vector.
////////////////////////////////////////////////////////////////////////////
float2 Utils_LatLong_DirectionToSpherical(float3 direction) {
    // theta should be in the range [0; PI]
    float theta = acos(direction.y);
    theta += theta < 0.0f ? PI : 0.0f;
        
    // phi should be in the range [0; 2*PI]
    float phi = atan2(-direction.x, direction.z);
    phi += phi < 0.0f ? 2.0f * PI : 0.0f;
    return float2(theta, phi);
}

////////////////////////////////////////////////////////////////////////////
// Converts from a normalized direction to spherical texture coordinates.
////////////////////////////////////////////////////////////////////////////
float2 Utils_LatLong_DirectionToSphericalUV(float3 direction) {
    float2 spherical = Utils_LatLong_DirectionToSpherical(direction);
    return float2(spherical.y / (2.0f * PI),
                  1.0f - spherical.x / PI);
}

#endif // _PHYSICALLY_BIASED_UTILS_LATLONG_H_
