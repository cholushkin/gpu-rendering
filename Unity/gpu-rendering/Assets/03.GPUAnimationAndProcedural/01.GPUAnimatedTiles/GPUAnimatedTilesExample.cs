using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Demonstrates GPU animated tiles.
/// Instance transforms are static.
/// Animation is performed in the vertex shader.
/// </summary>
public class GPUAnimatedTilesExample : MonoBehaviour
{
    /*
    TODO ROADMAP

    STEP 01.GPUAnimatedTiles
    ✔ GPU animated tiles
    ✔ vertex shader wave animation
    ✔ CPU uploads instance data only once

    Next Steps
    - GPU procedural tiles
    - noise driven transforms
    */

    public Material Material;
    public Camera MainCamera;

    public Texture2D[] TileTextures;

    public int GridSize = 128;

    public float TileSpacing = 1.5f;
    public float TileScale = 1.0f;

    public float WaveAmplitude = 0.3f;
    public float WaveFrequency = 0.4f;

    Mesh _quad;

    GraphicsBuffer _instanceBuffer;

    Texture2DArray _tileArray;

    int _instanceCount;

    Bounds _bounds;

    struct InstanceData
    {
        public Matrix4x4 transform;
        public Vector2 gridPos;
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
    }

    void SetupCamera()
    {
        MainCamera.transform.position = new Vector3(0,0,60);
        MainCamera.transform.rotation = Quaternion.Euler(0,180,0);

        MainCamera.orthographic = true;
        MainCamera.orthographicSize = 40;
    }

    void ComputeBounds()
    {
        float size = GridSize * TileSpacing;

        _bounds = new Bounds(
            Vector3.zero,
            new Vector3(size,size,10)
        );
    }

    void CreateInstanceBuffer()
    {
        InstanceData[] data = new InstanceData[_instanceCount];

        int index = 0;

        for(int y=0;y<GridSize;y++)
        for(int x=0;x<GridSize;x++)
        {
            Vector3 position = new Vector3(
                (x - GridSize*0.5f)*TileSpacing,
                (y - GridSize*0.5f)*TileSpacing,
                0
            );

            Matrix4x4 matrix =
                Matrix4x4.TRS(
                    position,
                    Quaternion.identity,
                    Vector3.one * TileScale
                );

            data[index].transform = matrix;
            data[index].gridPos = new Vector2(x,y);
            data[index].tileIndex = Random.Range(0,TileTextures.Length);

            index++;
        }

        int stride = sizeof(float)*(16 + 2 + 1);

        _instanceBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured,
            _instanceCount,
            stride
        );

        _instanceBuffer.SetData(data);

        Material.SetBuffer("_InstanceData",_instanceBuffer);
    }

    void Update()
    {
        Material.SetFloat("_TimeValue",Time.time);
        Material.SetFloat("_WaveAmplitude",WaveAmplitude);
        Material.SetFloat("_WaveFrequency",WaveFrequency);

        Graphics.DrawMeshInstancedProcedural(
            _quad,
            0,
            Material,
            _bounds,
            _instanceCount
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

        for(int i=0;i<TileTextures.Length;i++)
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

        Material.SetTexture("_Tiles",_tileArray);
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
        if(_instanceBuffer != null)
            _instanceBuffer.Release();
    }
}