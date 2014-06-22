////////////////////////////////////////////////////////////////////////////////
// Copyright © Asger Vejen Hoedt 2013 All Rights Reserved. No part of this
// document may be reproduced, copied, modified or adapted without the written
// consent from the author.
////////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;

namespace BlobOMatic {

public class ModelGUI {

    public const int TextureSize = 512;
    public const int BorderSize = 6;
    public const int PreviewSize = 2 * BorderSize + TextureSize;
    public static Texture2D BorderTex;
    
    public EditorModel Model { get; private set; }
    private ModelPreview previewer;

    public float Height { get { return 2.0f * 22.5f + PreviewSize + (Model.UseLocalSettings ? 45.0f : 0.0f); } }
    public static float Width { get { return PreviewSize * 2; } }

    private bool updateBlob = false;
    public void QueueBlobUpdate() { updateBlob = true; }

    private bool updatePreview = false;
    public void QueuePreviewUpdate() { updatePreview = true; }

    // Preview rotations
    public float VerticalRotation { 
        get { return previewer.VerticalRotation; } 
        set { previewer.VerticalRotation = value; }
    }
    public float HorizontalRotation { 
        get { return previewer.HorizontalRotation; } 
        set { previewer.HorizontalRotation = value; }
    }


    public ModelGUI(EditorModel model) {
        Model = model;
        previewer = new ModelPreview(model);
    }

    public void Destroy() {
        Model.Destroy();
    }

