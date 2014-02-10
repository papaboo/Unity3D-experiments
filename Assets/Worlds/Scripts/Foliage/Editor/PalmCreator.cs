using UnityEditor;
using UnityEngine;
using System.Collections;

namespace Worlds {
namespace Foliage {

public class PalmCreator /* : EditorWindow */ {
    
    [MenuItem("Worlds/Foliage/Create Palms")]
    public static void CreateStems() {
        // int[] stemBlockseeds = { 3299, 773, 4127, 6553, 6481, 163, 5303 };

        // Mesh[] stemBlock = new Mesh[stemBlockseeds.Length];
        // for (int s = 0; s < stemBlockseeds.Length; ++s) {
        //     Mesh stemMesh = CreateStemMesh(stemBlockseeds[s]);
        //     AssetDatabase.CreateAsset(stemMesh, "Assets/Worlds/Meshes/Foliage/Palms/" + stemMesh.name +".asset");
        //     stemBlock[s] = stemMesh;
        // }

        // Material defaultStemMat = AssetDatabase.LoadAssetAtPath("Assets/Worlds/Materials/DefaultPalmStem.mat", typeof(Material)) as Material;

        // int[] stemSeeds = { 3299, 773, 4127, 6553, 6481, 163, 5303 };
        // int[] stemHeights = { 6, 8, 10, 12, 14, 16, 17 };
        // for (int s = 0; s < stemSeeds.Length; ++s) {
        //     GameObject stemGO = CreateStem(stemBlock, defaultStemMat, stemHeights[s], stemSeeds[s]);
        //     PrefabUtility.CreatePrefab("Assets/Worlds/Prefabs/Foliage/Palms/" + stemGO.name + ".prefab", stemGO);
        //     GameObject.DestroyImmediate(stemGO);
        // }

        // int[] leafSeeds = { 3299, 773, 4127, 6553, 6481, 163, 5303 };
        // float[] leafLength = { 6.0f, 8.0f, 10.0f, 12.0f, 14.0f, 16.0f, 17.0f };
        // int[] leafSeeds = { 3299 };
        // float[] leafLength = { 6.0f };
        // for (int s = 0; s < leafSeeds.Length; ++s) {
        //     Mesh leafMesh = CreateLeafMesh(leafSeeds[s], leafLength[s]);
        //     AssetDatabase.CreateAsset(leafMesh, "Assets/Worlds/Meshes/Foliage/Palms/" + leafMesh.name +".asset");
        // }

        int seed = 773;
        Mesh leafMesh = CreateLeafMesh2(seed, 4, 1.0f);
        AssetDatabase.CreateAsset(leafMesh, "Assets/Worlds/Meshes/Foliage/Palms/" + leafMesh.name +"_2.asset");
    }

    public static Mesh CreateStemMesh(int seed) {

        Random.seed = seed;
        
        int sides = 7;
        int quads = sides * 2;
        float height = 0.25f;
        float bottomDiameter = 0.125f;
        float topDiameter = 0.2f;
        
        Vector3[] vertices = new Vector3[quads * 3 + 1];
        int lidOffset = quads * 2;
        int centerIndex = vertices.Length-1;
        float sideRotation = Random.Range(0.0f, 2.0f * Mathf.PI);
        for (int q = 0; q < quads; ++q) {
            float x = Mathf.Cos(sideRotation + q * 2.0f * Mathf.PI / quads);
            float z = Mathf.Sin(sideRotation + q * 2.0f * Mathf.PI / quads);
            
            Vector3 bottom = new Vector3(x * bottomDiameter, 0.0f, z * bottomDiameter);
            Vector3 top = new Vector3(x * topDiameter, height, z * topDiameter);

            float rand = q % 2 == 0 ? Random.Range(0.8f, 0.9f) : Random.Range(1.1f, 1.4f);
            top = bottom + (top - bottom) * rand;
            if ((q % 2) == 1)
                top += new Vector3(x, 0.0f, z) * topDiameter * 0.1f * rand;

            vertices[q * 2] = bottom;
            vertices[q * 2 + 1] = top;
            vertices[lidOffset + q] = top; // For the 'lid'
        }

        vertices[centerIndex] = new Vector3(0.0f, height * 0.5f, 0.0f);

        int[] indices = new int[quads * 6 + quads * 3]; // quads * 6 indices for the sides and quads * 3 for the top
        for (int s = 0; s < sides-1; ++s) {
            AddStemSideIndices(s * 4, s * 4 + 1, s * 4 + 2, s * 4 + 3, s * 4 + 4, s * 4 + 5,
                               lidOffset + s * 2, lidOffset + s * 2 + 1, lidOffset + s * 2 + 2, centerIndex,
                               indices, s * 18);
        }

        int lastSide = (sides - 1) * 4;
        int lastLid = lidOffset + (sides - 1) * 2;
        AddStemSideIndices(lastSide, lastSide + 1, lastSide + 2, lastSide + 3, 0, 1,
                           lastLid, lastLid + 1, lidOffset, centerIndex,
                           indices, (sides - 1) * 18);
        
        Mesh mesh = new Mesh();
        mesh.name = "PalmStem_" + seed;
        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();

        return mesh;
    }

