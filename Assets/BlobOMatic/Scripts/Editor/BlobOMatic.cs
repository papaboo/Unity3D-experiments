////////////////////////////////////////////////////////////////////////////////
// Copyright © Asger Vejen Hoedt 2013 All Rights Reserved. No part of this
// document may be reproduced, copied, modified or adapted without the written
// consent from the author.
////////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace BlobOMatic {

public class BlobOMatic : EditorWindow {

    public static BlobOMatic Instance = null;

    private static Dictionary<Object, ModelGUI> models = null; // Consider using path as key, longer comparison times, but better sorting in the editor GUI.
    private static HashSet<Object> ignoredSelections = null;
    public static Camera ModelCamera { get; private set; }
    private GameObject previewLight;
    public static Camera BlobCamera { get; private set; }

    public static int EditorLayer = -1;

    // GUI vars
    public Texture2D DarkTexture { get; private set; }
    public Texture2D HorizontalDelimeterTex { get; private set; }
    Vector2 modelScrollPos = Vector2.zero;

    ModelGUI rotatingModel = null;
    Vector2 mouseDelta;
    
    [MenuItem("Window/Blob-O-Matic")]
    public static void Init() {
        if (EditorApplication.isPlaying) {
            Debug.LogError("Blob-O-Matic only supported in Edit Mode.");
            return;            
        }

        EditorLayer = FindOrCreateLayer("UnityEditor");
        if (EditorLayer == -1) {
            Debug.LogError("Blob-O-Matic requires a layer called 'UnityEditor'. " + 
                           "Create such a layer before opening the editor.");
            return;
        }
        
        Instance = EditorWindow.GetWindow<BlobOMatic>();
        Instance.title = "Blob-O-Matic";
        Instance.wantsMouseMove = true;
        Instance.DarkTexture = AssetDatabase.LoadAssetAtPath("Assets/BlobOMatic/Textures/Dark.png", typeof(Texture2D)) as Texture2D;
        Instance.HorizontalDelimeterTex = AssetDatabase.LoadAssetAtPath("Assets/BlobOMatic/Textures/TransparentDark.png", typeof(Texture2D)) as Texture2D;
        ModelGUI.BorderTex = AssetDatabase.LoadAssetAtPath("Assets/BlobOMatic/Textures/Previewborder.png", typeof(Texture2D)) as Texture2D;
        Instance.Show();
        
        models = new Dictionary<Object, ModelGUI>();
        ignoredSelections = new HashSet<Object>();

        ModelCamera = Utils.CreateModelCamera(1 << EditorLayer);
        ModelCamera.gameObject.hideFlags = HideFlags.HideAndDontSave;
        BlobCamera = Utils.CreateBlobCamera(1 << EditorLayer);
        BlobCamera.gameObject.hideFlags = HideFlags.HideAndDontSave;

        // Setup light
        GameObject previewLightPrefab = AssetDatabase.LoadAssetAtPath("Assets/BlobOMatic/Light.prefab", typeof(GameObject)) as GameObject;
        Instance.previewLight = GameObject.Instantiate(previewLightPrefab) as GameObject;
        Instance.previewLight.SetHideFlagsRecursively(HideFlags.HideAndDontSave);
        Instance.previewLight.transform.parent = ModelCamera.transform;
        foreach (Light l in Instance.previewLight.GetComponentsInChildren<Light>(true)) {
            l.color *= (Color.white - RenderSettings.ambientLight);
            l.cullingMask = 1 << EditorLayer;
        }

        // Load and apply settings
        if (SettingsSaver.DoesSaveFileExist())
            Settings.LoadFromStream(SettingsSaver.Restore());
        Instance.ApplySettings();

        // Render already selected models
        Instance.OnSelectionChange();

        Instance.hideFlags = HideFlags.DontSave;
        EditorApplication.playmodeStateChanged += OnPlayToggle;
    }


    static void OnPlayToggle() {
        // Close on playing to avoid serialization problems
        Instance.Close();
    }

    void OnDestroy() {
        if (models != null) {
            foreach (KeyValuePair<Object, ModelGUI> p in models)
                p.Value.Destroy();
            models = null;
            ignoredSelections = null;
        }
        
        if (ModelCamera != null)
            GameObject.DestroyImmediate(ModelCamera.gameObject);
        
        if (BlobCamera != null) {
            GameObject.DestroyImmediate(BlobCamera.targetTexture);
            GameObject.DestroyImmediate(BlobCamera.gameObject);
        }
        GameObject.DestroyImmediate(previewLight);

        EditorLayer = -1;
    }

