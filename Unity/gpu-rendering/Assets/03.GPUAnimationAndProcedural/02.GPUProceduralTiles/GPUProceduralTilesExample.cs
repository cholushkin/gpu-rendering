using UnityEngine;

/// <summary>
/// Demonstrates procedural tile placement directly in the GPU.
/// No instance buffer is used.
/// The shader derives tile position from SV_InstanceID.
/// </summary>
public class GPUProceduralTilesExample : MonoBehaviour
{
    /*
    TODO ROADMAP

    STEP 02.GPUProceduralTiles
    ✔ procedural instance placement
    ✔ no instance buffer
    ✔ transforms derived from instanceID

    Next Steps
    - compute driven instance generation
    */

    public Material Material;
    public Camera MainCamera;

    public Texture2D[] TileTextures;

    public int GridSize = 128;

    public float TileSpacing = 1.5f;

    Mesh _quad;

    int _instanceCount;

    Texture2DArray _tileArray;

    Bounds _bounds;

    void Start()
    {
        SetupCamera();

        _quad = GenerateQuad();

        _instanceCount = GridSize * GridSize;

        CreateTextureArray();

        ComputeBounds();
    }

    void SetupCamera()
    {
        MainCamera.transform.position = new Vector3(0,-95,84);
        MainCamera.transform.rotation = Quaternion.Euler(-50,180,0);

        MainCamera.orthographic = true;
        MainCamera.orthographicSize = 60;
    }

    void ComputeBounds()
    {
        float size = GridSize * TileSpacing;

        _bounds = new Bounds(
            Vector3.zero,
            new Vector3(size,size,10)
        );
    }

    void Update()
    {
        Material.SetInt("_GridSize",GridSize);
        Material.SetFloat("_TileSpacing",TileSpacing);

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
        Material.SetInt("_TileCount",TileTextures.Length);
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
}