using UnityEditor;
using UnityEngine;
using System.Collections;

namespace Planet {

public class PlanetCreator : EditorWindow {

    public static PlanetCreator Instance = null;

    private new string name = "Bob";
    private int detail = 96;
    private float minHeight = 40.0f;
    private float maxHeight = 42.0f;
    private float oceanHeight = 40.4f;
    
    [MenuItem("Worlds/Planet Creator")]
    public static void Creator() {
        Instance = EditorWindow.GetWindow<PlanetCreator>();
        Instance.title = "Planet-O-Matic";
        Instance.Show();
    }

    void OnGUI() {
        GUILayout.BeginArea(new Rect(0, 0, position.width, position.height)); {

            name = EditorGUILayout.TextField("Name", name);

            EditorGUILayout.Space();

            detail = EditorGUILayout.IntField("Detail", detail);
            minHeight = EditorGUILayout.FloatField("Min height", minHeight);
            maxHeight = EditorGUILayout.FloatField("Max height", maxHeight);

            EditorGUILayout.Space();

            oceanHeight = EditorGUILayout.FloatField("Ocean Height", oceanHeight);

            EditorGUILayout.Space();

            if (GUILayout.Button("Create"))
                Create();
                
        } GUILayout.EndArea();
    }

    void Create() {
        
        string planetFolder = Application.dataPath + "/Worlds/Planets/" + name;
        System.IO.Directory.CreateDirectory(planetFolder);

        // TODO Create heightmap

        Land land = CreateLand();
        Ocean ocean = CreateOcean(minHeight, maxHeight, oceanHeight, land.Heightmap);
        
        GameObject planetGO = new GameObject(name);
        Planet planet = planetGO.AddComponent<Planet>();
        planet.Land = land;
        land.transform.parent = planet.transform;
        planet.Ocean = ocean;
        ocean.transform.parent = planet.transform;

        string assetDatabaseFolder = "Assets/Worlds/Planets/" + name + "/";
        PrefabUtility.CreatePrefab(assetDatabaseFolder + name + ".prefab", planetGO);
    }

    Land CreateLand() {
        GameObject landGO = new GameObject("Land");

        Mesh mesh = AssetDatabase.LoadAssetAtPath("Assets/Worlds/Meshes/LandMesh_" + detail +".asset", typeof(Mesh)) as Mesh;
        if (mesh == null) {
            mesh = CreateMesh(detail, maxHeight);
            mesh.name = "Land_" + detail;
            AssetDatabase.CreateAsset(mesh, "Assets/Worlds/Meshes/LandMesh_" + detail +".asset");
        }

        MeshFilter mf = landGO.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        // Ground material
        string assetDatabaseFolder = "Assets/Worlds/Planets/" + name + "/";
        Material groundMat = AssetDatabase.LoadAssetAtPath(assetDatabaseFolder + "LandMat.mat", typeof(Material)) as Material;
        if (groundMat == null) {
            groundMat = new Material(Shader.Find("Planets/Land"));
            AssetDatabase.CreateAsset(groundMat, assetDatabaseFolder + "LandMat.mat");
        }

        MeshRenderer mr = landGO.AddComponent<MeshRenderer>();
        mr.material = groundMat;

        Land land = landGO.AddComponent<Land>();
        land.Size = detail;
        land.MinHeight = minHeight;
        land.MaxHeight = maxHeight;
        land.SandPercentage = (oceanHeight - minHeight) / (maxHeight - minHeight);
        land.InitializeMaterial(detail, minHeight, maxHeight, 0.2f);
        
        return land;
    }

    Ocean CreateOcean(float landMinHeight, float landMaxHeight, float oceanHeight, Cubemap landHeightmap) {
        string assetDatabaseFolder = "Assets/Worlds/Planets/" + name + "/";

        GameObject oceanGO = new GameObject("Ocean");

        // Ocean mesh
        // Mesh mesh = AssetDatabase.LoadAssetAtPath("Assets/Worlds/Meshes/OceanMesh_" + detail +".asset", typeof(Mesh)) as Mesh;
        // if (mesh == null) {
            Mesh mesh = CreateMesh(96, oceanHeight, false);
            mesh.name = "Ocean_" + detail;
            AssetDatabase.CreateAsset(mesh, "Assets/Worlds/Meshes/OceanMesh_" + detail +".asset");
        // }

        MeshFilter mf = oceanGO.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        // Ocean material
        Material oceanMat = AssetDatabase.LoadAssetAtPath("Assets/Worlds/Planets/" + name + "/OceanMat.mat", typeof(Material)) as Material;
        if (oceanMat == null) {
            oceanMat = new Material(Shader.Find("Planets/Ocean"));
            AssetDatabase.CreateAsset(oceanMat, assetDatabaseFolder + "OceanMat.mat");
        }

        MeshRenderer mr = oceanGO.AddComponent<MeshRenderer>();
        mr.material = oceanMat;

        Ocean ocean = oceanGO.AddComponent<Ocean>();
        ocean.OceanHeight = oceanHeight;
        ocean.Initialize(landMinHeight, landMaxHeight, oceanHeight, landHeightmap);

        return ocean;
    }

