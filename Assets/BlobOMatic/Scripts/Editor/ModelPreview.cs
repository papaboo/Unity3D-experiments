////////////////////////////////////////////////////////////////////////////////
// Copyright © Asger Vejen Hoedt 2013 All Rights Reserved. No part of this
// document may be reproduced, copied, modified or adapted without the written
// consent from the author.
////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;

namespace BlobOMatic {
    
public class ModelPreview {
    
    public RenderTexture Preview { get; private set; }
    
    private GameObject previewRoot;
    private EditorModel previewModel;
    private GameObject groundPlane;

    public float VerticalRotation = 30.0f;
    public float HorizontalRotation = 20.0f;

    public ModelPreview(EditorModel model) {
        previewModel = model;

        // Preview texture
        int previewSize = ModelGUI.TextureSize * 2;
        Preview = new RenderTexture(previewSize, previewSize, 24);
        Preview.hideFlags = HideFlags.HideInInspector | HideFlags.DontSave;

        // Preview scene
        previewRoot = new GameObject(model.Name + "Preview");
        previewRoot.transform.position = model.BlobModel.transform.position;

        model.BlobModel.transform.parent = previewRoot.transform;

        groundPlane = CreateGroundPlane(model.ModelBounds);
        groundPlane.transform.position = new Vector3(0, model.ModelBounds.min.y - model.ModelBounds.size.y * 0.01f, 0);
        groundPlane.transform.parent = previewRoot.transform;

        previewRoot.SetHideFlagsRecursively(HideFlags.HideAndDontSave);
        
        RenderPreview();
    }

    public void RenderPreview() {
        BlobOMatic.ModelCamera.targetTexture = Preview;
        
        // Activate the model and the ground plane
        previewModel.BlobModel.gameObject.SetActive(true);
        groundPlane.SetActive(true);

        // Adjust the camera's position
        Quaternion rot = Quaternion.AngleAxis(VerticalRotation, Vector3.up) * Quaternion.AngleAxis(HorizontalRotation, Vector3.right);
        float distance = previewModel.ModelBounds.extents.magnitude * 2.0f;
        Vector3 offset = previewModel.ModelBounds.center - new Vector3(0,0,distance);
        BlobOMatic.ModelCamera.transform.position = previewModel.ModelBounds.center + rot * offset;
        BlobOMatic.ModelCamera.transform.rotation = rot;

        // Adjust clipping planes to always keep the model in focus and show an 'infinite' plane.
        BlobOMatic.ModelCamera.nearClipPlane = distance * 0.05f;
        BlobOMatic.ModelCamera.farClipPlane = distance * 50.0f;

        // Add bounderies
        Transform bounds = null;
        if (Settings.ShowBounderies) {
            bounds = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            bounds.gameObject.layer = BlobOMatic.EditorLayer;
            float bounderyHeight = previewModel.ModelBounds.size.y * previewModel.DistanceSetting * 0.01f;
            bounds.position = new Vector3(previewModel.ModelBounds.center.x,
                                          previewModel.ModelBounds.min.y + 0.5f * bounderyHeight,
                                          previewModel.ModelBounds.center.z);
            
            Vector2 blobSize = previewModel.UseLocalSettings ? 
                previewModel.LocalSettings.ComputeBlobSize(previewModel.ModelBounds) :
                Settings.ComputeBlobSize(previewModel.ModelBounds);
            bounds.localScale = new Vector3(blobSize.x, bounderyHeight, blobSize.y);
            
            bounds.renderer.sharedMaterial = new Material(Shader.Find("Transparent/Bumped Specular"));
            bounds.renderer.sharedMaterial.color = new Color(0.1f, 0.5f, 1.0f, 0.45f);
            bounds.renderer.sharedMaterial.SetColor("_SpecColor", new Color(0.9f, 0.5f, 1.0f, 0.45f));
            bounds.renderer.sharedMaterial.SetFloat("_Shininess", 1.0f);
        }

        BlobOMatic.ModelCamera.Render();

        // Disable
        if (bounds)
            Transform.DestroyImmediate(bounds.gameObject);
        previewModel.BlobModel.gameObject.SetActive(false);
        groundPlane.SetActive(false);
        BlobOMatic.ModelCamera.targetTexture = null;
    }

    private GameObject CreateGroundPlane(Bounds bounds) {
        GameObject plane = new GameObject("Plane");
        plane.layer = BlobOMatic.EditorLayer;
        
        float halfSize = 100.0f * Mathf.Max(bounds.extents.x, bounds.extents.z);
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[]{new Vector3(-halfSize, 0.0f,  halfSize),
                                      new Vector3(-halfSize, 0.0f, -halfSize),
                                      new Vector3( halfSize, 0.0f, -halfSize),
                                      new Vector3( halfSize, 0.0f,  halfSize)};
        mesh.uv = new Vector2[]{new Vector3(-halfSize,-halfSize),
                                new Vector3(-halfSize, halfSize),
                                new Vector3( halfSize, halfSize),
                                new Vector3( halfSize,-halfSize)};
        mesh.triangles = new int[]{0,2,1,0,3,2};
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        plane.AddComponent<MeshFilter>().sharedMesh = mesh;
        
        plane.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Unlit/Texture"));
        Texture2D planeTex = AssetDatabase.LoadAssetAtPath("Assets/BlobOMatic/Textures/Plane.png", typeof(Texture2D)) as Texture2D;
        if (planeTex == null) Debug.LogError("could not find plane tex");
        plane.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = planeTex;
        
        return plane;
    }

}

} // NS BlobOMatic