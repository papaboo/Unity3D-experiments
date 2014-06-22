////////////////////////////////////////////////////////////////////////////////
// Copyright © Asger Vejen Hoedt 2013 All Rights Reserved. No part of this
// document may be reproduced, copied, modified or adapted without the written
// consent from the author.
////////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using BlobOMatic.Editor;

namespace BlobOMatic {

public class EditorModel {
    
    private GameObject prefab;
    private GameObject assetInstance;

    public ABlobModel BlobModel { get; private set; }

    public string Name { get { return BlobModel.name; }}
    public Bounds ModelBounds { get { return BlobModel.ModelBounds; }}

    public string saveAssetFolder;

    public Texture2D BlobShadow { get { return BlobModel.BlobShadow; } }
    public bool ICanHasShadow { get { return BlobModel.ModelBounds.size != Vector3.zero; } }

    private Dictionary<GameObject, int> originalLayers = new Dictionary<GameObject, int>();

    public LocalSettings LocalSettings { get; private set; }
    public bool UseLocalSettings = false;

    public float DistanceSetting { get { return UseLocalSettings ? LocalSettings.Distance : Settings.Distance; } }
    
    public EditorModel(GameObject assetPrefab) {
        prefab = assetPrefab;

        saveAssetFolder = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(prefab));
        
        assetInstance = GameObject.Instantiate(assetPrefab) as GameObject;
        assetInstance.name = assetPrefab.name;
        assetInstance.SetActive(true);

        // Check for LocalSettings on the prefab and use that if possible
        LocalSettings = assetInstance.GetComponentInChildren<LocalSettings>();
        if (LocalSettings != null) {
            UseLocalSettings = true;
            BlobModel = assetInstance.AddComponent<ABlobModel>().InitWithExistingBlob(LocalSettings.gameObject);
        } else {
            Mesh blobMesh = AssetDatabase.LoadAssetAtPath("Assets/BlobOMatic/BlobMesh.asset", typeof(Mesh)) as Mesh;
            BlobModel = assetInstance.AddComponent<ABlobModel>().Init(blobMesh);
            if (!ICanHasShadow) return; // Not a valid model, bugger off!
            
            LocalSettings = BlobModel.BlobRenderer.gameObject.AddComponent<LocalSettings>().CopyGlobalSettings();

            saveAssetFolder +=  "/" + prefab.name + "WithBlob";
        }

        // Collect original layers and set everything to blob layer
        foreach (Transform t in assetInstance.GetComponentsInChildren<Transform>(true))
            originalLayers[t.gameObject] = t.gameObject.layer;
        assetInstance.SetLayerRecursively(BlobOMatic.EditorLayer);

        assetInstance.SetActive(false);
        assetInstance.SetHideFlagsRecursively(HideFlags.HideAndDontSave);

        RenderBlob();
    }

    public void Destroy() {
        if (BlobModel.BlobRenderer != null && BlobModel.BlobRenderer.sharedMaterial != null)
            Material.DestroyImmediate(BlobModel.BlobRenderer.sharedMaterial);
        if (BlobModel.BlobShadow != null)
            GameObject.DestroyImmediate(BlobModel.BlobShadow);
        if (assetInstance)
            GameObject.DestroyImmediate(assetInstance);
    }

    public void RenderBlob() {
        BlobModel.gameObject.SetActive(true);

        BlobBlur blobBlur = BlobOMatic.BlobCamera.GetComponent<BlobBlur>();
        float falloff, blurFactor, textureSize;
        ABlobModel.BlobSizeComputation blobSizeComputation;
        if (UseLocalSettings) {
            BlobOMatic.BlobCamera.farClipPlane = LocalSettings.Distance * 0.01f * BlobModel.ModelBounds.size.y;

            blobBlur.BlurIterations = LocalSettings.BlurIterations;
            blobBlur.Strength = LocalSettings.Strength;
            Shader.SetGlobalFloat("_BlobFalloffExponent", LocalSettings.Falloff);

            falloff = LocalSettings.Falloff;
            blurFactor = LocalSettings.BlurFactor;
            textureSize = LocalSettings.TextureSize;
            blobSizeComputation = LocalSettings.ComputeBlobSize;
        } else {
            BlobOMatic.BlobCamera.farClipPlane = Settings.Distance * 0.01f * BlobModel.ModelBounds.size.y;

            blobBlur.BlurIterations = Settings.BlurIterations;
            blobBlur.Strength = Settings.Strength;
            Shader.SetGlobalFloat("_BlobFalloffExponent", Settings.Falloff);

            falloff = Settings.Falloff;
            blurFactor = Settings.BlurFactor;
            textureSize = Settings.TextureSize;
            blobSizeComputation = Settings.ComputeBlobSize;
        }
        

        BlobModel.RenderBlob(BlobOMatic.BlobCamera, blobSizeComputation, falloff, blurFactor, textureSize);
        BlobModel.gameObject.SetActive(false);
    }

    public void SaveAsPrefab() {
        // Create folder for storing asset
        //string assetPath = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(prefab));
        System.IO.Directory.CreateDirectory(Application.dataPath + saveAssetFolder.Remove(0, 6));
        string assetFolder = saveAssetFolder + "/";

        // Store blob texture
        string texPath = Application.dataPath + assetFolder.Remove(0, 6) + prefab.name + ".png";
		System.IO.File.WriteAllBytes(texPath, BlobModel.BlobShadow.EncodeToPNG());
        AssetDatabase.Refresh();  // Add the new tex to the asset database
        Texture2D blobTex = AssetDatabase.LoadAssetAtPath(assetFolder + prefab.name + ".png", typeof(Texture2D)) as Texture2D;

        // Store blob material with a reference to the new texture
        Material blobMat = new Material(Shader.Find("Unlit/Transparent"));
        blobMat.mainTexture = blobTex;
        AssetDatabase.CreateAsset(blobMat, assetFolder + prefab.name + ".mat");

        // Reset to original layers
        foreach (KeyValuePair<GameObject, int> p in originalLayers)
            p.Key.layer = p.Value;
        BlobModel.BlobRenderer.gameObject.layer = BlobModel.gameObject.layer;

        // Create prefab which references the blob mat
        Material oldBlobMat = BlobModel.BlobRenderer.sharedMaterial;
        BlobModel.BlobRenderer.sharedMaterial = blobMat;
        ABlobModel prefabBlobScript = ABlobModel.Instantiate(BlobModel) as ABlobModel;
        GameObject blobedModelPrefab = prefabBlobScript.gameObject;
        ABlobModel.DestroyImmediate(prefabBlobScript);
        if (!UseLocalSettings)
            foreach(LocalSettings ls in blobedModelPrefab.GetComponentsInChildren<LocalSettings>(true))
                ls.CopyGlobalSettings();

        blobedModelPrefab.SetStaticRecursively(true);
        blobedModelPrefab.SetActive(true);
        PrefabUtility.CreatePrefab(assetFolder + prefab.name + ".prefab", blobedModelPrefab);
        GameObject.DestroyImmediate(blobedModelPrefab);
        
        // Restore the material and reset layers to blob
        BlobModel.BlobRenderer.sharedMaterial = oldBlobMat;
        BlobModel.gameObject.SetLayerRecursively(BlobOMatic.EditorLayer);
    }

}

} // NS BlobOMatic
