using UnityEngine;
using UnityEditor;
using System.Collections;

public class LowDisrepancyTexture {

    [MenuItem("Biased/Utils/Low Disrepancy Samples Texture")]
    public static void Go() {
        int sampleCount = 2048;
        Texture2D sampleTex = new Texture2D(sampleCount, 1, TextureFormat.ARGB32, false);
        sampleTex.wrapMode = TextureWrapMode.Repeat;
        sampleTex.filterMode = FilterMode.Point;
        
        Color32[] pixels = new Color32[sampleCount];
        uint scrambleU = 5569;
        uint scrambleV = 95597;
        for (uint s = 0; s < sampleCount; ++s) {
            Vector2 sample = RandomSamplers.Utils.Sample02(2211 + s, scrambleU, scrambleV);
            pixels[s] = Encode(sample);
            if (s == 0)
                Debug.Log("Sample: " + sample.ToString("0.00000000") + "\nEncoded: " + Encode(sample) + "\nDecoded: " + Decode(Encode(sample)).ToString("0.00000000"));
        }
        
        sampleTex.SetPixels32(pixels);
        sampleTex.Apply(false); // Don't update mipmaps and make unreadable for the CPU

        string texPath = Application.dataPath + "/BiasedPhysics/Textures/LowDiscrepancy.png";
        System.IO.File.WriteAllBytes(texPath, sampleTex.EncodeToPNG());
    }
    
    public static Color32 Encode(Vector2 sample) {
        // Encode each dimension in 16 bit
        int x = (int)(65536.0f * sample.x);
        if (x > 65535) {
            x = 65535; // Should never ever ever ever ever happen (except maybe in the case of weird floating point behaviour, but multiplication should preserve precision)
            Debug.LogWarning(sample.x + " was too close to one and was rounded to 65536. Explicitly rounding down.");
        }

        int y = (int)(65536.0f * sample.y);
        if (y > 65535) {
            y = 65535; // See above
            Debug.LogWarning(sample.y + " was too close to one and was rounded to 65536. Explicitly rounding down.");
        }
        
        return new Color32((byte)((x>>8) % 256), (byte)(x % 256), // Encode the highest 8 bit of 8 in red and the lowest 8 bits in green
                           (byte)((y>>8) % 256), (byte)(y % 256)); // Same as above but with blue and alpha.
    }

    public static Vector2 Decode(Color32 sample) {
        Color s = sample; // Convert to range [0, 1] as that is what we'll have on the GPU

        float x = (s.r * 256.0f + s.g) * 255.0f;
        float y = (s.b * 256.0f + s.a) * 255.0f;

        return new Vector2(x, y) / 65536.0f;
    }

}
