using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Demonstrates GPU instancing with texture arrays.
/// Each instance randomly selects one tile from the texture array.
/// </summary>
public class TextureArrayTilesExample : MonoBehaviour
{
    public Material Material;
    public Camera MainCamera;

    public int GridSize = 32;

    public Texture2D[] TileTextures;

    public float ZoomAmplitude = 10f;
    public float ZoomBase = 20f;
    public float ZoomSpeed = 1f;

    private Mesh _quad;

    private List<Matrix4x4> _matrices;
    private List<float> _tileIndices;

    private MaterialPropertyBlock _propertyBlock;

    private Texture2DArray _tileArray;


    private void Start()
    {
        MainCamera.transform.position = new Vector3(0, 0, 30);
        MainCamera.transform.rotation = Quaternion.Euler(0, 180, 0);
        MainCamera.orthographic = true;
        MainCamera.orthographicSize = 20;

        _quad = GenerateQuad();

        int count = GridSize * GridSize;

        _matrices = new List<Matrix4x4>(count);
        _tileIndices = new List<float>(count);

        _propertyBlock = new MaterialPropertyBlock();

        CreateTextureArray();

        for (int i = 0; i < count; i++)
        {
            _matrices.Add(Matrix4x4.identity);

            float tileIndex =
                Random.Range(0, TileTextures.Length);

            _tileIndices.Add(tileIndex);
        }

        Material.SetTexture("_Tiles", _tileArray);

        Debug.Log($"Texture array layers: {_tileArray.depth}");
    }


    /// <summary>
    /// Displays renderer statistics similar to previous tutorial steps.
    /// </summary>
    private void OnGUI()
    {
        int tileCount = GridSize * GridSize;

        GUI.Box(new Rect(10, 10, 240, 150), "Renderer Stats");

        GUILayout.BeginArea(new Rect(20, 40, 220, 120));

        GUILayout.Label($"Tiles Rendered: {tileCount}");
        GUILayout.Label($"Grid Size: {GridSize} x {GridSize}");
        GUILayout.Label($"Instances: {tileCount}");

        if (_tileArray != null)
        {
            GUILayout.Label($"Texture Layers: {_tileArray.depth}");
        }

        int drawCalls = Mathf.CeilToInt(tileCount / 1023f);
        GUILayout.Label($"Draw Calls (approx): {drawCalls}");

        GUILayout.Label($"Zoom: {MainCamera.orthographicSize:F2}");

        GUILayout.EndArea();
    }


    private void Update()
    {
        float time = Time.time;

        float zoom =
            ZoomBase +
            Mathf.Sin(time * ZoomSpeed) * ZoomAmplitude;

        MainCamera.orthographicSize = zoom;

        int index = 0;

        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                float wave =
                    Mathf.Sin(time + x * 0.3f + y * 0.3f);

                Vector3 position =
                    new Vector3(
                        (x - GridSize * 0.5f) * 1.5f,
                        (y - GridSize * 0.5f) * 1.5f,
                        0
                    );

                Quaternion rotation =
                    Quaternion.Euler(0, 0, wave * 30f);

                _matrices[index] =
                    Matrix4x4.TRS(position, rotation, Vector3.one);

                index++;
            }
        }

        _propertyBlock.SetFloatArray("_TileIndex", _tileIndices);

        Graphics.DrawMeshInstanced(
            _quad,
            0,
            Material,
            _matrices,
            _propertyBlock
        );
    }


    private void CreateTextureArray()
    {
        if (TileTextures == null || TileTextures.Length == 0)
        {
            Debug.LogError("TextureArrayTilesExample: No tile textures assigned.");
            return;
        }

        Texture2D first = TileTextures[0];

        int size = first.width;
        TextureFormat format = first.format;

        // Validate textures
        for (int i = 0; i < TileTextures.Length; i++)
        {
            Texture2D tex = TileTextures[i];

            if (tex.width != size || tex.height != size)
            {
                Debug.LogError(
                    $"Tile texture '{tex.name}' has mismatching size. " +
                    $"Expected {size}x{size}, got {tex.width}x{tex.height}"
                );
                return;
            }

            if (tex.format != format)
            {
                Debug.LogError(
                    $"Tile texture '{tex.name}' has mismatching format.\n" +
                    $"Expected format: {format}\n" +
                    $"Actual format: {tex.format}\n\n" +
                    $"All textures used for Texture2DArray must share the same format."
                );
                return;
            }
        }

        _tileArray = new Texture2DArray(
            size,
            size,
            TileTextures.Length,
            format,
            false
        );

        for (int i = 0; i < TileTextures.Length; i++)
        {
            Graphics.CopyTexture(
                TileTextures[i],
                0,
                0,
                _tileArray,
                i,
                0
            );
        }

        _tileArray.Apply();

        Debug.Log(
            $"TextureArray created: {TileTextures.Length} layers, " +
            $"{size}x{size}, format {format}"
        );
    }


    private Mesh GenerateQuad()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3(0.5f, -0.5f, 0),
            new Vector3(0.5f, 0.5f, 0),
            new Vector3(-0.5f, 0.5f, 0)
        };

        mesh.uv = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };

        mesh.triangles = new int[]
        {
            0, 1, 2,
            0, 2, 3
        };

        return mesh;
    }
}