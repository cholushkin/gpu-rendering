using UnityEngine;

public class MassiveTileWorldsExample : MonoBehaviour
{
    /*
    ============================================================
    STEP 03 — Massive Tile Worlds

    TODO ROADMAP

    Foundation
    ✔ Camera-centered tile window
    ✔ Compute-generated tile instances
    ✔ Indirect GPU rendering

    Terrain
    ✔ Noise-based tile height

    Next Steps
    - Chunk streaming
    - GPU frustum culling
    - Biome driven tiles
    ============================================================
    */

    public Camera MainCamera;
    public Transform CamCenter;

    public ComputeShader Generator;
    public Material Material;

    public Texture2D TileTexture;

    [Header("View")]
    public int ViewWidth = 128;
    public int ViewHeight = 64;

    [Header("Tile")]
    public float TileSpacing = 1.2f;
    public float TileScale = 1f;

    [Header("Terrain")]
    public float HeightScale = 3f;

    [Header("Noise")]
    public float NoiseScale = 0.05f;

    [Header("Camera Motion")]
    public float ForwardSpeed = 10f;

    struct InstanceData
    {
        public Vector2 position;
        public float rotation;
        public float scale;
        public float noise;
    }

    ComputeBuffer _instanceBuffer;
    ComputeBuffer _argsBuffer;

    Mesh _quad;

    int _instanceCount;
    int _kernel;

    Bounds _bounds;

    void Start()
    {
        SetupCamera();

        _instanceCount = ViewWidth * ViewHeight;

        _kernel = Generator.FindKernel("GenerateTiles");

        CreateQuad();
        CreateBuffers();

        Material.SetTexture("_MainTex", TileTexture);

        float size = Mathf.Max(ViewWidth, ViewHeight) * TileSpacing;

        _bounds = new Bounds(Vector3.zero, new Vector3(size, size, 1000));
    }

    void SetupCamera()
    {
        CamCenter.position = Vector3.zero;
        CamCenter.rotation = Quaternion.Euler(-33, 180, 0);

        MainCamera.orthographic = true;
        MainCamera.orthographicSize = 30;
    }

    void CreateQuad()
    {
        _quad = new Mesh();

        _quad.vertices = new Vector3[]
        {
            new Vector3(-0.5f,-0.5f,0),
            new Vector3(0.5f,-0.5f,0),
            new Vector3(-0.5f,0.5f,0),
            new Vector3(0.5f,0.5f,0)
        };

        _quad.uv = new Vector2[]
        {
            new Vector2(0,0),
            new Vector2(1,0),
            new Vector2(0,1),
            new Vector2(1,1)
        };

        _quad.triangles = new int[]
        {
            0,2,1,
            2,3,1
        };
    }

    void CreateBuffers()
    {
        int stride =
            sizeof(float)*2 +
            sizeof(float) +
            sizeof(float) +
            sizeof(float);

        _instanceBuffer = new ComputeBuffer(_instanceCount, stride);

        Material.SetBuffer("_InstanceData", _instanceBuffer);

        uint[] args = new uint[5];

        args[0] = _quad.GetIndexCount(0);
        args[1] = (uint)_instanceCount;
        args[2] = _quad.GetIndexStart(0);
        args[3] = _quad.GetBaseVertex(0);
        args[4] = 0;

        _argsBuffer = new ComputeBuffer(
            1,
            args.Length * sizeof(uint),
            ComputeBufferType.IndirectArguments
        );

        _argsBuffer.SetData(args);
    }

    void Update()
    {
        CamCenter.position += Vector3.right * ForwardSpeed * Time.deltaTime;

        Vector3 cam = CamCenter.position;

        //------------------------------------------------
        // Compute shader
        //------------------------------------------------

        Generator.SetBuffer(_kernel,"Result",_instanceBuffer);

        Generator.SetInt("ViewWidth",ViewWidth);
        Generator.SetInt("ViewHeight",ViewHeight);

        Generator.SetFloat("TileSpacing",TileSpacing);
        Generator.SetFloat("TileScale",TileScale);

        Generator.SetFloat("NoiseScale",NoiseScale);

        Generator.SetFloat("CameraX",cam.x);
        Generator.SetFloat("CameraZ",cam.z);

        int gx = Mathf.CeilToInt(ViewWidth/8f);
        int gy = Mathf.CeilToInt(ViewHeight/8f);

        Generator.Dispatch(_kernel,gx,gy,1);

        //------------------------------------------------
        // Rendering
        //------------------------------------------------

        Material.SetVector("_CameraOffset", cam);
        Material.SetFloat("_HeightScale", HeightScale);

        _bounds.center = cam;

        Graphics.DrawMeshInstancedIndirect(
            _quad,
            0,
            Material,
            _bounds,
            _argsBuffer
        );
    }

    void OnDestroy()
    {
        if(_instanceBuffer!=null) _instanceBuffer.Release();
        if(_argsBuffer!=null) _argsBuffer.Release();
    }
}