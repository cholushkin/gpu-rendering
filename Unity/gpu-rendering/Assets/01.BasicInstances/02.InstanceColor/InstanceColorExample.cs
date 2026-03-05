using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Demonstrates GPU instancing with per-instance color.
/// Each tile receives a random color that is passed to the shader.
/// </summary>
public class InstanceColorExample : MonoBehaviour
{
    public Material Material;
    public Camera MainCamera;
    public int GridSize = 32;

    public float ZoomAmplitude = 10f;
    public float ZoomBase = 20f;
    public float ZoomSpeed = 1f;

    private Mesh _quad;

    private List<Matrix4x4> _matrices = new List<Matrix4x4>();
    private List<Vector4> _colors = new List<Vector4>();

    private MaterialPropertyBlock _propertyBlock;


    private void Start()
    {
        // Camera setup
        MainCamera.transform.position = new Vector3(0, 0, 30);
        MainCamera.transform.rotation = Quaternion.Euler(0, 180, 0);
        MainCamera.orthographic = true;
        MainCamera.orthographicSize = 20;

        _quad = GenerateQuad();

        int count = GridSize * GridSize;

        _matrices = new List<Matrix4x4>(count);
        _colors = new List<Vector4>(count);

        _propertyBlock = new MaterialPropertyBlock();

        // Initialize matrices and random colors
        for (int i = 0; i < count; i++)
        {
            _matrices.Add(Matrix4x4.identity);

            Vector4 color = new Vector4(
                Random.value,
                Random.value,
                Random.value,
                1.0f
            );

            _colors.Add(color);
        }

        int estimatedDrawCalls = Mathf.CeilToInt(count / 1023f);

        Debug.Log(
            "[InstanceColorExample]\n" +
            $"Grid Size: {GridSize} x {GridSize}\n" +
            $"Instances: {count}\n" +
            $"Estimated Draw Calls: {estimatedDrawCalls}\n" +
            $"Feature: Per-instance colors"
        );
    }


    private void OnGUI()
    {
        int tileCount = GridSize * GridSize;

        GUI.Box(new Rect(10, 10, 220, 120), "Renderer Stats");

        GUILayout.BeginArea(new Rect(20, 40, 200, 100));

        GUILayout.Label($"Tiles Rendered: {tileCount}");
        GUILayout.Label($"Instances: {tileCount}");

        int drawCalls = Mathf.CeilToInt(tileCount / 1023f);
        GUILayout.Label($"Draw Calls (approx): {drawCalls}");

        GUILayout.Label($"Zoom: {MainCamera.orthographicSize:F2}");

        GUILayout.EndArea();
    }


    private void Update()
    {
        float time = Time.time;

        float zoom = ZoomBase + Mathf.Sin(time * ZoomSpeed) * ZoomAmplitude;
        MainCamera.orthographicSize = zoom;

        int index = 0;

        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                float wave = Mathf.Sin(time + x * 0.3f + y * 0.3f);

                Vector3 position = new Vector3(
                    (x - GridSize * 0.5f) * 1.5f,
                    (y - GridSize * 0.5f) * 1.5f,
                    0
                );

                Quaternion rotation = Quaternion.Euler(
                    0,
                    0,
                    wave * 30f
                );

                _matrices[index] = Matrix4x4.TRS(position, rotation, Vector3.one);

                index++;
            }
        }

        // Upload instance colors
        _propertyBlock.SetVectorArray("_InstanceColor", _colors);

        // Render instances
        Graphics.DrawMeshInstanced(
            _quad,
            0,
            Material,
            _matrices,
            _propertyBlock
        );
    }


    private Mesh GenerateQuad()
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