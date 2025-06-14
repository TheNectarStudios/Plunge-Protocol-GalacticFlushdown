using UnityEngine;

public class UVModifier : MonoBehaviour
{
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector2[] uvs = mesh.uv;

        for (int i = 0; i < uvs.Length; i++)
        {
            // Example: Scale all UVs
            uvs[i] *= 0.5f;
        }

        mesh.uv = uvs;
    }
}