using UnityEngine;
using UnityEditor;
using System.Collections;

public static class TextureConverter {

    [MenuItem("Biased/Convert LatLongs to Cubemaps")]
    public static void ConvertLatLongToCubemap() {
        foreach(Object o in Selection.GetFiltered(typeof(Texture2D),
                                                  SelectionMode.Assets)) {
            try {
                Texture2D latLong = o as Texture2D;
                
                int faceSize = Mathf.ClosestPowerOfTwo(latLong.width / 4);
                Cubemap cube = new Cubemap(faceSize, latLong.format, latLong.mipmapCount > 0);
                
                // For each side
                foreach (CubemapFace face in System.Enum.GetValues(typeof(CubemapFace))) {
                    EditorUtility.DisplayProgressBar("Latlong to cubemap", "Processing " + latLong.name + " " + face, (float) face / 6.0f);
                    Color[] pixels = new Color[faceSize * faceSize];
                    for (int x = 0; x < faceSize; ++x)
                        for (int y = 0; y < faceSize; ++y) {
                            
                            Vector3 dir = Utils.Cubemap.CubemapDirection(face,
                                                                        (x+0.5f) / (float)faceSize - 0.5f,
                                                                        (y+0.5f) / (float)faceSize - 0.5f);

                            Vector2 uv = Utils.Cubemap.DirectionToSphericalUV(dir);
                        
                            int index = x + y * faceSize;
                            pixels[index] = latLong.GetPixelBilinear(uv.x, uv.y);
                        }
                
                    cube.SetPixels(pixels, face);
                    cube.Apply();
                }
            
                string sourcePath = AssetDatabase.GetAssetPath(latLong);
                string saveFolder = System.IO.Path.GetDirectoryName(sourcePath);
                string destPath = saveFolder +"/" + latLong.name + ".cubemap";
                AssetDatabase.CreateAsset(cube, destPath);
                Debug.Log("Converted " + sourcePath + " to cubemap");
            } catch (System.Exception e) {
                Debug.LogError("Convertion from lat long to cubemap failed:\n" + e);
            }
        }
        EditorUtility.ClearProgressBar();
    }

    /*
    [MenuItem("Biased/Test/Cubemap Direction Calculation")]
    public static void CubemapDirectionGenerationTest() {
        int faceSize = 4;
        Cubemap cube = new Cubemap(faceSize, TextureFormat.RGB24, false);
        
        // For each side
        foreach (CubemapFace face in System.Enum.GetValues(typeof(CubemapFace))) {
            Color[] pixels = new Color[faceSize * faceSize];
            for (int x = 0; x < faceSize; ++x)
                for (int y = 0; y < faceSize; ++y) {
                    
                    int index = x + y * faceSize;
                    Vector3 dir = CubemapUtils.CubemapDirection(face,
                                                                (x+0.5f) / (float)faceSize - 0.5f,
                                                                (y+0.5f) / (float)faceSize - 0.5f);
                    
                    pixels[index] = new Color(dir.x, dir.y, dir.z);
                }
                
            cube.SetPixels(pixels, face);
            cube.Apply();
        }
        
        AssetDatabase.CreateAsset(cube, "Assets/PhysicallyBiased/TestCubemapDirections.cubemap");
        Debug.Log("Generated /PhysicallyBiased/TestCubemapDirections.cubemap");
    }

    [MenuItem("Biased/Test/Generate LatLong")]
    public static void GenerateTestLatLong() {
        Texture2D latlong = new Texture2D(256, 128, TextureFormat.RGB24, true);
        latlong.wrapMode = TextureWrapMode.Clamp;
        
        for (int u = 0; u < latlong.width; ++u) {
            float phi = ((u + 0.5f) / (float) latlong.width) * 2.0f * Mathf.PI;
            for (int v = 0; v < latlong.height; ++v) {
                float theta = (1.0f - (v + 0.5f) / (float) latlong.height) * Mathf.PI;
                Vector3 dir = CubemapUtils.SphericalToDirection(theta, phi);

                latlong.SetPixel(u, v, FaceToColor(CubemapUtils.GetCubemapFace(dir)));
            }
        }
        latlong.Apply(); // Not really needed, but lets make sure
        
        string texPath = Application.dataPath + "/PhysicallyBiased/TestLatLong.png";
        System.IO.File.WriteAllBytes(texPath, latlong.EncodeToPNG());
        AssetDatabase.Refresh();
        Debug.Log("Generated /PhysicallyBiased/TestLatLong.png");
    }

    public static Color FaceToColor(CubemapFace face) {
        switch (face) {
        case CubemapFace.PositiveX:
            return new Color(1,0,0);
        case CubemapFace.NegativeX:
            return new Color(0,1,1);
        case CubemapFace.PositiveY:
            return new Color(0,1,0);
        case CubemapFace.NegativeY:
            return new Color(1,0,1);
        case CubemapFace.PositiveZ:
            return new Color(0,0,1);
        case CubemapFace.NegativeZ:
            return new Color(1,1,0);
        }
        return new Color(0,0,0); // Shaddap compiler
    }
    */
}
