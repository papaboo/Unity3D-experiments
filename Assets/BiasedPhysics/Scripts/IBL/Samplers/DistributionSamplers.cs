using UnityEngine;
using System.Collections;

public struct DistributionSample {
    public Vector3 Direction;
    public float PDF;
    
    public DistributionSample(Vector3 direction, float pdf) {
        this.Direction = direction;
        this.PDF = pdf;
    }

    public override string ToString() {
        return "[Direction: " + Direction.ToString("0.000") + ", pdf: " + PDF + "]";
    }
}

////////////////////////////////////////////////////////////////////////////////
// Cosine distribution. Useful for diffuse BRDFs such as Lambert (also Oren-Nayar?)
////////////////////////////////////////////////////////////////////////////////
public static class CosineDistribution {

    public static DistributionSample Sample(Vector2 sampleUV) {
        float cosTheta = sampleUV.x;
        float sinTheta = Mathf.Sqrt(1.0f - cosTheta * cosTheta);
        float phi = 2 * Mathf.PI * sampleUV.y;
        
        return new DistributionSample(new Vector3(Mathf.Cos(phi) * sinTheta, cosTheta, Mathf.Sin(phi) * sinTheta),
                                      cosTheta / Mathf.PI);
    }

    public static float PDF(Vector3 wOut) {
        return Mathf.Abs(wOut.y) / Mathf.PI;
    }
}

////////////////////////////////////////////////////////////////////////////////
// Power cosine distribution. Useful for most isotropic glossy distributions
////////////////////////////////////////////////////////////////////////////////
public static class PowerCosineDistribution {

    public static DistributionSample Sample(Vector2 sampleUV, float exponent) {
        float phi = 2.0f * Mathf.PI * sampleUV.x;
        float cosTheta = Mathf.Pow(sampleUV.y, 1.0f / (exponent + 1.0f));
        float sinTheta = Mathf.Sqrt(1.0f - cosTheta * cosTheta);
        
        return new DistributionSample(new Vector3(Mathf.Cos(phi) * sinTheta, cosTheta, Mathf.Sin(phi) * sinTheta),
                                      PDF(cosTheta, exponent));
    }

    public static float PDF(Vector3 wHalf, Vector3 wNorm, float exponent) {
        float absCosTheta = Mathf.Abs(Vector3.Dot(wHalf, wNorm));
        return PDF(absCosTheta, exponent);
    }

    public static float PDF(float absCosTheta, float exponent) {
        float norm = (exponent+1.0f) / (2.0f * Mathf.PI);
        return norm * Mathf.Pow(absCosTheta, exponent);
    }
}

////////////////////////////////////////////////////////////////////////////////
// Power cosine distribution. Useful for most isotropic glossy distributions
////////////////////////////////////////////////////////////////////////////////
public static class SphereDistribution {

    public static DistributionSample Sample(Vector2 sampleUV) {
        float z = 1.0f - 2.0f * sampleUV.x;
        float r = Mathf.Sqrt(Mathf.Max(0.0f, 1.0f - z * z));
        float phi = 2.0f * Mathf.PI * sampleUV.y;
        float x = r * Mathf.Cos(phi);
        float y = r * Mathf.Sin(phi);
        return new DistributionSample(new Vector3(x, y, z), PDF());
    }

    public static float PDF() {
        return 1.0f / (4.0f * Mathf.PI);
    }
}
