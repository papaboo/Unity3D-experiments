using UnityEngine;
using System.Collections;

//==============================================================================
// Updates the environment each frame.
// If an environment name is given, then the script will search for a texture
//with that name in the GameObject's material and set it's texture.
//==============================================================================
public class RealtimeEnvironment : MonoBehaviour {

    // TODO
    // disable on invisible
    // Update right before rendering?
    // Or simply update OnRender?
    // Detect if latlong is needed


    public enum eMode { Cubical, CubicalAndLatLong }
    public eMode Mode;

    public int CubemapSize = 256;
    public RenderTexture CubicalEnvironment; // { get; private set; }
    public RenderTexture LatLongEnvironment;// { get; private set; }

    // Data for applying the texture to the GameObject's material
    [System.Serializable]
    public class PropertyData {
        public enum eType { Ignore, Cubical, LatLong }
        public eType Type;
        public string EnvironmentName;
    }
    public PropertyData MaterialProperty;
    
    public class CameraData {
        public float nearClip;
        public float farClip;
    }
    public CameraData CameraSetup;
    private Camera cam;

	void Awake() {
        Initialize();
	}
	
    void OnDestroy() {
		if (CubicalEnvironment != null) DestroyImmediate(CubicalEnvironment);
        if (LatLongEnvironment != null) DestroyImmediate(LatLongEnvironment);
    }

	void LateUpdate() {
		if (cam.RenderToCubemap(CubicalEnvironment))
            Debug.LogWarning("Could not render to cubemap.");
        if (Mode == eMode.CubicalAndLatLong) {
            Debug.LogWarning("eMode.CubicalAndLatLong not supported.");
            Mode = eMode.Cubical;
        }
	}

    public void Initialize() {
        CreateCamera();
        CreateTextures();
    }

    private void CreateCamera() {
        cam = gameObject.AddComponent<Camera>();
        cam.farClipPlane = 100;
        cam.enabled = false;
    }

    private void CreateTextures() {
        CubicalEnvironment = new RenderTexture(CubemapSize, CubemapSize, 16);
        CubicalEnvironment.isCubemap = true;
        CubicalEnvironment.isPowerOfTwo = true;
        CubicalEnvironment.Create();
        
        if (Mode == eMode.CubicalAndLatLong) {
            LatLongEnvironment = new RenderTexture(CubemapSize * 4, CubemapSize * 2, 16);
        }

        if (MaterialProperty.Type != PropertyData.eType.Ignore) {
            RenderTexture tex = MaterialProperty.Type != PropertyData.eType.Cubical ? 
                CubicalEnvironment : LatLongEnvironment;
            renderer.material.SetTexture(MaterialProperty.EnvironmentName, tex);
        }
    }
}
