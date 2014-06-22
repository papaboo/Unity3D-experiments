////////////////////////////////////////////////////////////////////////////////
// Copyright © Asger Vejen Hoedt 2013 All Rights Reserved. No part of this
// document may be reproduced, copied, modified or adapted without the written
// consent from the author.
////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

namespace BlobOMatic {

public class ABlobModel : MonoBehaviour {

    public Texture2D BlobShadow { get; protected set; }
    public Bounds ModelBounds { get; protected set; }
    public Renderer BlobRenderer { get; protected set; }
    public int MaxTextureSize = 4096; 

    public ABlobModel InitWithExistingBlob(GameObject blob) {
        blob.transform.parent = null; // Detach so that blob isn't taken into account when computing bound
        
        // Find bound size
        ModelBounds = ComputeBounds();

        // Reattach blob
        blob.transform.parent = transform;
        BlobRenderer = blob.GetComponent<MeshRenderer>();
        BlobRenderer.sharedMaterial = new Material(BlobRenderer.sharedMaterial);

        return this;
    }

    public ABlobModel Init(Mesh planeMesh) {

        // Find bound size
        ModelBounds = ComputeBounds();

        // Create blob
        GameObject blob = new GameObject("Blob");
        blob.transform.position = new Vector3(ModelBounds.center.x, ModelBounds.min.y, ModelBounds.center.z);
        blob.transform.parent = transform;

        MeshFilter blobMesh = blob.AddComponent<MeshFilter>();
        blobMesh.sharedMesh = planeMesh;

        BlobRenderer = blob.AddComponent<MeshRenderer>();
        BlobRenderer.sharedMaterial = new Material(Shader.Find("Unlit/Transparent"));
        BlobRenderer.sharedMaterial.hideFlags = HideFlags.HideInInspector | HideFlags.DontSave;

        return this;
    }

    private Bounds ComputeBounds() {
        Renderer[] rs = GetComponentsInChildren<Renderer>(true);
        if (rs.Length > 0) {
            Bounds modelBounds = rs[0].bounds;
            for (int b = 1; b < rs.Length; ++b)
                //ModelBounds.Encapsulate(rs[b].bounds); // No idea why this all of a sudden fails
                modelBounds = Utils.Union(modelBounds, rs[b].bounds);
            return modelBounds;
        } else {
            Debug.LogWarning(name + " contains no renderers.");
            return new Bounds(Vector3.zero, Vector3.zero);
        }
    }

    public delegate Vector2 BlobSizeComputation(Bounds b);

    public void RenderBlob(Camera blobCam, BlobSizeComputation ComputeBlobSize, float falloff, float blurFactor, float textureSize) {
        // Resize blob, remember to counter root scale
        Vector2 blobSize = ComputeBlobSize(ModelBounds);
        Vector3 rootScale = transform.localScale;
        BlobRenderer.transform.localScale = new Vector3(blobSize.x / rootScale.x, 1.0f / rootScale.y, blobSize.y / rootScale.z);
        
        // Resize blob texture
        int width, height; float textureDownScale;
        CalcTextureSize(blobSize, textureSize, out width, out height, out textureDownScale);
        if (BlobShadow == null || BlobShadow.width != width || BlobShadow.height != height) {
            if (BlobShadow != null) Texture2D.DestroyImmediate(BlobShadow);
            BlobShadow = new Texture2D(width, height);
            BlobShadow.filterMode = FilterMode.Trilinear;
            BlobShadow.hideFlags = HideFlags.HideInInspector | HideFlags.DontSave;
            BlobRenderer.sharedMaterial.mainTexture = BlobShadow;
        }

        // Setup blob camera
        RenderTexture targetTex = blobCam.targetTexture;
        Vector2 blobTexScale = new Vector2((float)targetTex.width / (float)BlobShadow.width,
                                           (float)targetTex.height / (float)BlobShadow.height);

        // Magic! Can't remember whats gaoing on here anymore. The margin start
        // copmutes the distance to where the margin should start (I think!) but
        // I've no idea what margin end is. An inverse lerp is used in the
        // shader to compute the intensity.
        Vector2 marginEnd = 
            new Vector2(1.0f - (float)(targetTex.width  - BlobShadow.width)  / (float)targetTex.width,
                        1.0f - (float)(targetTex.height - BlobShadow.height) / (float)targetTex.height);

        Vector2 marginStart = blobCam.GetComponent<BlobBlur>().MarginStart = 
            new Vector2(marginEnd.x / blobSize.x * ModelBounds.size.x,
                        marginEnd.y / blobSize.y * ModelBounds.size.z);

        blobCam.GetComponent<BlobBlur>().MarginEnd = new Vector2(Mathf.Max(marginEnd.x, marginStart.x),
                                                                 Mathf.Max(marginEnd.y, marginStart.y));

        // Place camera so it can see the model
        blobCam.transform.position = new Vector3(ModelBounds.center.x, ModelBounds.min.y - 0.01f * ModelBounds.size.y, ModelBounds.center.z);
        blobCam.orthographicSize = blobSize.y * 0.5f * blobTexScale.y;
        blobCam.aspect = 1.0f;

        // Convert texture size to mipmap max offset.
        // If the texture size had to be scaled due to originally exceeding 4096 pixels, 
        // the scale is also applied to the texture size to keep the shadow relatively consistent.
        float modelSize = textureSize * Mathf.Max(ModelBounds.size.x, ModelBounds.size.z);
        blobCam.GetComponent<BlobBlur>().MipOffset = Mathf.Log(blurFactor * modelSize * textureDownScale, 2.0f);

        // Render blob
        Shader.SetGlobalFloat("_BlobFalloffExponent", falloff);
        blobCam.Render();

        // Copy from target tex to BlobShadow
        RenderTexture oldActive = RenderTexture.active;
        RenderTexture.active = targetTex;
        BlobShadow.ReadPixels(new Rect((targetTex.width - BlobShadow.width) / 2, (targetTex.height - BlobShadow.height) / 2, 
                                       BlobShadow.width, BlobShadow.height), 0, 0);
        BlobShadow.Apply();
		RenderTexture.active = oldActive;
    }
        
    private void CalcTextureSize(Vector2 blobSize, float textureSize, 
                                 out int width, out int height, out float downScale) {
        downScale = 1.0f;
        width = (int)(blobSize.x * textureSize);
        height = (int)(blobSize.y * textureSize);
        
        // Correct sizes so it never exceeds 4096 along any dimension and scale
        // the other dimension accordingly.
        if (width > MaxTextureSize) {
            downScale =  (float)MaxTextureSize / (float)width;
            height = (int)(height * downScale);
            width = MaxTextureSize;
        }
        if (height > MaxTextureSize) {
            downScale =  (float)MaxTextureSize / (float)height;
            width = (int)(width * downScale);
            height = MaxTextureSize;
        }
        
        // Sizes must be even so halfsize is an int and blitting around the
        // center can be pixel perfect.
        if ((width & 0x00000001) == 1) ++width;
        if ((height & 0x00000001) == 1) ++height;
    }

}

} // NS BlobOMatic