    public static void AddStemSideIndices(int s0, int s1, int s2, int s3, int s4, int s5, // side indices
                                          int t0, int t1, int t2, int center, // top indices
                                          int[] indices, int index) {

        // Side
        indices[index    ] = s0;
        indices[index + 1] = s1;
        indices[index + 2] = s2;
        
        indices[index + 3] = s2;
        indices[index + 4] = s1;
        indices[index + 5] = s3;
        
        indices[index + 6] = s2;
        indices[index + 7] = s3;
        indices[index + 8] = s5;
        
        indices[index + 9] = s2;
        indices[index +10] = s5;
        indices[index +11] = s4;
        
        // Top
        indices[index + 12] = t1;
        indices[index + 13] = t0;
        indices[index + 14] = center;
        
        indices[index + 15] = t2;
        indices[index + 16] = t1;
        indices[index + 17] = center;
    }

    public static GameObject CreateStem(Mesh[] stemBlocks, Material mat, int height, int seed) {
        Random.seed = seed;

        float minAngle = 1.0f;
        float maxAngle = 5.0f;
        
        float angle = Random.Range(minAngle, maxAngle);
        
        // Vertical rotation in the signed range +/- [5, 15] with a difference of 5 between min and max
        float verticalRotationMin = Random.Range(-5.0f, 5.0f);
        verticalRotationMin += verticalRotationMin > 0.0f ? 5.0f : -5.0f;
        float verticalRotationMax = verticalRotationMin + (verticalRotationMin > 0.0f ? 5.0f : -5.0f);

        GameObject stemGO = new GameObject("PalmStem_" + seed);
        stemGO.AddComponent<MeshFilter>().sharedMesh = stemBlocks[Random.Range(0, stemBlocks.Length-1)];
        stemGO.AddComponent<MeshRenderer>().sharedMaterial = mat;
        
        Transform parent = stemGO.transform;
        for (int i = 1; i < height; ++i) {
            GameObject subStemGO = new GameObject("PalmStem" + i);

            subStemGO.AddComponent<MeshFilter>().sharedMesh = stemBlocks[Random.Range(0, stemBlocks.Length-1)];
            subStemGO.AddComponent<MeshRenderer>().sharedMaterial = mat;

            subStemGO.transform.parent = parent;
            float hr = (i * 3 < height * 2) ? angle : (angle * -1.5f);
            subStemGO.transform.localRotation = Quaternion.Euler(hr, Random.Range(verticalRotationMin, verticalRotationMax), 0.0f);
            subStemGO.transform.localPosition = new Vector3(0.0f, 0.125f, 0.0f);

            parent = subStemGO.transform;
        }

        return stemGO;
    }

    private struct BenderRodriguez {
        Vector2 Anchor;
        Vector2 Direction;
        float sign; // used to make sure that we also move from p0 to p1
        
        ////////////////////////////////////////////////////////////////////////
        //
        // For a certain length, L the bend angle is defined as the angle that
        // should be covered on the unit sphere when the length is projectet
        // onto it. This is done by computing an anchor point (center of the
        // sphere) and a direction of length radius along which the angle is
        // zero.
        //     
        // p0 ----------+---------- p1
        //    \       |_|         /
        //     \        |        /
        //      \       |       /
        //       \      |      /
        //        \     |H    /
        //       R \    |    /
        //          \   |   /
        //           \hA|  /
        //            \ | /
        //             \|/
        //            anchor
        ////////////////////////////////////////////////////////////////////////
        
