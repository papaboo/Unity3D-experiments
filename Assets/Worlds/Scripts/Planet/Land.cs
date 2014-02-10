using UnityEngine;
using System.Collections;

namespace Planet {

public class Land : MonoBehaviour {

    // Should not be set at runtime
    public int Size = 96;
    public float MinHeight = 10.0f;
    public float MaxHeight = 11.0f;
    public float SandPercentage = 0.2f;

    public Cubemap Heightmap { get; private set; }

    // TODO Fast height getter (6 2D arrays of the height pr face padded with their neighbours) but how to interpolate?
    
    void Awake() {
        InitializeMaterial(Size, MinHeight, MaxHeight, SandPercentage);
    }
    
    public void InitializeMaterial(int size, float minHeight, float maxHeight, float sandPercentage) {
        Material mat = renderer.sharedMaterial;

        Heightmap = mat.GetTexture("_LandCube") as Cubemap;

        mat.SetFloat("_InvLandDetail", 1.0f / size);
        mat.SetVector("_MinMaxHeight_SandPercentage", new Vector4(minHeight, maxHeight, sandPercentage, 0.0f));
    }
}

} // NS Planet