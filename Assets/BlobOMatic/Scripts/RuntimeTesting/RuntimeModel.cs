////////////////////////////////////////////////////////////////////////////////
// Copyright © Asger Vejen Hoedt 2013 All Rights Reserved. No part of this
// document may be reproduced, copied, modified or adapted without the written
// consent from the author.
////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;

namespace BlobOMatic {

public class RuntimeModel : ABlobModel {

    public Camera BlobCam { get; private set; }
    private Mesh blobMesh;

	void Awake() {
        // Create blob mesh
        blobMesh = new Mesh();
        Vector3[] verts = {new Vector3(-0.5f, 0.0f,  0.5f),
                           new Vector3(-0.5f, 0.0f, -0.5f),
                           new Vector3( 0.5f, 0.0f, -0.5f),
                           new Vector3( 0.5f, 0.0f,  0.5f)};
        blobMesh.vertices = verts;
        blobMesh.uv = new Vector2[]{new Vector3(0,0),
                                    new Vector3(0,1),
                                    new Vector3(1,1),
                                    new Vector3(1,0)};
        blobMesh.triangles = new int[]{0,2,1,0,3,2};

        Init(blobMesh);
        
        int blobLayer = LayerMask.NameToLayer("UnityEditor");
        gameObject.SetLayerRecursively(blobLayer);

        BlobCam = Utils.CreateBlobCamera(blobLayer);
        BlobCam.transform.parent = transform;
        BlobCam.cullingMask = 1 << blobLayer;
        MaxTextureSize = 512;
        BlobCam.targetTexture = new RenderTexture(MaxTextureSize, MaxTextureSize, 24);
	}

    void OnDestroy() {
        if (BlobCam != null) {
            Destroy(BlobCam.targetTexture);
            Destroy(BlobCam);
        }
        Destroy(blobMesh);
        Destroy(BlobRenderer.sharedMaterial);
        Destroy(BlobShadow);
    }

    void LateUpdate() {
        RenderBlob(BlobCam, Settings.ComputeBlobSize, Settings.Falloff, 0.05f, Settings.TextureSize);
    }
}

} // NS BlobOMatic