        // Length * 0.5 / sin(angle * 0.5) = R / sin(90)
        public BenderRodriguez(Vector2 p0, Vector2 p1, float bendAngle) {
            Vector2 p0ToP1 = p1 - p0;
            Vector2 tangent = new Vector2(-p0ToP1.y, p0ToP1.x);
            // Debug.Log("tangent: " + tangent.ToString("0.000"));

            float length = p0ToP1.magnitude;
            // Debug.Log("length: " + length);
            float halfRads = (0.5f * bendAngle) * Mathf.Deg2Rad;
            // Debug.Log("half rads: " + halfRads);
            float radius = (length * 0.5f) / Mathf.Sin(halfRads);
            // Debug.Log("radius: " + radius);

            float height = Mathf.Sqrt(radius * radius - (length * 0.5f) * (length * 0.5f));
            // Debug.Log("height: " + height);
            Vector2 middle = (p0 + p1) * 0.5f;
            Anchor = middle + tangent.normalized * height;
            // Debug.Log("Anchor: " + Anchor.ToString("0.000"));
            
            Direction = p0 - Anchor;
            // Debug.Log("Direction: " + Direction.ToString("0.000"));
            
            // Debug.Log("|p0, Anchor| " + (p0 - Anchor).magnitude);
            // Debug.Log("|p1, Anchor| " + (p1 - Anchor).magnitude);

            // Dumb way of setting the sign, but hey it's Editor time code, so
            // dumb code, fast development is great.
            sign = 1.0f;

            Vector2 maybeP1 = GetPosition(bendAngle);
            // Debug.Log("maybeP1: " + maybeP1.ToString("0.000"));
            sign = (maybeP1 - p1).magnitude < length ? 1.0f : -1.0f;
            // Debug.Log("sign: " + sign);
        }
        
        public Vector2 GetPosition(float angle) {
            float rads = sign * angle * Mathf.Deg2Rad;

            Vector2 tangent = new Vector2(-Direction.y, Direction.x);
            Vector2 dir = Direction * Mathf.Cos(rads) + tangent * Mathf.Sin(rads);
            // Debug.Log("GetPosition(" + angle + ") with Anchor: " + Anchor.ToString("0.000") + ", Direction: " + Direction.ToString("0.000") + "\n" +
            //           "tangent: " + tangent.ToString("0.000") + "\n" + 
            //           "dir = " + Direction.ToString("0.000") + " * " + Mathf.Cos(rads) + " + " + tangent.ToString("0.000") + " * " + Mathf.Sin(rads) + " = " + dir.ToString("0.000") + "\n" +
            //           "position: " + (Anchor + dir).ToString("0.000"));
            
            return Anchor + dir;
        }
    }
    
    public static Mesh CreateLeafMesh(int seed, float length) {
        Random.seed = seed;

        float outerAngle = 90.0f;
        Vector2 outerP0 = Vector2.zero;
        Vector2 outerP1 = new Vector2(0.0f, length);
        BenderRodriguez outerBend = new BenderRodriguez(outerP0, outerP1, outerAngle);

        int quads = 7;
        
        // TODO Add a 'spine'
        
        Vector3[] verts = new Vector3[(quads+1) * 2];
        for (int q = 0; q <= quads; ++q) {
            float delta = q / (float)quads;

            Vector2 outerPos = outerBend.GetPosition(outerAngle * delta * delta);
            verts[2*q  ] = outerPos;
            outerPos.x *= -1.0f;
            verts[2*q+1] = outerPos;
        }

        int[] indices = new int[quads * 6];
        for (int q = 0; q < quads; ++q) {
            indices[6*q  ] = 2*q;
            indices[6*q+1] = 2*q+2;
            indices[6*q+2] = 2*q+1;

            indices[6*q+3] = 2*q+1;
            indices[6*q+4] = 2*q+2;
            indices[6*q+5] = 2*q+3;
        }

        Mesh mesh = new Mesh();
        mesh.name = "PalmLeaf_" + seed;
        mesh.vertices = verts;
        mesh.triangles = indices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();

        return mesh;
    }

