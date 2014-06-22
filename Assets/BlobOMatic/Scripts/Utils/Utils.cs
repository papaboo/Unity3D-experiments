////////////////////////////////////////////////////////////////////////////////
// Copyright © Asger Vejen Hoedt 2013 All Rights Reserved. No part of this
// document may be reproduced, copied, modified or adapted without the written
// consent from the author.
////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

namespace BlobOMatic {

public static class Utils {

    public static Camera CreateModelCamera(int cullingMask) {
        GameObject camGO = new GameObject("Blob Model Camera");
        Camera cam = camGO.AddComponent<Camera>();
        cam.enabled = false;
        cam.cullingMask = cullingMask;
        cam.backgroundColor = new Color32(40, 77, 121, 255);

        return cam;
    }

    public static Camera CreateBlobCamera(int cullingMask) {
        GameObject camGO = new GameObject("Blob shadow camera");
        
        camGO.transform.rotation = Quaternion.AngleAxis(90, new Vector3(-1,0,0));

        Camera blobCamera = camGO.AddComponent<Camera>();
        blobCamera.enabled = false;
        blobCamera.orthographic = true;
        blobCamera.clearFlags = CameraClearFlags.SolidColor;
        blobCamera.nearClipPlane = 0.0f;
        blobCamera.farClipPlane = Settings.Distance;
        blobCamera.backgroundColor = new Color(0,0,0,0);
        blobCamera.cullingMask = cullingMask;
        blobCamera.SetReplacementShader(Shader.Find("BlobOMatic/ExponentialBlob"), "");
        blobCamera.targetTexture = new RenderTexture(4096, 4096, 24);
        Shader.SetGlobalFloat("_BlobExponent", 2.0f);
        
        camGO.AddComponent<BlobBlur>();

        return blobCamera;
    }

    public static Bounds Union(Bounds lhs, Bounds rhs) {
        Vector3 min = new Vector3(Mathf.Min(lhs.min.x, rhs.min.x),
                                  Mathf.Min(lhs.min.y, rhs.min.y),
                                  Mathf.Min(lhs.min.z, rhs.min.z));

        Vector3 max = new Vector3(Mathf.Max(lhs.max.x, rhs.max.x),
                                  Mathf.Max(lhs.max.y, rhs.max.y),
                                  Mathf.Max(lhs.max.z, rhs.max.z));
        
        return new Bounds((min + max) * 0.5f, max - min);
    }

}

} // NS BlobOMatic