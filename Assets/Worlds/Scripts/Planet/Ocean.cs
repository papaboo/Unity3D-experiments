using UnityEngine;
using System.Collections;

namespace Planet {

public class Ocean : MonoBehaviour {

    public float OceanHeight;

    void Awake() {
        // Initialize();
    }

    public void Initialize(float landMinHeight, float landMaxHeight, float oceanHeight, Cubemap landHeightmap) {
        Material mat = renderer.sharedMaterial;
        mat.SetVector("_MinMaxHeight_OceanHeight", new Vector4(landMinHeight, landMaxHeight, oceanHeight, 0.0f));
        
        //mat.SetTexture("_LandCube", planet.renderer.sharedMaterial.GetTexture("_LandCube"));
        mat.SetTexture("_LandCube", landHeightmap);
    }
    
}

} // NS Planet