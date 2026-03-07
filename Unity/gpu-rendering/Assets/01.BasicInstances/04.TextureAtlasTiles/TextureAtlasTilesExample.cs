using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Demonstrates GPU instancing using a texture atlas.
/// Each instance selects a tile region from a shared atlas texture.
/// </summary>
public class TextureAtlasTilesExample : MonoBehaviour
{
    /*
    TODO ROADMAP

    Step 04.TextureAtlasTiles
    ✔ atlas texture sampling
    ✔ per-instance atlas UV transform

    Next Steps
    02.GPUInstanceData
    - structured GPU instance buffers
    - remove MaterialPropertyBlock arrays
    */

    public Material Material;
    public Camera MainCamera;

    public int GridSize = 32;

    [Header("Atlas")]
    public Texture2D AtlasTexture;
    public int AtlasColumns = 4;
    public int AtlasRows = 4;

    public float ZoomAmplitude = 10f;
    public float ZoomBase = 20f;
    public float ZoomSpeed = 1f;

    [Header("Tile Transform")]
    public float TileScale = 1.0f;
    public float RotationAmplitude = 30f;
    public float WaveFrequency = 0.3f;
    public float TileSpacing = 1.5f;

    private Mesh _quad;

    private List<Matrix4x4> _matrices;

    // per-instance atlas uv transform
    private Vector4[] _atlasUV;

    private MaterialPropertyBlock _propertyBlock;

    private int _atlasTileCount;


    private void Start()
    {
        MainCamera.transform.position = new Vector3(0,0,30);
        MainCamera.transform.rotation = Quaternion.Euler(0,180,0);
        MainCamera.orthographic = true;
        MainCamera.orthographicSize = 20;

        _quad = GenerateQuad();

        int count = GridSize * GridSize;

        _matrices = new List<Matrix4x4>(count);
        _atlasUV = new Vector4[count];

        _propertyBlock = new MaterialPropertyBlock();

        _atlasTileCount = AtlasColumns * AtlasRows;

        for(int i=0;i<count;i++)
        {
            _matrices.Add(Matrix4x4.identity);

            int tileIndex = Random.Range(0,_atlasTileCount);

            _atlasUV[i] = ComputeAtlasUV(tileIndex);
        }

        Debug.Log(
            $"[TextureAtlasTilesExample]\n" +
            $"Atlas Grid: {AtlasColumns} x {AtlasRows}\n" +
            $"Atlas Tiles: {_atlasTileCount}"
        );
    }


    private void OnGUI()
    {
        int tileCount = GridSize * GridSize;

        GUI.Box(new Rect(10,10,240,150),"Renderer Stats");

        GUILayout.BeginArea(new Rect(20,40,220,120));

        GUILayout.Label($"Tiles Rendered: {tileCount}");
        GUILayout.Label($"Atlas Tiles: {_atlasTileCount}");

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

        Vector3 scaleVector = Vector3.one * TileScale;

        for(int y=0;y<GridSize;y++)
        {
            for(int x=0;x<GridSize;x++)
            {
                float wave =
                    Mathf.Sin(time + x * WaveFrequency + y * WaveFrequency);

                Vector3 position =
                    new Vector3(
                        (x - GridSize * 0.5f) * TileSpacing,
                        (y - GridSize * 0.5f) * TileSpacing,
                        0
                    );

                Quaternion rotation =
                    Quaternion.Euler(0,0,wave * RotationAmplitude);

                _matrices[index] =
                    Matrix4x4.TRS(position,rotation,scaleVector);

                index++;
            }
        }

        _propertyBlock.SetVectorArray("_AtlasUV", _atlasUV);
        _propertyBlock.SetTexture("_Atlas", AtlasTexture);

        Graphics.DrawMeshInstanced(
            _quad,
            0,
            Material,
            _matrices,
            _propertyBlock
        );
    }


    private Vector4 ComputeAtlasUV(int tileIndex)
    {
        float scaleX = 1f / AtlasColumns;
        float scaleY = 1f / AtlasRows;

        int x = tileIndex % AtlasColumns;
        int y = tileIndex / AtlasColumns;

        float offsetX = x * scaleX;
        float offsetY = y * scaleY;

        return new Vector4(scaleX,scaleY,offsetX,offsetY);
    }


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
}