using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// STEP 04 — SDF Tile Renderer Example
///
/// Generates procedural SDF tiles and renders them
/// using GPU instancing.
/// </summary>
public class SDFTileRendererExample : MonoBehaviour
{
    public ComputeShader Generator;

    public Material TileMaterial;

    public Camera MainCamera;

    public int Resolution = 128;

    public int TileCount = 4;

    public int GridSize = 16;

    RenderTexture _tileArray;

    ComputeBuffer _commandBuffer;

    Mesh _quad;

    List<Matrix4x4> _matrices;

    float[] _tileIndices;

    MaterialPropertyBlock _props;

    int _kernel;

    void Start()
    {
        SetupCamera();

        _quad = GenerateQuad();

        _kernel = Generator.FindKernel("GenerateTiles");

        CreateTileArray();

        GenerateTiles();

        CreateGrid();
    }

    void CreateTileArray()
    {
        _tileArray = new RenderTexture(
            Resolution,
            Resolution,
            0,
            RenderTextureFormat.ARGBFloat
        );

        _tileArray.dimension =
            UnityEngine.Rendering.TextureDimension.Tex2DArray;

        _tileArray.volumeDepth = TileCount;

        _tileArray.enableRandomWrite = true;

        _tileArray.Create();

        TileMaterial.SetTexture("_Tiles",_tileArray);
    }

    void GenerateTiles()
    {
        for(int tile=0;tile<TileCount;tile++)
        {
            SDFCommand[] commands =
            {
                new SDFCommand
                {
                    shapeType = tile % 4,
                    operation = 0,
                    paramsA = new Vector4(0.3f,0.2f,0.03f,0),
                    paramsB = Vector4.zero
                }
            };

            _commandBuffer =
                new ComputeBuffer(1,System.Runtime.InteropServices.Marshal.SizeOf(typeof(SDFCommand)));

            _commandBuffer.SetData(commands);

            Generator.SetBuffer(_kernel,"Commands",_commandBuffer);

            Generator.SetInt("CommandCount",1);

            Generator.SetTexture(_kernel,"Result",_tileArray);

            Generator.SetInt("Resolution",Resolution);

            Generator.SetInt("TileIndex",tile);

            int groups = Mathf.CeilToInt(Resolution/8f);

            Generator.Dispatch(_kernel,groups,groups,1);

            _commandBuffer.Release();
        }
    }

    void CreateGrid()
    {
        int count = GridSize*GridSize;

        _matrices = new List<Matrix4x4>(count);

        _tileIndices = new float[count];

        _props = new MaterialPropertyBlock();

        int index=0;

        for(int y=0;y<GridSize;y++)
        for(int x=0;x<GridSize;x++)
        {
            Vector3 pos = new Vector3(
                x-GridSize*0.5f,
                y-GridSize*0.5f,
                0
            );

            _matrices.Add(
                Matrix4x4.TRS(pos,Quaternion.identity,Vector3.one)
            );

            _tileIndices[index] =
                Random.Range(0,TileCount);

            index++;
        }
    }

    void Update()
    {
        _props.SetFloatArray("_TileIndex",_tileIndices);

        Graphics.DrawMeshInstanced(
            _quad,
            0,
            TileMaterial,
            _matrices,
            _props
        );
    }

    void SetupCamera()
    {
        MainCamera.transform.position = new Vector3(0, 0, 30);
        MainCamera.transform.rotation = Quaternion.Euler(0, 180, 0);

        MainCamera.orthographic = true;
        MainCamera.orthographicSize = 10;
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