using UnityEditor;
using UnityEngine;
using System.Collections;

namespace Worlds {

public class CloudCreator : EditorWindow {

    private struct NoiseLayer {
        public int Samples;
        public float Amplitude;
        public float Radius;
        
        public NoiseLayer(int samples, float amplitude, float radius) {
            Samples = samples; Amplitude = amplitude; Radius = radius;
        }
    }

    private struct NoiseVector {
        public Vector3 Normal;
        public float Amplitude;
        public float Radius;
    }

    public static CloudCreator Instance = null;

    private string worldName = "Bob";
    
    private int detail = 64;
    private NoiseLayer[] noiseLayers = new NoiseLayer[1];
    
    [MenuItem("Worlds/Cloud Creator")]
    public static void Creator() {
        Instance = EditorWindow.GetWindow<CloudCreator>();
        Instance.title = "Cloud-O-Matic";
        
        // Init noise
        Instance.noiseLayers = new NoiseLayer[3];
        Instance.noiseLayers[0] = new NoiseLayer(  64, 0.50f, 0.40f);
        Instance.noiseLayers[1] = new NoiseLayer( 256, 0.20f, 0.15f);
        Instance.noiseLayers[2] = new NoiseLayer(1024, 0.08f, 0.06f);

        Instance.Show();
    }

    void OnGUI() {
        GUILayout.BeginArea(new Rect(0, 0, position.width, position.height)); {

            worldName = EditorGUILayout.TextField("World Name", worldName);

            EditorGUILayout.Space();

            detail = EditorGUILayout.IntField("Cloud detail", detail);
            
            EditorGUILayout.Space();

            int noiseLevels = EditorGUILayout.IntField("Noise levels", noiseLayers.Length);
            if (noiseLayers.Length != noiseLevels) {
                NoiseLayer[] newNoiseLayers = new NoiseLayer[noiseLevels];
                for (int i = 0; i < Mathf.Min(noiseLayers.Length, noiseLevels); ++i)
                    newNoiseLayers[i] = noiseLayers[i];
                noiseLayers = newNoiseLayers;
            }

            for (int i = 0; i < noiseLayers.Length; ++i) {
                EditorGUILayout.LabelField("Noise layer " + i);
                noiseLayers[i].Samples = EditorGUILayout.IntField("Samples", noiseLayers[i].Samples);
                noiseLayers[i].Amplitude = EditorGUILayout.FloatField("Amplitude", noiseLayers[i].Amplitude);
                noiseLayers[i].Radius = EditorGUILayout.FloatField("Radius", noiseLayers[i].Radius);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Create"))
                Create();
                
        } GUILayout.EndArea();
    }

    void Create() {

        try {
            string planetFolder = Application.dataPath + "/Worlds/Planets/" + worldName;
            System.IO.Directory.CreateDirectory(planetFolder);

            NoiseVector[] noiseVectors = CreateNoiseVectors(noiseLayers);
            
            Cubemap clouds = new Cubemap(detail, TextureFormat.RGBA32, false); // false for now as unity doesn't have seamless cubemaps $)%ˆ#)!_#
        
            foreach (CubemapFace face in System.Enum.GetValues(typeof(CubemapFace))) {
                EditorUtility.DisplayProgressBar("Latlong to cubemap", "Processing " + face, (float) face / 6.0f);
                Color[] pixels = new Color[detail * detail];
                for (int x = 0; x < detail; ++x)
                    for (int y = 0; y < detail; ++y) {
                        Vector3 dir = Utils.Cubemap.CubemapDirection(face,
                                                                     (x+0.5f) / (float)detail - 0.5f,
                                                                     (y+0.5f) / (float)detail - 0.5f);
                        
                        float intensity = 0.0f;
                        foreach (NoiseVector vec in noiseVectors) {
                            float distance = (dir - vec.Normal).magnitude;
                            float v = Mathf.Max(0.0f, 1.0f - distance / vec.Radius);
                            intensity += v * vec.Amplitude;
                        }
                        
                        int index = x + y * detail;
                        pixels[index] = new Color(intensity, intensity, intensity, intensity);
                    }

                clouds.SetPixels(pixels, face);
                clouds.Apply();
            }
            
            clouds.SmoothEdges(); // Because unity doesn't support seamless filtering, but is it enough?

            string saveFolder = "Assets/Worlds/Planets/" + worldName + "/";
            string savePath = saveFolder + "clouds.cubemap";
            AssetDatabase.CreateAsset(clouds, savePath);

        } catch (System.Exception e) {
            Debug.LogError("Creation of clouds failed:\n" + e);
        }
        
        EditorUtility.ClearProgressBar();
    }
    
    NoiseVector[] CreateNoiseVectors(NoiseLayer[] noiseLayers) {
        int vectorCount = 0;
        foreach (NoiseLayer layer in noiseLayers)
            vectorCount += layer.Samples;
        
        NoiseVector[] vectors = new NoiseVector[vectorCount];
        int vIndex = 0;
        foreach (NoiseLayer layer in noiseLayers) {
            uint scrambleU = (uint)Random.Range(int.MinValue, int.MaxValue);
            uint scrambleV = (uint)Random.Range(int.MinValue, int.MaxValue);
            uint offset = 2011;
            for (uint i = 0; i < (uint)layer.Samples; ++i) {
                Vector2 sample2D = RandomSamplers.Utils.Sample02(i + offset, scrambleU, scrambleV);
                vectors[vIndex].Normal = SphereDistribution.Sample(sample2D).Direction;
                vectors[vIndex].Amplitude = layer.Amplitude;
                vectors[vIndex].Radius = layer.Radius;
                ++vIndex;
            }
        }
        
        return vectors;
    }
    
}

} // NS Planet