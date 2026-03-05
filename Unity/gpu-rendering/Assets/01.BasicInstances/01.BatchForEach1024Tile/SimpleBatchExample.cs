using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Demonstrates CPU driven instanced rendering using Graphics.DrawMeshInstanced.
/// A grid of quads is generated and animated every frame.
/// This example serves as the baseline for the GPU instancing tutorial series.
/// </summary>
public class SimpleBatchExample : MonoBehaviour
{
    public Material Material;
    public Camera MainCamera;
    public int GridSize = 32;

    public float ZoomAmplitude = 10f;
    public float ZoomBase = 20f;
    public float ZoomSpeed = 1f;

    private Mesh _quad;
    private List<Matrix4x4> _matrices = new List<Matrix4x4>();


    /// <summary>
    /// Initializes camera setup, generates mesh, and allocates instance buffers.
    /// </summary>
    private void Start()
    {
        // Configure camera for a 2D-style orthographic view of the tile grid
        MainCamera.transform.position = new Vector3(0, 0, 30);
        MainCamera.transform.rotation = Quaternion.Euler(0, 180, 0);
        MainCamera.orthographic = true;
        MainCamera.orthographicSize = 20;

        // Generate the quad mesh used by every tile instance
        _quad = GenerateQuad();

        int count = GridSize * GridSize;

        // Preallocate transform buffer to avoid runtime allocations
        _matrices = new List<Matrix4x4>(count);

        for (int i = 0; i < count; i++)
        {
            _matrices.Add(Matrix4x4.identity);
        }

        int estimatedDrawCalls = Mathf.CeilToInt(count / 1023f);

        // Print important tutorial diagnostics
        Debug.Log(
            "[SimpleBatchExample]\n" +
            $"Grid Size: {GridSize} x {GridSize}\n" +
            $"Instance Count: {count}\n" +
            $"Estimated Draw Calls: {estimatedDrawCalls}\n" +
            $"Instancing Limit Per Call: 1023\n" +
            $"Rendering Method: Graphics.DrawMeshInstanced"
        );
    }


    /// <summary>
    /// Displays simple runtime statistics.
    /// </summary>
    private void OnGUI()
    {
        int tileCount = GridSize * GridSize;

        GUI.Box(new Rect(10, 10, 220, 120), "Renderer Stats");

        GUILayout.BeginArea(new Rect(20, 40, 200, 100));

        GUILayout.Label($"Tiles Rendered: {tileCount}");
        GUILayout.Label($"Grid Size: {GridSize} x {GridSize}");
        GUILayout.Label($"Instances: {tileCount}");

        // Unity limits DrawMeshInstanced to 1023 instances per draw call
        int drawCalls = Mathf.CeilToInt(tileCount / 1023f);
        GUILayout.Label($"Draw Calls (approx): {drawCalls}");

        GUILayout.Label($"Zoom: {MainCamera.orthographicSize:F2}");

        GUILayout.EndArea();
    }


    /// <summary>
    /// Updates instance transforms and submits them to the GPU.
    /// </summary>
    private void Update()
    {
        float time = Time.time;

        // Animate camera zoom using a sine wave
        float zoom = ZoomBase + Mathf.Sin(time * ZoomSpeed) * ZoomAmplitude;
        MainCamera.orthographicSize = zoom;

        int index = 0;

        // Iterate through the tile grid and update transforms
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                // Generate a smooth wave animation across the grid
                float wave = Mathf.Sin(time + x * 0.3f + y * 0.3f);

                // Compute world position of the tile
                Vector3 position = new Vector3(
                    (x - GridSize * 0.5f) * 1.5f,
                    (y - GridSize * 0.5f) * 1.5f,
                    0
                );

                // Rotate tiles based on the wave animation
                Quaternion rotation = Quaternion.Euler(
                    0,
                    0,
                    wave * 30f
                );

                // Store transformation matrix for this instance
                _matrices[index] = Matrix4x4.TRS(position, rotation, Vector3.one);

                index++;
            }
        }

        // Submit instance transforms to the GPU.
        // Unity automatically batches up to 1023 instances per draw call.
        Graphics.DrawMeshInstanced(
            _quad,
            0,
            Material,
            _matrices
        );
    }


    /// <summary>
    /// Creates a simple quad mesh used for each tile instance.
    /// </summary>
    private Mesh GenerateQuad()
    {
        Mesh mesh = new Mesh();

        // Define quad vertices centered around origin
        mesh.vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3( 0.5f, -0.5f, 0),
            new Vector3( 0.5f,  0.5f, 0),
            new Vector3(-0.5f,  0.5f, 0)
        };

        // Two triangles forming the quad
        mesh.triangles = new int[]
        {
            0, 1, 2,
            0, 2, 3
        };

        return mesh;
    }
}