using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Demonstrates GPU instancing using a StructuredBuffer instead of
/// MaterialPropertyBlock arrays.
///
/// Previous examples:
///     CPU uploads arrays every frame using MaterialPropertyBlock.
///
/// This example:
///     Instance data is uploaded once into a GPU StructuredBuffer.
///     The shader reads instance data directly using SV_InstanceID.
///
/// This removes:
///     • MaterialPropertyBlock arrays
///     • Unity instancing macros
///     • the 1023 instance limit of DrawMeshInstanced
/// </summary>
public class StructuredInstanceBufferExample : MonoBehaviour
{
    /*
    TODO ROADMAP

    Step 01.StructuredInstanceBuffer
    ✔ StructuredBuffer instance data
    ✔ GPU-side instance lookup using SV_InstanceID
    ✔ RenderMeshPrimitives
    ✔ removal of MaterialPropertyBlock arrays

    Next Step
    02.DrawMeshInstancedIndirect
    - GPU controlled instance count
    - indirect draw arguments
    */

    public Material Material;
    public Camera MainCamera;

    public int GridSize = 64;

    public Texture2D[] TileTextures;

    public float ZoomAmplitude = 10f;
    public float ZoomBase = 20f;
    public float ZoomSpeed = 1f;

    [Header("Tile Transform")]
    public float TileScale = 1.0f;
    public float TileSpacing = 1.5f;

    private Mesh _quad;

    // GPU buffer storing instance data
    private GraphicsBuffer _instanceBuffer;

    // texture array used for tile rendering
    private Texture2DArray _tileArray;

    private int _instanceCount;

    private Bounds _bounds;
    private RenderParams _renderParams;

    /// <summary>
    /// Instance data layout stored in the GPU buffer.
    ///
    /// IMPORTANT:
    /// This struct layout must match the shader struct exactly.
    /// </summary>
    struct InstanceData
    {
        public Matrix4x4 transform;
        public float tileIndex;
    }


    private void Start()
    {
        // Configure camera for orthographic grid view
        MainCamera.transform.position = new Vector3(0, 0, 30);
        MainCamera.transform.rotation = Quaternion.Euler(0, 180, 0);
        MainCamera.orthographic = true;
        MainCamera.orthographicSize = 20;

        _quad = GenerateQuad();

        _instanceCount = GridSize * GridSize;

        ComputeBounds();
        CreateTextureArray();
        CreateInstanceBuffer();

        // Setup render params (used by RenderMeshPrimitives)
        _renderParams = new RenderParams(Material);
        _renderParams.worldBounds = _bounds;
    }


    /// <summary>
    /// Computes world bounds for the entire grid.
    /// Required for frustum culling.
    /// </summary>
    private void ComputeBounds()
    {
        float width = GridSize * TileSpacing;
        float height = GridSize * TileSpacing;

        _bounds = new Bounds(
            Vector3.zero,
            new Vector3(width, height, 10f)
        );
    }


    /// <summary>
    /// Creates the GPU instance buffer and uploads instance data.
    /// </summary>
    private void CreateInstanceBuffer()
    {
        InstanceData[] data = new InstanceData[_instanceCount];

        int index = 0;

        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                Vector3 position =
                    new Vector3(
                        (x - GridSize * 0.5f) * TileSpacing,
                        (y - GridSize * 0.5f) * TileSpacing,
                        0
                    );

                Matrix4x4 matrix =
                    Matrix4x4.TRS(
                        position,
                        Quaternion.identity,
                        Vector3.one * TileScale
                    );

                float tileIndex =
                    Random.Range(0, TileTextures.Length);

                data[index].transform = matrix;
                data[index].tileIndex = tileIndex;

                index++;
            }
        }

        /*
        STRIDE EXPLANATION

        Matrix4x4 = 16 floats
        tileIndex = 1 float

        Total floats = 17

        Each float = 4 bytes

        17 * 4 = 68 bytes
        */

        int stride = sizeof(float) * (16 + 1);

        _instanceBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured,
            _instanceCount,
            stride
        );

        _instanceBuffer.SetData(data);

        // Bind buffer to material
        Material.SetBuffer("_InstanceData", _instanceBuffer);
    }


    private void Update()
    {
        float time = Time.time;

        float zoom =
            ZoomBase +
            Mathf.Sin(time * ZoomSpeed) * ZoomAmplitude;

        MainCamera.orthographicSize = zoom;

        // Ensure buffer is bound before rendering
        Material.SetBuffer("_InstanceData", _instanceBuffer);

        Graphics.RenderMeshPrimitives(
            _renderParams,
            _quad,
            0,
            _instanceCount
        );
    }


    /// <summary>
    /// Debug UI
    /// </summary>
    private void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 260, 150), "Renderer Stats");

        GUILayout.BeginArea(new Rect(20, 40, 240, 120));

        GUILayout.Label($"Instances: {_instanceCount}");
        GUILayout.Label($"Rendering: StructuredBuffer");
        GUILayout.Label($"Draw Calls: 1");

        GUILayout.Label($"Zoom: {MainCamera.orthographicSize:F2}");

        if (_tileArray != null)
            GUILayout.Label($"Texture Layers: {_tileArray.depth}");

        GUILayout.EndArea();
    }


    /// <summary>
    /// Builds Texture2DArray used by the shader
    /// </summary>
    private void CreateTextureArray()
    {
        if (TileTextures == null || TileTextures.Length == 0)
        {
            Debug.LogError("No tile textures assigned.");
            return;
        }

        Texture2D first = TileTextures[0];

        int size = first.width;
        TextureFormat format = first.format;

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

        Material.SetTexture("_Tiles", _tileArray);
    }


    /// <summary>
    /// Generates quad mesh used for tiles
    /// </summary>
    private Mesh GenerateQuad()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = new Vector3[]
        {
            new Vector3(-0.5f,-0.5f,0),
            new Vector3(0.5f,-0.5f,0),
            new Vector3(0.5f,0.5f,0),
            new Vector3(-0.5f,0.5f,0)
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

        return mesh;
    }


    /// <summary>
    /// Release GPU resources
    /// </summary>
    private void OnDestroy()
    {
        if (_instanceBuffer != null)
            _instanceBuffer.Release();
    }
}