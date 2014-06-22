using UnityEngine;
using UnityEditor;
using System.Collections;

public static class Convoluter {

    const int SAMPLES = 256;

    [MenuItem("Biased/Convolute")]
    public static void Convolute() {
        Object[] os = Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets);
        
        for (int o = 0; o < os.Length; ++o) {

            Texture2D latlong = os[o] as Texture2D;
            if (latlong != null)
                ConvoluteLatLong(latlong);
        }        
    }

    public static void ConvoluteLatLong(Texture2D inputMap) {
    
        Texture2D convoluted = new Texture2D(inputMap.width, inputMap.height, TextureFormat.RGB24, true);
        convoluted.filterMode = FilterMode.Trilinear;
        convoluted.name = inputMap.name + "_Convoluted";

        convoluted.SetPixels(inputMap.GetPixels());
        convoluted.Apply(false);

        Color[] srcPixels = convoluted.GetPixels();
        int srcWidth = convoluted.width;
        int srcHeight = convoluted.height;

        int mipmapCount = convoluted.mipmapCount;
            
        // Calc total pixels, used by the progress bar and not counting the
        // zero'th layer as it is processed nearly instantatious.
        int totalPixelCount = 0;
        for (int m = 1; m < mipmapCount; ++m) {
            int mipWidth = convoluted.width >> m;
            if (mipWidth == 0) mipWidth = 1;
            int mipHeight = convoluted.height >> m;
            if (mipHeight == 0) mipHeight = 1;
            totalPixelCount += mipHeight * mipWidth;
        }

        Vector2[] randomSamples = new Vector2[SAMPLES];
        uint scrambleU = 5569;
        uint scrambleV = 95597;
        for (uint s = 0; s < SAMPLES; ++s)
            randomSamples[s] = RandomSamplers.Utils.Sample02(2211 + s, scrambleU, scrambleV);

        System.DateTime start = System.DateTime.Now;

        // Convolute each miplevel seperately
        int pixelsProcessed = 0;
        for (int mipLevel = mipmapCount-1; mipLevel > 0; --mipLevel) {
            int mipWidth = convoluted.width >> mipLevel;
            if (mipWidth == 0) mipWidth = 1;
            int mipHeight = convoluted.height >> mipLevel;
            if (mipHeight == 0) mipHeight = 1;

            EditorUtility.DisplayProgressBar("Convoluting lat long skybox", "Processing " + inputMap.name, pixelsProcessed / (float) totalPixelCount);
                
            float exponent = ExponentFromMipLevel(mipLevel, mipmapCount-2);
            Debug.Log("shininess " + exponent + " at mip level " + mipLevel + " of " + mipmapCount);

            // Loop over all pixels
            Color[] pixels = new Color[mipWidth * mipHeight];
            Threading.Parallel.For(mipWidth, u => 
                    {
                        float phi = (((u + 0.5f) / (float) mipWidth)) * 2.0f * Mathf.PI;
                        for (int v = 0; v < mipHeight; ++v) {
                            float theta = (1.0f - (v + 0.5f) / (float) mipHeight) * Mathf.PI;
                            
                            // Compute direction
                            Vector3 normal = Utils.Cubemap.SphericalToDirection(theta, phi).normalized;
                            Vector3 tangent, bitangent;
                            CreateTangents(normal, out tangent, out bitangent);
                            
                            // Create samples and sample
                            Color summedColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                            float totalWeight = 0.0f;
                            for (int s = 0; s < SAMPLES; ++s) {
                                DistributionSample halfwaySample = PowerCosineDistribution.Sample(randomSamples[s], exponent);
                                
                                if (halfwaySample.PDF > 0.0f) {
                                    halfwaySample.Direction = tangent * halfwaySample.Direction.x + normal * halfwaySample.Direction.y + bitangent * halfwaySample.Direction.z;
                                    Vector2 sampleUV = Utils.Cubemap.DirectionToSphericalUV(halfwaySample.Direction);
                                    
                                    // TODO Perform own repeat-clamp sampling ?
                                    int x = (int)(sampleUV.x * (srcWidth-1));
                                    int y = (int)(sampleUV.y * (srcHeight-1));
                                    Color sample = srcPixels[x + y * srcWidth];
                                    
                                    float lightScale = Vector3.Dot(halfwaySample.Direction, normal);
                                    summedColor += sample * lightScale;
                                    totalWeight += lightScale;
                                }
                            }
                            
                            int index = u + v * mipWidth;
                            pixels[index] = summedColor / totalWeight;
                        }
                    }
                );

            convoluted.SetPixels(pixels, mipLevel);

            pixelsProcessed += mipWidth * mipHeight;
        }

        EditorUtility.DisplayProgressBar("Convoluting lat long skybox", "Saving " + inputMap.name, 0.99f);

        convoluted.Apply(false, true);

        string sourcePath = AssetDatabase.GetAssetPath(inputMap);
        string saveFolder = System.IO.Path.GetDirectoryName(sourcePath);
        string destPath = saveFolder +"/" + convoluted.name + ".asset";
        AssetDatabase.CreateAsset(convoluted, destPath);

        Texture2D.DestroyImmediate(convoluted);

        EditorUtility.ClearProgressBar();

        System.DateTime stop = System.DateTime.Now;
        Debug.Log("Conveluted " + sourcePath + " in " + (stop - start).TotalSeconds + "seconds.");
    }


    // [MenuItem("Biased/Convolute Cubemap")]
    // public static void ConvoluteCubemap() {
    //     foreach(Object o in Selection.GetFiltered(typeof(Cubemap),
    //                                               SelectionMode.Assets)) {

    //         // TODO proper progress bar that takes into acount mipmap size
    //         Cubemap inputMap = o as Cubemap;
    
    //         Cubemap convoluted = new Cubemap(inputMap.width, TextureFormat.RGBA32, true);
    //         convoluted.name = inputMap.name + "_Convoluted";

    //         // Copy base level as specular reflecting
    //         foreach (CubemapFace face in System.Enum.GetValues(typeof(CubemapFace)))
    //             convoluted.SetPixels(inputMap.GetPixels(face), face);

    //         convoluted.Apply();

    //         // For each mipmap level
    //         int mipmapCount = convoluted.MipmapCount();
    //         for (int m = 1; m < mipmapCount; ++m) {
    //             int faceSize = convoluted.width >> m;
                
    //             float exponent = 512.0f;

    //             // For each side
    //             // NOTE to get bilinear samples, the easiest way is to pregenerate 6
    //             // textures (one for each side) and then pad them with a 1 px border
    //             // fetched from the other textures.
    //             Color[] pixels = new Color[faceSize * faceSize]; // Reuse for each side
    //             foreach (CubemapFace face in System.Enum.GetValues(typeof(CubemapFace))) {
    //                 EditorUtility.DisplayProgressBar("Convoluting cubemap", "Processing " + inputMap.name + " " + face, (float) face / 6.0f);
    //                 for (int x = 0; x < faceSize; ++x)
    //                     for (int y = 0; y < faceSize; ++y) {
    //                         // Compute direction
    //                         int index = x + y * faceSize;
    //                         Vector3 normal = CubemapUtils.CubemapDirection(face,
    //                                                                        (x+0.5f) / (float)faceSize - 0.5f,
    //                                                                        (y+0.5f) / (float)faceSize - 0.5f);

    //                         // Create samples and sample
    //                         // TODO Use low discrepency sampler. Compute sample array once and then permute for each pixel
    //                         Color summedColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
    //                         float summedPDF = 0.0f;
    //                         for (int s = 0; s < SAMPLES; ++s) {
    //                             Vector2 sampleUV = new Vector2(Random.value, Random.value);
    //                             DistributionSample halfwaySample = PowerCosineDistribution.Sample(sampleUV, exponent);

    //                             if (halfwaySample.PDF > 0.0f) {
    //                                 Vector3 sampleDirection = Utils.Rotate(halfwaySample.Direction, normal);
                            
    //                                 summedColor += inputMap.NearestSample(sampleDirection) / halfwaySample.PDF;
    //                                 summedPDF += halfwaySample.PDF;
    //                             }
    //                         }

    //                         pixels[index] = summedColor / (float) SAMPLES;
    //                     }

    //                 convoluted.SetPixels(pixels, face, m);
    //             }
    //         }

    //         convoluted.Apply(false);

    //         string sourcePath = AssetDatabase.GetAssetPath(inputMap);
    //         string saveFolder = System.IO.Path.GetDirectoryName(sourcePath);
    //         string destPath = saveFolder +"/" + convoluted.name + ".cubemap";
    //         AssetDatabase.CreateAsset(convoluted, destPath);

    //         Debug.Log("Conveluted " + sourcePath);
    //     }

    //     EditorUtility.ClearProgressBar();
    // }

    public static void CreateTangents(Vector3 normal, out Vector3 tangent, out Vector3 bitangent) {
        float c = Mathf.Abs(normal.y) < 0.5f ? 0.0f : 1.0f;
        Vector3 t = new Vector3(0.0f, 1.0f - c, c);
        tangent = Vector3.Cross(t, normal).normalized;
        bitangent = Vector3.Cross(tangent, normal).normalized;
    }

    //==========================================================================
    // Based on the paper by McGuire et al.
    // http://graphics.cs.williams.edu/papers/EnvMipReport2013/paper.pdf
    // However we assume that a gloss exponent of 16383 is perfectly specular,
    // that the 0'th miplevel is perfectly specular and that the N-1'th layer is
    // completely diffuse, meaning an exponent of 0.
    // ==========================================================================
    private static float ExponentFromMipLevel(int mipLevel, int mipmapCount) {
        // float MIPlevel = log2(environmentMapWidth * sqrt(3)) - 0.5f * log2(glossyExponent + 1);
        // int mipOffset = mipmapCount - 1 - mipLevel;
        // return Mathf.Pow(2.0f, mipOffset * 2.0f) - 1; // Direct translation of paper, too specular
        
        // Assume that 16383 is perfectly specular, which is achieved with a mipexponent of 14
        float mipLerp = 1.0f - mipLevel / (mipmapCount-1.0f);
        float shininess = Mathf.Pow(2.0f, mipLerp * 14.0f) - 1;
        return shininess < 0 ? 0 : shininess; // To ensure valid exponents for miplevels below diffuse
    }

    //==========================================================================
    // The inverse of ExponentFromMipLevel
    //==========================================================================    
    private static float MipLevelFromExponent(float exponent, int mipmapCount) {
        // Clamp to 16383 as that is 'perfectly specular'
        // exponent = exponent < 16383.0f ? exponent : 16383.0f;
        
        float specularity = 1.0f - Log2(exponent + 1.0f) / 14.0f;
        return specularity * (mipmapCount-1.0f);
    }

    private static float Log2(float val) {
        return Mathf.Log(val) / Mathf.Log(2.0f);
    }
}

