using UnityEngine;
using System.Collections;

namespace Utils {

////////////////////////////////////////////////////////////////////////////////
// Cubemap coordinates. Contains the face of the cubemap and the x and y indices 
// normalized to the range [-0.5f, 0.5].
////////////////////////////////////////////////////////////////////////////////
public struct CubemapCoordinates {
    public CubemapFace Face;
    public float X, Y;
    
    public CubemapCoordinates(CubemapFace face, float x, float y) {
        Face = face; X = x; Y = y;
    }
    
    public override string ToString() {
        return "[" + Face + ", x: " + X + ", y: " + Y + "]";
    }
}

public static class Cubemap {

    ////////////////////////////////////////////////////////////////////////////
    // Converts spherical coordiantes (theta, phi) to a direction vector.
    // Theta is the angle from the up vector to the direction in the range [0; PI] 
    // Phi is the angle from the azimuth and is in the range [-PI; PI]
    // We use +Y as the up vector and +Z as the forward vector.
    // See PBRT page 290 and 
    // http://en.wikipedia.org/wiki/Spherical_coordinate_system#Cartesian_coordinates
    ////////////////////////////////////////////////////////////////////////////
    public static Vector3 SphericalToDirection(float theta, float phi) {
        float sinTheta = Mathf.Sin(theta);
        float z = sinTheta * Mathf.Cos(phi);
        float x = -sinTheta * Mathf.Sin(phi);
        float y = Mathf.Cos(theta);
        return new Vector3(x, y, z);
    }
    
    ////////////////////////////////////////////////////////////////////////////
    // Converts from a normalized direction to spherical coordinates (theta, phi).
    // Theta is the angle from the up vector to the direction in the range [0; PI] 
    // Phi is the angle from the azimuth and is in the range [0; 2 * PI]
    // We use +Y as the up vector and +Z as the forward vector.
    ////////////////////////////////////////////////////////////////////////////
    public static Vector2 DirectionToSpherical(Vector3 direction) {
        // theta should be in the range [0; PI]
        float theta = Mathf.Acos(direction.y);
        theta += theta < 0.0f ? Mathf.PI : 0.0f;
        
        // phi should be in the range [0; 2*PI]
        float phi = Mathf.Atan2(-direction.x, direction.z);
        phi += phi < 0.0f ? 2.0f * Mathf.PI : 0.0f;
        return new Vector2(theta, phi);
    }

    ////////////////////////////////////////////////////////////////////////////
    // Converts from a normalized direction to spherical texture coordinates.
    ////////////////////////////////////////////////////////////////////////////
    public static Vector2 DirectionToSphericalUV(Vector3 direction) {
        Vector2 spherical = DirectionToSpherical(direction);
        return new Vector2(spherical.y / (2.0f * Mathf.PI),
                           1.0f - spherical.x / Mathf.PI);
    }
    
