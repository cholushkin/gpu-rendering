using UnityEngine;
using System.Collections.Generic;

public class MinimalRenderTest : MonoBehaviour
{
    public Material material;
    public Camera mainCamera;
    public int gridSize = 32;

    
    public float zoomAmplitude = 10f;
    public float zoomBase = 20f;
    public float zoomSpeed = 1f;
        
    Mesh quad;

    List<Matrix4x4> matrices = new List<Matrix4x4>();

    void Start()
    {
        mainCamera.transform.position = new Vector3(0, 0, 30);
        mainCamera.transform.rotation = Quaternion.Euler(0,180,0);
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 20;

        quad = GenerateQuad();

        int count = gridSize * gridSize;

        // preallocate list
        for(int i=0;i<count;i++)
            matrices.Add(Matrix4x4.identity);
    }
    
    void OnGUI()
    {
        int tileCount = gridSize * gridSize;
        GUI.Box(new Rect(10, 10, 220, 120), "Renderer Stats");

        GUILayout.BeginArea(new Rect(20, 40, 200, 100));

        GUILayout.Label($"Tiles Rendered: {tileCount}");
        GUILayout.Label($"Grid Size: {gridSize} x {gridSize}");
        GUILayout.Label($"Instances: {tileCount}");

        int drawCalls = Mathf.CeilToInt(tileCount / 1023f);
        GUILayout.Label($"Draw Calls (approx): {drawCalls}");

        GUILayout.Label($"Zoom: {mainCamera.orthographicSize:F2}");

        GUILayout.EndArea();
    }

    void Update()
    {
        float t = Time.time;

        // animated zoom
        float zoom = zoomBase + Mathf.Sin(t * zoomSpeed) * zoomAmplitude;
        mainCamera.orthographicSize = zoom;

        int index = 0;

        for(int y = 0; y < gridSize; y++)
        {
            for(int x = 0; x < gridSize; x++)
            {
                float wave = Mathf.Sin(t + x * 0.3f + y * 0.3f);

                Vector3 pos =
                    new Vector3(
                        (x - gridSize * 0.5f) * 1.5f,
                        (y - gridSize * 0.5f) * 1.5f,
                        0);

                Quaternion rot =
                    Quaternion.Euler(
                        0,
                        0,
                        wave * 30f
                    );

                matrices[index] =
                    Matrix4x4.TRS(pos, rot, Vector3.one);

                index++;
            }
        }

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

        mesh.triangles = new int[]
        {
            0,1,2,
            0,2,3
        };

        return mesh;
    }
}