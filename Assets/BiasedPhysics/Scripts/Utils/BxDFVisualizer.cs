using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class BxDFVisualizer : MonoBehaviour {

    public Shader BxDF;
    public int Detail = 64;
    public Vector3 GlobalViewDir = (new Vector3(1,1,1)).normalized;

	void Awake() {
        renderer.material.shader = BxDF;
        
        gameObject.GetComponent<MeshFilter>().sharedMesh = CreatePlane(Detail);
	}
	
    Mesh CreatePlane(int detail) {
        Mesh mesh = new Mesh();
        
        int d = detail+1;
        Vector3[] pos = new Vector3[d*d];
        Vector2[] tcs = new Vector2[d*d];
        for (int y = 0; y < d; ++y)
            for (int x = 0; x < d; ++x) {
                int index = x + y * d;
                pos[index] = new Vector3(x / (float)detail - 0.5f, 0.0f, y / (float)detail - 0.5f);
                
                tcs[index] = new Vector2(x / (float)detail, y / (float)detail);
                //tcs[index] = new Vector2(0.25f, 0.75f);
            }
        mesh.vertices = pos;
        mesh.uv = tcs;

        int[] tris = new int[detail * detail * 6];
        for (int y = 0; y < detail; ++y)
            for (int x = 0; x < detail; ++x) {
                int v0 = x + y * d;
                int v1 = x + (y + 1) * d;
                int v2 = x + 1 + y * d;
                int v3 = x + 1 + (y + 1) * d;
                
                int index = (x + y * detail) * 6;
                
                tris[index+0] = v0; tris[index+1] = v1; tris[index+2] = v2;
                tris[index+3] = v2; tris[index+4] = v1; tris[index+5] = v3;
            }
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        mesh.bounds = new Bounds(Vector3.zero, 
                                 new Vector3(1e30f, 1e30f, 1e30f));

        return mesh;
    }
}