    public static Mesh CreateLeafMesh2(int seed, int leaflets, float length) {
        Random.seed = seed;

        float outerAngle = 110.0f;
        Vector2 outerP0 = Vector2.zero;
        Vector2 outerP1 = new Vector2(0.0f, length);
        BenderRodriguez outerBend = new BenderRodriguez(outerP0, outerP1, outerAngle);

        float innerAngle = 60.0f;
        BenderRodriguez innerBend = new BenderRodriguez(outerP0, outerP1, innerAngle);

        int leafletDetail = 1; // 1 or above
        int vertsPrLeaflet = leafletDetail + 2;
        int vertexCountPrSide = leaflets * vertsPrLeaflet;
        Vector3[] verts = new Vector3[vertexCountPrSide * 2];
        Debug.Log("||vertices|| " + verts.Length);

        // Build the first side of the leaf
        verts[0] = Vector3.zero;
        for (int l = 0; l < leaflets; ++l) {
            
            // Vertex on 'spine'
            float spineDelta = (l + 0.3f) / (float)leaflets;
            verts[1 + l * vertsPrLeaflet] = new Vector3(0.0f, 0.0f, spineDelta * length);

            // Outer vertex
            float outerDelta = (l + 1.0f) / (float)leaflets;
            Vector2 outerPos = outerBend.GetPosition(outerAngle * outerDelta);
            verts[1 + l * vertsPrLeaflet + 1] = new Vector3(outerPos.x, -0.4f * outerPos.x, outerPos.y);

            // Inner vertex if not last leaflet
            if (l < leaflets-1) {
                float innerDelta = (l + 0.9f) / (float)leaflets;
                Vector2 innerPos = innerBend.GetPosition(innerAngle * innerDelta);
                verts[1 + l * vertsPrLeaflet + 2] = new Vector3(innerPos.x, -0.4f * innerPos.x, innerPos.y);
            }
        }
        // Hard set last vertex to length away from origin to avoit precision errors
        verts[vertexCountPrSide-1] = new Vector3(0.0f, 0.0f, length);

        // Build the second side of the leaf
        for (int v = 0; v < vertexCountPrSide; ++v) {
            Vector3 vert = verts[v];
            vert.x *= -1.0f;
            verts[v + vertexCountPrSide] = vert;
        }

        string vertsStr = "";
        foreach (Vector3 v in verts)
            vertsStr += v.ToString("0.000");
        Debug.Log(vertsStr);
        

        // Index the first side of the leaf
        int trisPrSide = leaflets * leafletDetail + (leaflets - 1) * 2;
        int indicesPrSide = trisPrSide * 3;
        int[] indices = new int[indicesPrSide * 2];
        Debug.Log("||indices|| " + indices.Length);
        int prevSpineIndex = 1 - vertsPrLeaflet;
        int i = 0;
        while (i < indicesPrSide - 1) {
            int spineIndex = prevSpineIndex + vertsPrLeaflet;
            
            indices[i] = spineIndex-1;
            ++i;
            indices[i] = spineIndex+1;
            ++i;
            indices[i] = spineIndex;
            ++i;

            for (int l = 0; l < leafletDetail && i < indices.Length-1; ++l) {
                indices[i] = spineIndex;
                ++i;
                indices[i] = spineIndex + l + 1;
                ++i;
                indices[i] = spineIndex + l + 2;
                ++i;
            }
            
            if (i < indices.Length-1) {
                int nextSpineIndex = spineIndex + vertsPrLeaflet;
                indices[i] = spineIndex;
                ++i;
                indices[i] = nextSpineIndex - 1;
                ++i;
                indices[i] = nextSpineIndex;
                ++i;
            }
            
            prevSpineIndex = spineIndex;
        }

        // Index the second side of the leaf
        for (i = 0; i < indicesPrSide; ++i)
            indices[indicesPrSide + i] = indices[i] + vertexCountPrSide;
        
        string trisStr = "";
        for (i = 0; i < indices.Length / 3; ++i)
            trisStr += "[" + indices[i*3] + ", " + indices[i*3+1] + ", " + indices[i*3+2] + "], ";
        Debug.Log(trisStr);

        Mesh mesh = new Mesh();
        mesh.name = "PalmLeaf_" + seed;
        mesh.vertices = verts;
        mesh.triangles = indices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();

        return mesh;
    }
    
}

} // NS Foliage
} // NS Worlds