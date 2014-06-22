using UnityEngine;
using System.Collections;

public class SimplePostProcess : MonoBehaviour {
    
    public Material PostProcessMaterial;
    
    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (PostProcessMaterial != null) {
            Graphics.Blit(source, destination, PostProcessMaterial);
        } else {
            Debug.LogWarning("No material assigned to SimplePostProcess on " + gameObject.name);
            Graphics.Blit(source, destination);
        }
    }
}
