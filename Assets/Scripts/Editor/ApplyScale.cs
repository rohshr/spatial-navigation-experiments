using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class ApplyScale : MonoBehaviour
{
    [ContextMenu("Apply Scale")]
    void ApplyMeshScale()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        Mesh original = mf.sharedMesh;
        Mesh mesh = Instantiate(original);

        Vector3[] vertices = mesh.vertices;
        Vector3 scale = transform.localScale;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = Vector3.Scale(vertices[i], scale);
        }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mf.sharedMesh = mesh;

        transform.localScale = Vector3.one;
    }
}
