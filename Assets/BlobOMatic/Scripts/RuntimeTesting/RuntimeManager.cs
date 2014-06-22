////////////////////////////////////////////////////////////////////////////////
// Copyright © Asger Vejen Hoedt 2013 All Rights Reserved. No part of this
// document may be reproduced, copied, modified or adapted without the written
// consent from the author.
////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

namespace BlobOMatic {

public class RuntimeManager : MonoBehaviour {

    public RuntimeModel[] models;
    private RuntimeModel currentModel;
    public Material planeMat;

    private string distanceText;
    private string falloffText;
    private string strengthText;
    private string blurItrText;

    private float verticalRotation = 30.0f;
    private float horizontalRotation = -15.0f;

    void Awake() {
        distanceText = "7";
        falloffText = "3";
        strengthText = "0.5";
        blurItrText = "6";
    }

    void Start() {        
        foreach (RuntimeModel model in models) {
            GameObject plane = CreatePlane(model.ModelBounds);
            plane.transform.position = new Vector3(0, model.ModelBounds.min.y - model.ModelBounds.size.y * 0.01f, 0);
            plane.transform.parent = model.transform;
            model.gameObject.SetActive(false);
        }        

        currentModel = models[0];
        currentModel.gameObject.SetActive(true);
    }

    private GameObject CreatePlane(Bounds bounds) {
        GameObject plane = new GameObject("Plane");
        
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
        
        plane.AddComponent<MeshRenderer>().sharedMaterial = planeMat;
        
        return plane;
    }

    Vector2 prevPos = Vector2.zero;
    void Update() {
        if (Input.GetKeyDown(KeyCode.Mouse0))
            prevPos = Input.mousePosition;
        if (Input.GetKey(KeyCode.Mouse0)) {
            Vector2 delta = (Vector2)Input.mousePosition - prevPos;
            verticalRotation += delta.x / 2.0f;
            horizontalRotation += delta.y / 3.0f;
            horizontalRotation = Mathf.Clamp(horizontalRotation, -70.0f, 0.0f);
            prevPos = Input.mousePosition;
        }

        Quaternion rot = Quaternion.AngleAxis(verticalRotation, Vector3.up) * Quaternion.AngleAxis(horizontalRotation, Vector3.right);
        Vector3 offset = currentModel.ModelBounds.center - new Vector3(0,0,-currentModel.ModelBounds.extents.magnitude*2.0f);
        transform.position = currentModel.ModelBounds.center + rot * offset;
        transform.LookAt(currentModel.ModelBounds.center);
    }

    void OnGUI() {
        // Blob settings
        GUI.Box(new Rect(-5.0f, -5.0f, 245.0f, 105.0f + models.Length * 25.0f), "");
        
        Settings.Distance = FloatField(new Rect(2.5f, 2.5f, 230, 20), "Shadow Distance", ref distanceText, Settings.Distance);
        foreach (RuntimeModel model in models)
            model.BlobCam.farClipPlane = Settings.Distance;
        
        Settings.Falloff = FloatField(new Rect(2.5f, 25.0f, 230, 20), "Falloff", ref falloffText, Settings.Falloff);
        Shader.SetGlobalFloat("_BlobFalloffExponent", Settings.Falloff);

        Settings.Strength = FloatField(new Rect(2.5f, 47.5f, 230, 20), "Strength", ref strengthText, Settings.Strength);
        
        Settings.BlurIterations = IntField(new Rect(2.5f, 70.0f, 230, 20), "Blur Iterations", ref blurItrText, Settings.BlurIterations);
        if (Settings.BlurIterations < 0 || 20 < Settings.BlurIterations) {
            Settings.BlurIterations = Mathf.Clamp(Settings.BlurIterations, 0, 20);
            blurItrText = Settings.BlurIterations.ToString();
        }
        foreach (RuntimeModel model in models) {
            BlobBlur blobBlur = model.BlobCam.GetComponent<BlobBlur>();
            blobBlur.BlurIterations = Settings.BlurIterations;
            blobBlur.Strength = Settings.Strength;
        }
        
        // Model switching
        for (uint i = 0; i < models.Length; ++i) {
            float y = 95f + i * 25.0f;
            Rect r = new Rect(2.5f, y, 230.0f, 22.5f);
            if (GUI.Button(r, models[i].name)) {
                currentModel.gameObject.SetActive(false);
                currentModel = models[i];
                currentModel.gameObject.SetActive(true);
            }
        }
    }

    float FloatField(Rect r, string text, ref string valText, float val) {
        Rect halfRect = new Rect(r.x, r.y, r.width * 0.5f, r.height);
        GUI.Label(halfRect, text);
        halfRect.x += halfRect.width;
        valText = GUI.TextField(halfRect, valText);
        float v;
        if (float.TryParse(valText, out v))
            return v;
        else
            return val;
    }

    int IntField(Rect r, string text, ref string valText, int val) {
        Rect halfRect = new Rect(r.x, r.y, r.width * 0.5f, r.height);
        GUI.Label(halfRect, text);
        halfRect.x += halfRect.width;
        valText = GUI.TextField(halfRect, valText);
        int v;
        if (int.TryParse(valText, out v))
            return v;
        else
            return val;
    }

}

} // NS BlobOMatic
