using UnityEngine;

public class ComputeGeneratedTilesExample : MonoBehaviour
{
    public Camera MainCamera;
    public ComputeShader Generator;
    public Material Material;

    public Texture2D[] TileTextures;

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

    int _instanceCount;
    int _kernel;

    Mesh _quad;
    Bounds _bounds;

    void Start()
    {
        
        MainCamera.transform.position = new Vector3(0,0,30);
        MainCamera.transform.rotation = Quaternion.Euler(0,180,0);
        MainCamera.orthographic = true;
        MainCamera.orthographicSize = 20;
        
        _instanceCount = GridSize * GridSize;

        _kernel = Generator.FindKernel("GenerateTiles");

        CreateQuad();
        CreateInstanceBuffer();
        CreateTextureArray();

        DispatchCompute();
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

    void CreateInstanceBuffer()
    {
        int stride =
            sizeof(float)*2 +   // position
            sizeof(float) +     // rotation
            sizeof(float) +     // scale
            sizeof(uint);       // tileIndex

        _instanceBuffer = new ComputeBuffer(
            _instanceCount,
            stride
        );

        Material.SetBuffer("_InstanceData", _instanceBuffer);

        float size = GridSize * TileSpacing;

        _bounds = new Bounds(
            Vector3.zero,
            new Vector3(size,size,1000)
        );
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

        for (int i = 0; i < TileTextures.Length; i++)
            Graphics.CopyTexture(TileTextures[i],0,0,array,i,0);

        array.Apply(false,true);

        Material.SetTexture("_Tiles",array);
    }

    void DispatchCompute()
    {
        Generator.SetBuffer(_kernel,"Result",_instanceBuffer);

        Generator.SetInt("GridSize",GridSize);
        Generator.SetFloat("TileSpacing",TileSpacing);
        Generator.SetFloat("TileScale",TileScale);
        Generator.SetInt("TileCount",TileTextures.Length);

        int groups = Mathf.CeilToInt(GridSize / 8f);

        Generator.Dispatch(_kernel,groups,groups,1);
    }

    void Update()
    {
        Graphics.DrawMeshInstancedProcedural(
            _quad,
            0,
            Material,
            _bounds,
            _instanceCount
        );
    }

    void OnDestroy()
    {
        if(_instanceBuffer != null)
            _instanceBuffer.Release();
    }
}