    /**
     * Draws the GUI for a BlobModel starting horizontally at startY.
     */
    public void Draw(float startY) {
        // Update model textures if needed
        if (updateBlob)
            Model.RenderBlob();
        if  (updateBlob || updatePreview)
            previewer.RenderPreview();
        updateBlob = updatePreview = false;

        float previewX = 0.5f * (PreviewSize - EditorStyles.label.CalcSize(new GUIContent(Model.Name)).x);
        GUI.Label(new Rect(previewX, startY + 2.0f, PreviewSize-50, 20), Model.Name);

        GUI.DrawTexture(new Rect(BorderSize, BorderSize + startY + 22.5f, TextureSize, TextureSize), previewer.Preview, ScaleMode.StretchToFill, false);
        GUI.DrawTexture(new Rect(0, startY + 22.5f, PreviewSize, PreviewSize), BorderTex);

        // Calc blob preview size
        Vector2 blobCenter = new Vector2(PreviewSize + BorderSize + TextureSize * 0.5f,
                                         startY + BorderSize + 22.5f + TextureSize * 0.5f);
        Vector2 blobSize = new Vector2(TextureSize, TextureSize);
        if (Model.BlobShadow.width < Model.BlobShadow.height)
            blobSize.x *= (float)Model.BlobShadow.width / (float)Model.BlobShadow.height;
        else
            blobSize.y *= (float)Model.BlobShadow.height / (float)Model.BlobShadow.width;
        Vector2 upperLeftBlobCorner = new Vector2(blobCenter.x - blobSize.x * 0.5f, blobCenter.y - blobSize.y * 0.5f);

        // Draw blob shadow
        string blobText = "Blob Resolution " + Model.BlobShadow.width + " x " + Model.BlobShadow.height;
        float labelX = 1.5f * PreviewSize - EditorStyles.label.CalcSize(new GUIContent(blobText)).x * 0.5f;
        GUI.Label(new Rect(labelX, startY + 2.0f, PreviewSize, 20), blobText);
        GUI.DrawTexture(new Rect(upperLeftBlobCorner.x, upperLeftBlobCorner.y, blobSize.x, blobSize.y), Model.BlobShadow);
        GUI.DrawTexture(new Rect(PreviewSize, startY + 22.5f, PreviewSize, PreviewSize), BorderTex);
        // Draw blob shadow border
        if (Model.BlobShadow.width > Model.BlobShadow.height) {
            float borderSize = TextureSize * (Model.BlobShadow.width - Model.BlobShadow.height) / Model.BlobShadow.width * 0.5f;
            GUI.DrawTexture(new Rect(upperLeftBlobCorner.x, blobCenter.y - TextureSize * 0.5f, TextureSize,  borderSize), BlobOMatic.Instance.DarkTexture);
            GUI.DrawTexture(new Rect(upperLeftBlobCorner.x, blobCenter.y + TextureSize * 0.5f, TextureSize, -borderSize), BlobOMatic.Instance.DarkTexture);
        } else if (Model.BlobShadow.height > Model.BlobShadow.width) {
            float borderSize = TextureSize * (Model.BlobShadow.height - Model.BlobShadow.width) / Model.BlobShadow.height * 0.5f;
            GUI.DrawTexture(new Rect(blobCenter.x - TextureSize * 0.5f, upperLeftBlobCorner.y,  borderSize, TextureSize), BlobOMatic.Instance.DarkTexture);
            GUI.DrawTexture(new Rect(blobCenter.x + TextureSize * 0.5f, upperLeftBlobCorner.y, -borderSize, TextureSize), BlobOMatic.Instance.DarkTexture);
        }
        
        if (GUI.Button(new Rect(PreviewSize * 2-126, startY + 22.5f + PreviewSize, 126, 20), "Generate Prefab"))
            Model.SaveAsPrefab();
        
        bool usedLocalSettings = Model.UseLocalSettings;
        Model.UseLocalSettings = EditorGUI.Foldout(new Rect(0, startY + 22.5f + PreviewSize, 100, 20), Model.UseLocalSettings, "Local Settings");
        updateBlob = usedLocalSettings != Model.UseLocalSettings;
        if (Model.UseLocalSettings) {
            float y = startY + 22.5f + PreviewSize + 22.5f;

            float settingsWidth = 180.0f;
            float settingsStartX = 12.0f;
            float settingsMargin = 22.5f;

            LocalSettings settings = Model.LocalSettings;
            
            Rect fieldRect = new Rect(settingsStartX, y, settingsWidth, 16.0f);
            float elementOffset = 19.0f;

            // Distance - Strength
            {
                float oldDistance = settings.Distance;
                settings.Distance = Mathf.Max(BlobGUI.FloatField(fieldRect, "Distance (%)", settings.Distance), 0.001f);
                fieldRect.y += elementOffset;
                updateBlob |= oldDistance != settings.Distance;

                float oldStr = settings.Strength;
                settings.Strength = Mathf.Clamp(BlobGUI.FloatField(fieldRect, "Strength (%)", settings.Strength), 0.0f, 100.0f);
                updateBlob |= oldStr != settings.Strength;
            }

            // Vertical line
            GUI.DrawTextureWithTexCoords(new Rect(fieldRect.x + settingsWidth + settingsMargin - 1.0f, y, 2.0f, 35.0f),
                                         BlobOMatic.Instance.HorizontalDelimeterTex, new Rect(0, 0, 1, 1));
            
            // Margin size TODO Convert to use BlobGUI as GUILayout is buggy in 4.2.2f1 in windows.
            fieldRect.x += settingsWidth + 2.0f * settingsMargin;
            GUILayout.BeginArea(new Rect(fieldRect.x, y, settingsWidth, 45.0f)); {
                Vector2 oldMarginSize = settings.MarginSize;
                settings.MarginSize = EditorGUILayout.Vector2Field("Margin Size (%)", settings.MarginSize);
                updateBlob |= oldMarginSize != settings.MarginSize;
            } GUILayout.EndArea();

            // Vertical line
            GUI.DrawTextureWithTexCoords(new Rect(fieldRect.x + settingsWidth + settingsMargin - 1.0f, y, 2.0f, 35.0f),
                                         BlobOMatic.Instance.HorizontalDelimeterTex, new Rect(0, 0, 1, 1));

            // Blurring
            {
                fieldRect.x += settingsWidth + 2.0f * settingsMargin;
                fieldRect.y = y;

                int oldItrs = settings.BlurIterations;
                settings.BlurIterations = BlobGUI.IntField(fieldRect, "Blur Iterations", settings.BlurIterations);
                fieldRect.y += elementOffset;
                updateBlob |= oldItrs != settings.BlurIterations;
                
                float oldFac = settings.BlurFactor;
                settings.BlurFactor = Mathf.Clamp01(BlobGUI.FloatField(fieldRect, "Blur Factor", settings.BlurFactor));
                updateBlob |= oldFac != settings.BlurFactor;
            }

            // Vertical line
            GUI.DrawTextureWithTexCoords(new Rect(fieldRect.x + settingsWidth + settingsMargin - 1.0f, y, 2.0f, 35.0f),
                                         BlobOMatic.Instance.HorizontalDelimeterTex, new Rect(0, 0, 1, 1));

            // Falloff - Texture size
            {
                fieldRect.x += settingsWidth + 2.0f * settingsMargin;
                fieldRect.y = y;
                
                float oldFalloff = settings.Falloff;
                settings.Falloff = BlobGUI.FloatField(fieldRect, "Falloff Speed", settings.Falloff);
                fieldRect.y += elementOffset;
                updateBlob |= oldFalloff != settings.Falloff;

                float oldTexSize = settings.TextureSize;
                settings.TextureSize = BlobGUI.FloatField(fieldRect, "Pixels pr. Unit", settings.TextureSize);
                updateBlob |= oldTexSize != settings.TextureSize;
            }

            // Vertical line
            GUI.DrawTextureWithTexCoords(new Rect(fieldRect.x + settingsWidth + settingsMargin - 1.0f, y, 2.0f, 35.0f),
                                         BlobOMatic.Instance.HorizontalDelimeterTex, new Rect(0, 0, 1, 1));

            // Copy global settings
            // NOTE Can I force it to update fields that are selected by the cursor?
            float copyButtonY = startY + 22.5f + PreviewSize + 39.0f;
            if (GUI.Button(new Rect(PreviewSize * 2-134, copyButtonY, 134, 20), "Copy Global Settings"))
                settings.CopyGlobalSettings();
        }
    }

    public bool IsDragging(float startY) {
        return GUI.RepeatButton(new Rect(0, startY + 22.5f, PreviewSize, PreviewSize), "", GUIStyle.none);
    }

}

} // NS BlobOMatic