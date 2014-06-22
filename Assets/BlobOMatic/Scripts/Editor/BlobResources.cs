////////////////////////////////////////////////////////////////////////////////
// Copyright © Asger Vejen Hoedt 2013 All Rights Reserved. No part of this
// document may be reproduced, copied, modified or adapted without the written
// consent from the author.
////////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;

#if false

namespace BlobOMatic {

public class BlobResources {

    [MenuItem("Blob-O-Matic/Create Blob Mesh")]
    public static void CreateBlobMesh() {
        Mesh blobMesh = new Mesh();
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
        blobMesh.RecalculateNormals();
        blobMesh.RecalculateBounds();
        
        AssetDatabase.CreateAsset(blobMesh, "Assets/BlobOMatic/BlobMesh.asset");
    }

    [MenuItem("Blob-O-Matic/Create Dark GUI Texture")]
    public static void CreateDarkGUITexture() {
        Texture2D t = new Texture2D(1,1);
        t.SetPixel(0,0, new Color32(41,41,41,255));
        t.Apply();

        string texPath = Application.dataPath + "/BlobOMatic/Textures/Dark.png";
        System.IO.File.WriteAllBytes(texPath, t.EncodeToPNG());
        AssetDatabase.Refresh();
    }

    [MenuItem("Blob-O-Matic/Create Transparent Dark GUI Texture")]
    public static void CreateTransparentDarkGUITexture() {
        Texture2D t = new Texture2D(1,1);
        t.SetPixel(0,0, new Color32(41,41,41,127));
        t.Apply();

        string texPath = Application.dataPath + "/BlobOMatic/Textures/TransparentDark.png";
        System.IO.File.WriteAllBytes(texPath, t.EncodeToPNG());
        AssetDatabase.Refresh();
    }

    [MenuItem("Blob-O-Matic/Create Plane Texture")]
    public static void CreatePlaneGUITexture() {
        int width = 128;
        Texture2D t = new Texture2D(width, width);
        for (int x = 0; x < width; ++x)
            for (int y = 0; y < width; ++y) {
                bool edge = x == 0 || x == width-1 || y == 0 || y == width-1;
                Color c = edge ? Color.black : Color.white;
                t.SetPixel(x, y, c);
            }
        t.Apply();

        string texPath = Application.dataPath + "/BlobOMatic/Textures/Plane.png";
        System.IO.File.WriteAllBytes(texPath, t.EncodeToPNG());
        AssetDatabase.Refresh();
    }

    [MenuItem("Blob-O-Matic/Create Preview Border Texture")]
    public static void CreatePreviewBorderTexture() {
        int width = ModelGUI.PreviewSize;
        Texture2D t = new Texture2D(width, width);
        for (int x = 0; x < width; ++x)
            for (int y = 0; y < width; ++y) {
                Color c = (x - y <= 0) ? 
                    new Color(0.18f, 0.18f, 0.18f, 1.0f) : 
                    new Color(0.16f, 0.16f, 0.16f, 1.0f);
                int distToEdge = Mathf.Min(Mathf.Min(x, width-x-1),
                                           Mathf.Min(y, width-y-1));
                
                if (distToEdge < ModelGUI.BorderSize) {
                    // c.b = 1.0f;
                    c.a = (float)(distToEdge+1) / (float)ModelGUI.BorderSize;
                } else 
                    c.a = 0.0f;
                t.SetPixel(x, y, c);
            }
        t.Apply();

        string texPath = Application.dataPath + "/BlobOMatic/Textures/PreviewBorder.png";
        System.IO.File.WriteAllBytes(texPath, t.EncodeToPNG());
        AssetDatabase.Refresh();
    }
}

} // NS BlobOMatic

#endif