    void OnSelectionChange() {
        // Rotating model may refer to a deselected models, so deselect it.
        // Since the user is selecting a model in a different window, we should
        // be fine.
        rotatingModel = null;

        // Add selected objects
        foreach(Object o in Selection.GetFiltered(typeof(GameObject),
                                                  SelectionMode.Assets)) {
            
            if (AssetDatabase.IsMainAsset(o)) {
                GameObject asset = o as GameObject;
                if (!(models.ContainsKey(asset) || ignoredSelections.Contains(asset))) {
                    EditorModel model = new EditorModel(asset);
                    if (model.ICanHasShadow) {
                        ModelGUI modelGUI = new ModelGUI(model);
                        models[asset] = modelGUI;
                    } else {
                        ignoredSelections.Add(asset);
                        model.Destroy();
                    }
                }
            }
        }
        
        // Destroy deselected object - TODO Can it be done in one pass for dictionaries?
        List<Object> toBeDestroyed = new List<Object>();
        foreach (KeyValuePair<Object, ModelGUI> p in models)
            if (!Selection.Contains(p.Key))
                toBeDestroyed.Add(p.Key);
        
        foreach (Object o in toBeDestroyed) {
            models[o].Destroy();
            models.Remove(o);
        }

        Repaint();
    }

    void Update() {
 
        if (rotatingModel != null) {
            rotatingModel.VerticalRotation += mouseDelta.x / ModelGUI.PreviewSize * 360;
            rotatingModel.HorizontalRotation += mouseDelta.y / 2.0f;
            rotatingModel.HorizontalRotation = Mathf.Clamp(rotatingModel.HorizontalRotation, 0.0f, 90.0f);
            rotatingModel.QueuePreviewUpdate();
            
            Repaint();
        } 
    }
    
