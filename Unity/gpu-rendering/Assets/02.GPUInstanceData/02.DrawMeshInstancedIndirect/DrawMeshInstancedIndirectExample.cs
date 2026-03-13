using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Demonstrates GPU-driven rendering using DrawMeshInstancedIndirect.
///
/// Instance data is stored in a StructuredBuffer.
/// Draw parameters are stored in a GPU argument buffer.
/// The GPU reads instance count directly from the argument buffer.
/// </summary>
public class DrawMeshInstancedIndirectExample : MonoBehaviour
{
    /*
    TODO ROADMAP

    STEP 02.DrawMeshInstancedIndirect
    ✔ GPU argument buffer
    ✔ DrawMeshInstancedIndirect
    ✔ GPU-controlled instance count

    Next Steps
    - GPU culling
    - compute-generated instance lists
    - indirect argument buffer updates
    */

    public Material Material;
    public Camera MainCamera;

    public int GridSize = 128;

    public Texture2D[] TileTextures;

    public float ZoomAmplitude = 20f;
    public float ZoomBase = 40f;
    public float ZoomSpeed = 1f;

    [Header("Tile Transform")]
    public float TileScale = 1.0f;
    public float TileSpacing = 1.2f;

    Mesh _quad;

    GraphicsBuffer _instanceBuffer;

    GraphicsBuffer _argsBuffer;

    Texture2DArray _tileArray;

    Bounds _bounds;

    int _instanceCount;

    struct InstanceData
    {
        public Matrix4x4 transform;
        public float tileIndex;
    }

    void Start()
    {
        SetupCamera();

        _quad = GenerateQuad();

        _instanceCount = GridSize * GridSize;

        ComputeBounds();

        CreateTextureArray();

        CreateInstanceBuffer();

        CreateIndirectArguments();
    }

    void SetupCamera()
    {
        MainCamera.transform.position = new Vector3(0, 0, 60);
        MainCamera.transform.rotation = Quaternion.Euler(0, 180, 0);

        MainCamera.orthographic = true;
        MainCamera.orthographicSize = 40;
    }

    void ComputeBounds()
    {
        float size = GridSize * TileSpacing;

        _bounds = new Bounds(
            Vector3.zero,
            new Vector3(size, size, 10)
        );
    }

    void CreateInstanceBuffer()
    {
        InstanceData[] data = new InstanceData[_instanceCount];

        int index = 0;

        for (int y = 0; y < GridSize; y++)
        for (int x = 0; x < GridSize; x++)
        {
            Vector3 position = new Vector3(
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

        int stride = sizeof(float) * (16 + 1);

        _instanceBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured,
            _instanceCount,
            stride
        );

        _instanceBuffer.SetData(data);

        Material.SetBuffer("_InstanceData", _instanceBuffer);
    }

    void CreateIndirectArguments()
    {
        uint[] args = new uint[5];

        args[0] = _quad.GetIndexCount(0);
        args[1] = (uint)_instanceCount;
        args[2] = _quad.GetIndexStart(0);
        args[3] = _quad.GetBaseVertex(0);
        args[4] = 0;

        _argsBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.IndirectArguments,
            1,
            args.Length * sizeof(uint)
        );

        _argsBuffer.SetData(args);
    }

    void Update()
    {
        float time = Time.time;

        float zoom =
            ZoomBase +
            Mathf.Sin(time * ZoomSpeed) * ZoomAmplitude;

        MainCamera.orthographicSize = zoom;

        Material.SetBuffer("_InstanceData", _instanceBuffer);

        Graphics.DrawMeshInstancedIndirect(
            _quad,
            0,
            Material,
            _bounds,
            _argsBuffer
        );
    }

    void CreateTextureArray()
    {
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

    Mesh GenerateQuad()
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

    void OnDestroy()
    {
        if (_instanceBuffer != null)
            _instanceBuffer.Release();

        if (_argsBuffer != null)
            _argsBuffer.Release();
    }
}