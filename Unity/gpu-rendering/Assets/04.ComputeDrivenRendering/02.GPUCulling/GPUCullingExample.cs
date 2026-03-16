using UnityEngine;

public class GPUCullingExample : MonoBehaviour
{
    public ComputeShader Generator;
    public ComputeShader Culling;

    public Material Material;

    public Texture2D[] TileTextures;

    public Camera MainCamera;
    public Transform CamCenter;

    public int GridSize = 128;
    public float TileSpacing = 1.2f;
    public float TileScale = 1f;

    struct InstanceData
    {
        public Vector2 position;
        public float rotation;
        public float scale;
        public uint tileIndex;
    }

    ComputeBuffer _instanceBuffer;
    ComputeBuffer _visibleBuffer;
    ComputeBuffer _argsBuffer;

    int _instanceCount;

    int _genKernel;
    int _cullKernel;

    Mesh _quad;

    int _visibleInstances;

    Bounds _bounds;

    bool _useCulling;

    void Start()
    {
        SetupCamera();

        _useCulling = Culling != null;

        if (_useCulling)
            _cullKernel = Culling.FindKernel("FrustumCull");

        _instanceCount = GridSize * GridSize;

        _genKernel = Generator.FindKernel("GenerateTiles");

        CreateQuad();
        CreateBuffers();
        CreateTextureArray();
        GenerateInstances();

        float worldSize = GridSize * TileSpacing;

        _bounds = new Bounds(
            Vector3.zero,
            new Vector3(worldSize, worldSize, 10)
        );
    }

    void SetupCamera()
    {
        CamCenter.position = new Vector3(0,0,0);
        CamCenter.rotation = Quaternion.Euler(0,180,0);

        MainCamera.orthographic = true;
        MainCamera.orthographicSize = 40;
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
            sizeof(uint);

        _instanceBuffer = new ComputeBuffer(_instanceCount,stride);

        _visibleBuffer = new ComputeBuffer(
            _instanceCount,
            stride,
            ComputeBufferType.Append
        );

        _visibleBuffer.SetCounterValue(0);

        uint[] args = new uint[5];

        args[0] = _quad.GetIndexCount(0);
        args[1] = (uint)_instanceCount;
        args[2] = _quad.GetIndexStart(0);
        args[3] = _quad.GetBaseVertex(0);
        args[4] = 0;

        _argsBuffer = new ComputeBuffer(
            1,
            args.Length*sizeof(uint),
            ComputeBufferType.IndirectArguments
        );

        _argsBuffer.SetData(args);

        Material.SetBuffer("_InstanceData",_instanceBuffer);
    }

    void CreateTextureArray()
    {
        int size = TileTextures[0].width;
        var format = TileTextures[0].graphicsFormat;

        Texture2DArray array = new Texture2DArray(
            size,
            size,
            TileTextures.Length,
            format,
            UnityEngine.Experimental.Rendering.TextureCreationFlags.None
        );

        for(int i=0;i<TileTextures.Length;i++)
        {
            Graphics.CopyTexture(TileTextures[i],0,0,array,i,0);
        }

        array.Apply(false,true);

        Material.SetTexture("_Tiles",array);
    }

    void GenerateInstances()
    {
        Generator.SetBuffer(_genKernel,"Result",_instanceBuffer);

        Generator.SetInt("GridSize",GridSize);
        Generator.SetFloat("TileSpacing",TileSpacing);
        Generator.SetFloat("TileScale",TileScale);
        Generator.SetInt("TileCount",TileTextures.Length);

        int groups = Mathf.CeilToInt(GridSize/8f);

        Generator.Dispatch(_genKernel,groups,groups,1);
    }

    void Update()
    {
        if (_useCulling == false)
        {
            Graphics.DrawMeshInstancedIndirect(
                _quad,
                0,
                Material,
                _bounds,
                _argsBuffer
            );

            _visibleInstances = _instanceCount;

            return;
        }

        //------------------------------------------------
        // Compute camera rect
        //------------------------------------------------

        float h = MainCamera.orthographicSize;
        float w = h * MainCamera.aspect;

        Vector3 c = MainCamera.transform.position;

        Vector4 rect = new Vector4(
            c.x - w,
            c.y - h,
            c.x + w,
            c.y + h
        );

        //------------------------------------------------
        // GPU Culling
        //------------------------------------------------

        _visibleBuffer.SetCounterValue(0);

        uint[] args = new uint[5];
        _argsBuffer.GetData(args);
        args[1] = 0;
        _argsBuffer.SetData(args);

        Culling.SetBuffer(_cullKernel,"Input",_instanceBuffer);
        Culling.SetBuffer(_cullKernel,"Visible",_visibleBuffer);
        Culling.SetBuffer(_cullKernel,"Args",_argsBuffer);

        Culling.SetVector("CameraRect",rect);

        Culling.SetInt("InstanceCount",_instanceCount);

        int groups = Mathf.CeilToInt(_instanceCount/64f);

        Culling.Dispatch(_cullKernel,groups,1,1);

        Material.SetBuffer("_InstanceData",_visibleBuffer);

        _argsBuffer.GetData(args);
        _visibleInstances = (int)args[1];

        Graphics.DrawMeshInstancedIndirect(
            _quad,
            0,
            Material,
            _bounds,
            _argsBuffer
        );
    }

    void OnGUI()
    {
        GUI.Box(new Rect(10,10,220,120),"GPU Culling Stats");

        GUILayout.BeginArea(new Rect(20,40,200,100));

        GUILayout.Label("Generated: " + _instanceCount);
        GUILayout.Label("Visible: " + _visibleInstances);
        GUILayout.Label("Culling Enabled: " + _useCulling);

        GUILayout.EndArea();
    }

    void OnDestroy()
    {
        if(_instanceBuffer!=null) _instanceBuffer.Release();
        if(_visibleBuffer!=null) _visibleBuffer.Release();
        if(_argsBuffer!=null) _argsBuffer.Release();
    }
}