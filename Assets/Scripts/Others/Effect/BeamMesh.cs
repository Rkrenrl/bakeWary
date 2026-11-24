using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BeamMesh : MonoBehaviour
{
    public float height = 15f;
    public float bottomRadius = 1.2f;
    public float topRadius = 0.6f;
    public int segments = 32;

    void Awake()
    {
        Mesh mesh = new Mesh { name = "BeamMesh" };

        Vector3[] verts = new Vector3[(segments + 1) * 2];
        Vector2[] uvs = new Vector2[verts.Length];
        int[] tris = new int[segments * 6];

        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            float ang = t * Mathf.PI * 2f;
            float cos = Mathf.Cos(ang), sin = Mathf.Sin(ang);

            verts[i * 2]     = new Vector3(cos * bottomRadius, 0f, sin * bottomRadius);
            verts[i * 2 + 1] = new Vector3(cos * topRadius, height, sin * topRadius);
            uvs[i * 2]       = new Vector2(t, 0f);
            uvs[i * 2 + 1]   = new Vector2(t, 1f);
        }

        for (int i = 0; i < segments; i++)
        {
            int b = i * 2;
            int ti = i * 6;
            tris[ti]     = b;     tris[ti + 1] = b + 1; tris[ti + 2] = b + 2;
            tris[ti + 3] = b + 1; tris[ti + 4] = b + 3; tris[ti + 5] = b + 2;
        }

        mesh.vertices = verts;
        mesh.uv = uvs;
        mesh.triangles = tris;
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = mesh;
    }
}