    void OnGUI() {

        float scrollBarWidth = 13.0f;
        float scrollPadding = 2.5f;
        float scrollVerticalSpace = 22.5f;
        //float settingsWidth = Application.platform == RuntimePlatform.WindowsEditor ? 230.0f : 200.0f;
        float settingsWidth = 200.0f;
        float modelPreviewWidth = position.width - settingsWidth - scrollPadding * 2.0f;

        // Draw models
        float modelsHeight = scrollPadding * 2.0f + scrollVerticalSpace * (models.Count-1);
        foreach (KeyValuePair<Object, ModelGUI> p in models)
            modelsHeight += p.Value.Height;
        modelScrollPos = GUI.BeginScrollView(new Rect(scrollPadding, 0, modelPreviewWidth, position.height),
                                             modelScrollPos, new Rect(0, 0, ModelGUI.Width, modelsHeight)); {

            if (models.Count == 0)
                GUI.Label(new Rect(0.0f, 2.5f, ModelGUI.Width, 20), "Select a model or prefab in the Project view.");
            else {
                // Handle model rotation
                mouseDelta = Event.current.delta;
                if (Event.current.type == EventType.Repaint)
                    rotatingModel = null;
                
                float y = scrollPadding;
                foreach (KeyValuePair<Object, ModelGUI> p in models) {
                    if (p.Value.IsDragging(y))
                        rotatingModel = p.Value;
                    p.Value.Draw(y);
                    
                    y += p.Value.Height + scrollVerticalSpace;

                    GUI.DrawTextureWithTexCoords(new Rect(22.5f, y - scrollVerticalSpace / 2 - 2.0f, modelPreviewWidth - 45.0f - scrollBarWidth, 2.5f),
                                                 HorizontalDelimeterTex, new Rect(0, 0, 1, 1));
                }
            }

        } GUI.EndScrollView();

        // Draw vertical line
        GUI.DrawTexture(new Rect(modelPreviewWidth + scrollPadding, 0, scrollPadding, position.height), DarkTexture);
        
        // Draw settings
        {
            bool settingsChanged = false;
        
            float margin = 4.0f;
            Rect elementRect = new Rect(scrollPadding + modelPreviewWidth + margin, 0, settingsWidth - 2.0f * margin, 16.0f);
            float elementOffset = 19.0f;
            float spaceOffset = 6.0f;
    
            BlobGUI.Label(elementRect, "Global Settings:");
            elementRect.y += elementOffset;

            elementRect.y += spaceOffset; // Space

            float oldDistance = Settings.Distance;
            Settings.Distance = Mathf.Max(BlobGUI.FloatField(elementRect, "Distance (%)", Settings.Distance), 0.001f);
            elementRect.y += elementOffset;
            settingsChanged |= oldDistance != Settings.Distance;
            
            float oldMarginSize = Settings.MarginSize;
            Settings.MarginSize = BlobGUI.FloatField(elementRect, "Margin Size (%)", Settings.MarginSize);
            elementRect.y += elementOffset;
            settingsChanged |= oldMarginSize != Settings.MarginSize;

            float oldStr = Settings.Strength;
            Settings.Strength = Mathf.Clamp(BlobGUI.FloatField(elementRect, "Strength (%)", Settings.Strength), 0.0f, 100.0f);
            elementRect.y += elementOffset;
            settingsChanged |= oldStr != Settings.Strength;

            float oldFalloff = Settings.Falloff;
            Settings.Falloff = BlobGUI.FloatField(elementRect, "Falloff Speed", Settings.Falloff);
            elementRect.y += elementOffset;
            settingsChanged |= oldFalloff != Settings.Falloff;
        
            int oldItrs = Settings.BlurIterations;
            Settings.BlurIterations = BlobGUI.IntField(elementRect, "Blur Iterations", Settings.BlurIterations);
            elementRect.y += elementOffset;
            settingsChanged |= oldItrs != Settings.BlurIterations;
            
            float oldFac = Settings.BlurFactor;
            Settings.BlurFactor = Mathf.Clamp01(BlobGUI.FloatField(elementRect, "Blur Factor", Settings.BlurFactor));
            elementRect.y += elementOffset;
            settingsChanged |= oldFac != Settings.BlurFactor;

            float oldTexSize = Settings.TextureSize;
            Settings.TextureSize = BlobGUI.FloatField(elementRect, "Pixels pr. Unit", Settings.TextureSize);
            elementRect.y += elementOffset;
            settingsChanged |= oldTexSize != Settings.TextureSize;

            elementRect.y += 3.0f * spaceOffset; // Space space spac

            BlobGUI.Label(elementRect, "Editor Settings:");
            elementRect.y += elementOffset;

            elementRect.y += spaceOffset; // Space

            bool wasRealtime = Settings.RealtimeUpdate;
            Settings.RealtimeUpdate = BlobGUI.Toggle(elementRect, "Realtime Update", Settings.RealtimeUpdate);
            elementRect.y += elementOffset;
            settingsChanged |= (wasRealtime == false && Settings.RealtimeUpdate);

            bool hadBounderies = Settings.ShowBounderies;
            Settings.ShowBounderies = BlobGUI.Toggle(elementRect, "Show Bounderies", Settings.ShowBounderies);
            elementRect.y += elementOffset;
            bool updatePreview = hadBounderies != Settings.ShowBounderies;
            settingsChanged |= updatePreview;

            elementRect.y += 3.0f * spaceOffset; // Space space space

            // Settings - Buttons so extend the rect a bit
            elementRect.height += 2.0f;
            elementOffset += 3.0f;
            bool updateBlob = false;
            if (GUI.Button(elementRect, "Save Settings"))
                SettingsSaver.Save(Settings.SaveToStream());
            elementRect.y += elementOffset;

            if (GUI.Button(elementRect, "Restore Settings")) {
                if (SettingsSaver.DoesSaveFileExist()) {
                    Settings.LoadFromStream(SettingsSaver.Restore());
                    ApplySettings();
                    updateBlob = true;
                } else
                    Debug.LogWarning("Settings file not found. Please save settings before restoring.");
            }
            elementRect.y += elementOffset;

            GUI.enabled = !Settings.RealtimeUpdate;
            if (GUI.Button(elementRect, "Apply Settings") || (settingsChanged & Settings.RealtimeUpdate)) {
                ApplySettings();
                updateBlob = true;
            }
            GUI.enabled = true;
            elementRect.y += elementOffset;

            if (updateBlob)
                foreach (KeyValuePair<Object, ModelGUI> p in models)
                    p.Value.QueueBlobUpdate();
            else if (updatePreview)
                foreach (KeyValuePair<Object, ModelGUI> p in models)
                    p.Value.QueuePreviewUpdate();

            elementRect.y += spaceOffset; // Space
            
            if (GUI.Button(elementRect, "Generate All Prefabs"))
                foreach (KeyValuePair<Object, ModelGUI> p in models)
                    p.Value.Model.SaveAsPrefab();
            // elementRect.y += elementOffset;
        }        

        // GUILayout.BeginArea(new Rect(scrollPadding + modelPreviewWidth, 400, settingsWidth, position.height)); {

        //     bool settingsChanged = false;

        //     GUILayout.Label("Global Settings:");

        //     EditorGUILayout.Space();

        //     float oldDistance = Settings.Distance;
        //     Settings.Distance = Mathf.Max(EditorGUILayout.FloatField("Distance (%)", Settings.Distance), 0.001f);
        //     settingsChanged |= oldDistance != Settings.Distance;
            
        //     float oldMarginSize = Settings.MarginSize;
        //     Settings.MarginSize = EditorGUILayout.FloatField("Margin Size (%)", Settings.MarginSize);
        //     settingsChanged |= oldMarginSize != Settings.MarginSize;

        //     float oldStr = Settings.Strength;
        //     Settings.Strength = Mathf.Clamp(EditorGUILayout.FloatField("Strength (%)", Settings.Strength), 0.0f, 100.0f);
        //     settingsChanged |= oldStr != Settings.Strength;

        //     float oldFalloff = Settings.Falloff;
        //     Settings.Falloff = EditorGUILayout.FloatField("Falloff Speed", Settings.Falloff);
        //     settingsChanged |= oldFalloff != Settings.Falloff;
        
        //     int oldItrs = Settings.BlurIterations;
        //     Settings.BlurIterations = EditorGUILayout.IntField("Blur Iterations", Settings.BlurIterations);
        //     settingsChanged |= oldItrs != Settings.BlurIterations;
            
        //     float oldFac = Settings.BlurFactor;
        //     Settings.BlurFactor = Mathf.Clamp01(EditorGUILayout.FloatField("Blur Factor", Settings.BlurFactor));
        //     settingsChanged |= oldFac != Settings.BlurFactor;

        //     float oldTexSize = Settings.TextureSize;
        //     Settings.TextureSize = EditorGUILayout.FloatField("Pixels pr. Unit", Settings.TextureSize);
        //     settingsChanged |= oldTexSize != Settings.TextureSize;

        //     EditorGUILayout.Space();
        //     EditorGUILayout.Space();

        //     EditorGUILayout.LabelField("Editor Settings:", "");            

        //     EditorGUILayout.Space();
            
        //     bool wasRealtime = Settings.RealtimeUpdate;
        //     Settings.RealtimeUpdate = EditorGUILayout.Toggle("Realtime Update", Settings.RealtimeUpdate);
        //     settingsChanged |= (wasRealtime == false && Settings.RealtimeUpdate);

        //     bool hadBounderies = Settings.ShowBounderies;
        //     Settings.ShowBounderies = EditorGUILayout.Toggle("Show Bounderies", Settings.ShowBounderies);
        //     bool updatePreview = hadBounderies != Settings.ShowBounderies;
        //     settingsChanged |= updatePreview;

        //     EditorGUILayout.Space();
        //     EditorGUILayout.Space();

        //     bool updateBlob = false;
        //     if (GUILayout.Button("Save Settings"))
        //         SettingsSaver.Save(Settings.SaveToStream());

        //     if (GUILayout.Button("Restore Settings")) {
        //         if (SettingsSaver.DoesSaveFileExist()) {
        //             Settings.LoadFromStream(SettingsSaver.Restore());
        //             ApplySettings();
        //             updateBlob = true;
        //         } else
        //             Debug.LogWarning("Settings file not found. Please save settings before restoring.");
        //     }

        //     GUI.enabled = !Settings.RealtimeUpdate;
        //     if (GUILayout.Button("Apply Settings") || (settingsChanged & Settings.RealtimeUpdate)) {
        //         ApplySettings();
        //         updateBlob = true;
        //     }
        //     GUI.enabled = true;

        //     if (updateBlob)
        //         foreach (KeyValuePair<Object, ModelGUI> p in models)
        //             p.Value.QueueBlobUpdate();
        //     else if (updatePreview)
        //         foreach (KeyValuePair<Object, ModelGUI> p in models)
        //             p.Value.QueuePreviewUpdate();

        //     EditorGUILayout.Space();
            
        //     if (GUILayout.Button("Generate All Prefabs"))
        //         foreach (KeyValuePair<Object, ModelGUI> p in models)
        //             p.Value.Model.SaveAsPrefab();

        // } GUILayout.EndArea();
    }

    void ApplySettings() {
        BlobBlur blobBlur = BlobCamera.GetComponent<BlobBlur>();
        blobBlur.BlurIterations = Settings.BlurIterations;
        Shader.SetGlobalFloat("_BlobFalloffExponent", Settings.Falloff);
    }

    static int FindOrCreateLayer(string name) {
        int layer = LayerMask.NameToLayer("UnityEditor");
        
        if (layer == -1)
            for (int i = 31; i >= 0; --i) {
                string layerName = LayerMask.LayerToName(i);
                if (string.IsNullOrEmpty(layerName)) {
                    layer = i;
                    Debug.LogWarning("No layer named 'UnityEditor' found. Using empty layer " + i + " instead.");
                    break;;
                }
            }
        
        return layer;
    }
}

} // NS BlobOMatic