using UnityEngine;
using System.Collections;
using System.Threading;

public static class RGBMish {

    public static Vector4 Encode(Vector3 rgb) {
        Vector3 scaledRGB = new Vector3(Mathf.Sqrt(rgb.x), Mathf.Sqrt(rgb.y), Mathf.Sqrt(rgb.z));
        scaledRGB /= 6.0f;

        float m = Mathf.Max(Mathf.Max(scaledRGB.x, scaledRGB.y),
                            Mathf.Max(scaledRGB.z, 1e-6f));
        m = Mathf.Clamp01(Mathf.Ceil(m * 255.0f) / 255.0f);
        
        return new Vector4(Mathf.Clamp01(scaledRGB.x / m),
                           Mathf.Clamp01(scaledRGB.y / m),
                           Mathf.Clamp01(scaledRGB.z / m),
                           m);
    }
    
    public static Vector3 Decode(Vector4 rgbm) {
        float scale = 6.0f * rgbm.w;
        Vector3 scaledRGB = new Vector3(rgbm.x, rgbm.y, rgbm.z) * scale;
        return new Vector3(Mathf.Pow(scaledRGB.x, 2.0f),
                           Mathf.Pow(scaledRGB.y, 2.0f),
                           Mathf.Pow(scaledRGB.z, 2.0f));
    }
    
}
