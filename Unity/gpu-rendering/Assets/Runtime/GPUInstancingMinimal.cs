using UnityEngine;

public class GPUInstancingMinimal : MonoBehaviour
{
    public Material material;
    public Camera mainCamera;

    const int instanceCount = 100;

    Matrix4x4[] matrices;
    Mesh quad;

    void Start()
    {
        // Camera setup
        mainCamera.transform.position = new Vector3(0, 0, 30);
        mainCamera.transform.rotation = Quaternion.Euler(0,180,0);
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 20;

        quad = GenerateQuad();

        matrices = new Matrix4x4[instanceCount];

        // deterministic grid
        int index = 0;

        for (int y = -5; y < 5; y++)
        {
            for (int x = -5; x < 5; x++)
            {
                matrices[index++] =
                    Matrix4x4.TRS(
                        new Vector3(x * 2f, y * 2f, 0),
                        Quaternion.identity,
                        Vector3.one
                    );
            }
        }
    }

    void Update()
    {
        Graphics.DrawMeshInstanced(
            quad,
            0,
            material,
            matrices
        );
    }

    Mesh GenerateQuad()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = new Vector3[]
        {
            new Vector3(-0.5f,-0.5f,0),
            new Vector3( 0.5f,-0.5f,0),
            new Vector3( 0.5f, 0.5f,0),
            new Vector3(-0.5f, 0.5f,0)
        };

        mesh.uv = new Vector2[]
        {
            new Vector2(0,0),
            new Vector2(1,0),
            new Vector2(1,1),
            new Vector2(0,1)
        };

        mesh.triangles = new int[]
        {
            0,1,2,
            0,2,3
        };

        mesh.RecalculateBounds();

        return mesh;
    }
}