    ////////////////////////////////////////////////////////////////////////////
    // Computes the direction vector used for looking up in cubemap from the
    // x and y coords normalized to range [-0.5, 0.5] and the cube side.
    ////////////////////////////////////////////////////////////////////////////
    public static Vector3 CubemapDirection(CubemapCoordinates coords) {
        return CubemapDirection(coords.Face, coords.X, coords.Y);
    }
    public static Vector3 CubemapDirection(CubemapFace face, float x, float y) {
        switch(face) {
        case CubemapFace.PositiveX:
            return (new Vector3(0.5f, -y, -x)).normalized;
        case CubemapFace.NegativeX:
            return (new Vector3(-0.5f, -y, x)).normalized;
        case CubemapFace.PositiveY:
            return (new Vector3(x, 0.5f, y)).normalized;
        case CubemapFace.NegativeY:
            return (new Vector3(x, -0.5f, -y)).normalized;
        case CubemapFace.PositiveZ:
            return new Vector3(x, -y, 0.5f).normalized;
        case CubemapFace.NegativeZ:
        default: // because shaddap compiler
            return (new Vector3(-x, -y, -0.5f)).normalized;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    // Computes the cubemap face that a direction is pointing towards.
    ////////////////////////////////////////////////////////////////////////////
    public static CubemapFace GetCubemapFace(Vector3 dir) {
        Vector3 absDir = new Vector3(Mathf.Abs(dir.x), Mathf.Abs(dir.y), Mathf.Abs(dir.z));
        if (absDir.x >= absDir.y && absDir.x >= absDir.z)
            // X is major axis
            return dir.x > 0.0f ? CubemapFace.PositiveX : CubemapFace.NegativeX;
        else if (absDir.y >= absDir.z)
            // Y is major axis
            return dir.y > 0.0f ? CubemapFace.PositiveY : CubemapFace.NegativeY;
        else 
            // Z is major axis
            return dir.z > 0.0f ? CubemapFace.PositiveZ : CubemapFace.NegativeZ;
        
        // TODO Benchmark if this is ever so slightly faster ... and figure out why it won't work! ;)
        // float minLengthOfMajorAxis = Mathf.Sqrt(1.0f / 3.0f);
        // if (Mathf.Abs(dir.x) >= minLengthOfMajorAxis)
        //     // X is major axis
        //     return dir.x > 0.0f ? CubemapFace.PositiveX : CubemapFace.NegativeX;
        // else if (Mathf.Abs(dir.y) >= minLengthOfMajorAxis)
        //     // Y is major axis
        //     return dir.y > 0.0f ? CubemapFace.PositiveY : CubemapFace.NegativeY;
        // else // if (Mathf.Abs(dir.z) >= minLengthOfMajorAxis)
        //     // Z is major axis
        //     return dir.z > 0.0f ? CubemapFace.PositiveZ : CubemapFace.NegativeZ;
    }

    ////////////////////////////////////////////////////////////////////////////
    // Computes the normalized cubemap coordinates from a direction.
    ////////////////////////////////////////////////////////////////////////////
    public static CubemapCoordinates CubemapCoordinates(Vector3 dir) {
        CubemapFace face = GetCubemapFace(dir);
        switch(face) {
        case CubemapFace.NegativeX: {
            float norm = -0.5f / dir.x; // Normalizer that projects the direction onto the plane 0.5f units along the axis
            return new CubemapCoordinates(face, dir.z * norm, -dir.y * norm);
        }
        case CubemapFace.PositiveX: {
            float norm = 0.5f / dir.x;
            return new CubemapCoordinates(face, -dir.z * norm, -dir.y * norm);
        }
        case CubemapFace.NegativeY: {
            float norm = -0.5f / dir.y;
            return new CubemapCoordinates(face, dir.x * norm, -dir.z * norm);
        }
        case CubemapFace.PositiveY: {
            float norm = 0.5f / dir.y;
            return new CubemapCoordinates(face, dir.x * norm, dir.z * norm);
        }
        case CubemapFace.NegativeZ: {
            float norm = -0.5f / dir.z;
            return new CubemapCoordinates(face, -dir.x * norm, -dir.y * norm);
        }
        case CubemapFace.PositiveZ:
        default: { // because shaddap compiler
            float norm = 0.5f / dir.z;
            return new CubemapCoordinates(face, dir.x * norm, -dir.y * norm);
        }
        }
    }

} // class CubemapUtils

} // NS Utils

////////////////////////////////////////////////////////////////////////////
// Extension methods for unity cubemap
////////////////////////////////////////////////////////////////////////////
public static class ExtensionMethods {

    public static Color NearestSample(this Cubemap c, Utils.CubemapCoordinates coords) {
        // Can I somehow make sure that coords.X or coords.Y is never 1 for sane directions
        int x = (int)Mathf.Min(coords.X * c.width, c.width-1);
        int y = (int)Mathf.Min(coords.Y * c.width, c.width-1);
        
        return c.GetPixel(coords.Face, x, y);
    }
    
    public static Color NearestSample(this Cubemap c, Vector3 direction) {
        Utils.CubemapCoordinates coords = Utils.Cubemap.CubemapCoordinates(direction);
        return c.NearestSample(coords);
    }

    public static int MipmapCount(this Cubemap c) {
        int width = c.width;
        int mipmapCount = 0;
        while (width != 0) {
            ++mipmapCount;
            width = width >> 1;
        }
        return mipmapCount;
    }
    
} // class ExtensionMethods