    //==========================================================================
    // ACTUAL CREATION LOGIC
    //==========================================================================

    public static Mesh CreateMesh(int size, float height, bool normalizedVerts = true) {
        
        // NOTE If I split this into multiple meshes then make sure that each
        // mesh bounding box contains 0,0,0 to make sure that the patch doesn't
        // get culled.
        
        // Create in order PosX, NegX, PosY, NegY, PosZ, NegZ
        
        float invSize = 1.0f / size;
        
        // Create vertices
        Vector3[] vertices = new Vector3[size * size * 6];
        int vIndex = 0;
        foreach (CubemapFace face in System.Enum.GetValues(typeof(CubemapFace)))
            for (int x = 0; x < size; ++x) 
                for (int y = 0; y < size; ++y) {
                    float u = (x + 0.5f) * invSize - 0.5f;
                    float v = (y + 0.5f) * invSize - 0.5f;
                    
                    if (normalizedVerts)
                        vertices[vIndex] = Utils.Cubemap.CubemapDirection(face, u, v).normalized;
                    else 
                        vertices[vIndex] = Utils.Cubemap.CubemapDirection(face, u, v).normalized * height;
                    ++vIndex;
                }
        
        // Create indices
        int sideTriCount = (size-1) * (size-1) * 6 * 6; // Six sides and six indices pr quad
        int edgeTriCount = (size-1) * 12 * 6; // 12 edges of length size-1 and six indices pr quad
        int cornerTriCount = 8 * 3; // 8 corners and 3 indices pr triangle
        int[] indices = new int[sideTriCount + edgeTriCount + cornerTriCount];
        int iIndex = 0;

        // --- Create sides ---
        foreach (CubemapFace face in System.Enum.GetValues(typeof(CubemapFace))) {
            int vertOffset = size * size * (int)face;
            for (int x = 0; x < size-1; ++x) 
                for (int y = 0; y < size-1; ++y) {
                    int i0 = vertOffset + x + y * size;
                    int i1 = i0 + 1;
                    int i2 = i0 + size;
                    int i3 = i1 + size;

                    indices[iIndex]   = i0; indices[iIndex+1] = i1; indices[iIndex+2] = i2;
                    indices[iIndex+3] = i2; indices[iIndex+4] = i1; indices[iIndex+5] = i3;
                    
                    iIndex += 6;
                }
        }

        int posXOffset = 0;
        int negXOffset = posXOffset + size * size;
        int posYOffset = negXOffset + size * size;
        int negYOffset = posYOffset + size * size;
        int posZOffset = negYOffset + size * size;
        int negZOffset = posZOffset + size * size;

        // --- Create edges ---
        { // posX and posZ
            int edge2Offset = posZOffset + (size-1) * size;
            for (int x = 0; x < size-1; ++x) {
                int i0 = x; 
                int i1 = i0+1;
                int i2 = edge2Offset + x;
                int i3 = i2 + 1;
                
                indices[iIndex]   = i0; indices[iIndex+1] = i2; indices[iIndex+2] = i1;
                indices[iIndex+3] = i1; indices[iIndex+4] = i2; indices[iIndex+5] = i3;
                iIndex += 6;
            }
        }

        { // posX and negZ
            int edge1Offset = (size-1) * size;
            int edge2Offset = negZOffset;
            for (int x = 0; x < size-1; ++x) {
                int i0 = edge1Offset + x; 
                int i1 = i0 + 1;
                int i2 = edge2Offset + x;
                int i3 = i2 + 1;
                
                indices[iIndex]   = i0; indices[iIndex+1] = i1; indices[iIndex+2] = i2;
                indices[iIndex+3] = i1; indices[iIndex+4] = i3; indices[iIndex+5] = i2;
                iIndex += 6;
            }
        }

        { // posX and posY
            int edge1Offset = 0;
            int edge2Offset = posYOffset + size * size - 1;
            for (int i = 0; i < size-1; ++i) {
                int i0 = edge1Offset + i * size; 
                int i1 = i0 + size;
                int i2 = edge2Offset - i;
                int i3 = i2 - 1;
                
                indices[iIndex]   = i0; indices[iIndex+1] = i1; indices[iIndex+2] = i2;
                indices[iIndex+3] = i1; indices[iIndex+4] = i3; indices[iIndex+5] = i2;
                iIndex += 6;
            }
        }

        { // posX and negY
            int edge1Offset = size * size - 1;
            int edge2Offset = negYOffset + size * size - 1;
            for (int i = 0; i < size-1; ++i) {
                int i0 = edge1Offset - i * size;
                int i1 = i0 - size;
                int i2 = edge2Offset - i;
                int i3 = i2 - 1;
                
                indices[iIndex]   = i0; indices[iIndex+1] = i1; indices[iIndex+2] = i2;
                indices[iIndex+3] = i1; indices[iIndex+4] = i3; indices[iIndex+5] = i2;
                
                iIndex += 6;
            }
        }

        { // negX and negY
            int edge1Offset = negXOffset + size * size - 1;
            int edge2Offset = negYOffset;
            for (int i = 0; i < size-1; ++i) {
                int i0 = edge1Offset - i * size;
                int i1 = i0 - size;
                int i2 = edge2Offset + i;
                int i3 = i2 + 1;
                
                indices[iIndex]   = i0; indices[iIndex+1] = i1; indices[iIndex+2] = i2;
                indices[iIndex+3] = i1; indices[iIndex+4] = i3; indices[iIndex+5] = i2;
                iIndex += 6;
            }
        }

        { // negX and posY
            int edge1Offset = negXOffset;
            int edge2Offset = posYOffset;
            for (int i = 0; i < size-1; ++i) {
                int i0 = edge1Offset + i * size;
                int i1 = i0 + size;
                int i2 = edge2Offset + i;
                int i3 = i2 + 1;
                
                indices[iIndex]   = i0; indices[iIndex+1] = i1; indices[iIndex+2] = i2;
                indices[iIndex+3] = i1; indices[iIndex+4] = i3; indices[iIndex+5] = i2;
                iIndex += 6;
            }
        }

        { // negX and posZ
            int edge1Offset = negXOffset + size * size - 1;
            int edge2Offset = posZOffset + size - 1;
            for (int i = 0; i < size-1; ++i) {
                int i0 = edge1Offset - i;
                int i1 = i0 - 1;
                int i2 = edge2Offset - i;
                int i3 = i2 - 1;
                
                indices[iIndex]   = i0; indices[iIndex+1] = i2; indices[iIndex+2] = i1;
                indices[iIndex+3] = i2; indices[iIndex+4] = i3; indices[iIndex+5] = i1;
                iIndex += 6;
            }
        }

        { // negX and negZ
            int edge1Offset = negXOffset;
            int edge2Offset = negZOffset + (size-1) * size;
            for (int i = 0; i < size-1; ++i) {
                int i0 = edge1Offset + i;
                int i1 = i0 + 1;
                int i2 = edge2Offset + i;
                int i3 = i2 + 1;
                
                indices[iIndex]   = i0; indices[iIndex+1] = i2; indices[iIndex+2] = i1;
                indices[iIndex+3] = i2; indices[iIndex+4] = i3; indices[iIndex+5] = i1;
                iIndex += 6;
            }
        }

        { // negZ and negY
            int edge1Offset = negZOffset + size * size - 1;
            int edge2Offset = negYOffset + size - 1;
            for (int i = 0; i < size-1; ++i) {
                int i0 = edge1Offset - i * size;
                int i1 = i0 - size;
                int i2 = edge2Offset + i * size;
                int i3 = i2 + size;
                
                indices[iIndex]   = i0; indices[iIndex+1] = i1; indices[iIndex+2] = i2;
                indices[iIndex+3] = i1; indices[iIndex+4] = i3; indices[iIndex+5] = i2;
                iIndex += 6;
            }
        }

        { // negZ and posY
            int edge1Offset = negZOffset;
            int edge2Offset = posYOffset + (size-1) * size;
            for (int i = 0; i < size-1; ++i) {
                int i0 = edge1Offset + i * size;
                int i1 = i0 + size;
                int i2 = edge2Offset - i * size;
                int i3 = i2 - size;
                
                indices[iIndex]   = i0; indices[iIndex+1] = i1; indices[iIndex+2] = i2;
                indices[iIndex+3] = i1; indices[iIndex+4] = i3; indices[iIndex+5] = i2;
                iIndex += 6;
            }
        }

        { // posZ and posY
            int edge1Offset = posZOffset + (size-1) * size;
            int edge2Offset = posYOffset + size * size - 1;
            for (int i = 0; i < size-1; ++i) {
                int i0 = edge1Offset - i * size;
                int i1 = i0 - size;
                int i2 = edge2Offset - i * size;
                int i3 = i2 - size;
                
                indices[iIndex]   = i0; indices[iIndex+1] = i2; indices[iIndex+2] = i1;
                indices[iIndex+3] = i2; indices[iIndex+4] = i3; indices[iIndex+5] = i1;
                iIndex += 6;
            }
        }

        { // posZ and negY
            int edge1Offset = posZOffset + size * size - 1;
            int edge2Offset = negYOffset + (size-1) * size;
            for (int i = 0; i < size-1; ++i) {
                int i0 = edge1Offset - i * size;
                int i1 = i0 - size;
                int i2 = edge2Offset - i * size;
                int i3 = i2 - size;
                
                indices[iIndex]   = i0; indices[iIndex+1] = i1; indices[iIndex+2] = i2;
                indices[iIndex+3] = i1; indices[iIndex+4] = i3; indices[iIndex+5] = i2;
                iIndex += 6;
            }
        }


        // --- Create corners ---
        { // - (-0.5, -0.5, -0.5f)
            int negX01 = negXOffset + size-1;
            int negY01 = negYOffset + size-1;
            int negZ11 = negZOffset + size * size - 1;
            indices[iIndex] = negX01; indices[iIndex+1] = negZ11; indices[iIndex+2] = negY01;
            iIndex += 3;
        }

        { // - (-0.5, -0.5, 0.5f)
            int negX11 = negXOffset + size * size - 1;
            int negY00 = negYOffset;
            int posZ01 = posZOffset + size - 1;
            indices[iIndex] = negX11; indices[iIndex+1] = negY00; indices[iIndex+2] = posZ01;
            iIndex += 3;
        }

        { // - (-0.5, 0.5, -0.5f)
            int negX00 = negXOffset;
            int posY00 = posYOffset;
            int negZ10 = negZOffset + (size-1) * size;
            indices[iIndex] = negX00; indices[iIndex+1] = posY00; indices[iIndex+2] = negZ10;
            iIndex += 3;
        }

        { // - (-0.5, 0.5, 0.5f)
            int negX10 = negXOffset + (size-1) * size;
            int posY01 = posYOffset + size - 1;
            int posZ00 = posZOffset;
            indices[iIndex] = negX10; indices[iIndex+1] = posZ00; indices[iIndex+2] = posY01;
            iIndex += 3;
        }

        { // - (0.5, -0.5, -0.5f)
            int posX11 = posXOffset + size * size - 1;
            int negY11 = negYOffset + size * size - 1;
            int negZ01 = negZOffset + size-1;
            indices[iIndex] = posX11; indices[iIndex+1] = negY11; indices[iIndex+2] = negZ01;
            iIndex += 3;
        }

        { // - (0.5, -0.5, 0.5f)
            int posX01 = posXOffset + size - 1;
            int negY10 = negYOffset + (size-1) * size;
            int posZ11 = posZOffset + size * size - 1;
            indices[iIndex] = posX01; indices[iIndex+1] = posZ11; indices[iIndex+2] = negY10;
            iIndex += 3;
        }

        { // - (0.5, 0.5, -0.5f)
            int posX10 = posXOffset + (size-1) * size;
            int posY10 = posYOffset + (size-1) * size;
            int negZ00 = negZOffset;
            indices[iIndex] = posX10; indices[iIndex+1] = negZ00; indices[iIndex+2] = posY10;
            iIndex += 3;
        }

        { // - (0.5, 0.5, 0.5f)
            int posX00 = posXOffset;
            int posY11 = posYOffset + size * size - 1;
            int posZ10 = posZOffset + (size-1) * size;
            indices[iIndex] = posX00; indices[iIndex+1] = posY11; indices[iIndex+2] = posZ10;
            iIndex += 3;
        }

        Mesh mesh = new Mesh();
        mesh.name = "PlanetMesh";
        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.RecalculateNormals();
        mesh.bounds = new Bounds(Vector3.zero, new Vector3(height * 2, height * 2, height * 2));
        mesh.Optimize();

        return mesh;
    }

}

} // NS Planet
