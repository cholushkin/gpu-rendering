using UnityEngine;

/// <summary>
/// STEP 02 — SDF Texture Generator Example
///
/// Generates an SDF texture using a compute shader
/// and displays it on screen.
/// </summary>
public class SDFTextureGeneratorExample : MonoBehaviour
{
    [Header("Rendering")]

    public Camera MainCamera;

    public Material PreviewMaterial;

    public ComputeShader Generator;

    [Header("SDF Settings")]

    public int Resolution = 128;

    [Tooltip("0 Circle, 1 Box, 2 Line, 3 Cross")]
    public int ShapeType = 0;

    public Vector4 ShapeParamsA = new Vector4(0.35f,0.3f,0.05f,0);

    public Vector4 ShapeParamsB = Vector4.zero;

    Mesh _quad;

    SDFTileLibrary _library;

    void Start()
    {
        SetupCamera();

        _quad = GenerateQuad();

        _library = new SDFTileLibrary(
            Generator,
            Resolution
        );

        GenerateTexture();
    }

    void Update()
    {
        Graphics.DrawMesh(
            _quad,
            Matrix4x4.identity,
            PreviewMaterial,
            0
        );
    }

    void GenerateTexture()
    {
        _library.GenerateShape(
            ShapeType,
            ShapeParamsA,
            ShapeParamsB
        );

        PreviewMaterial.SetTexture(
            "_MainTex",
            _library.Texture
        );
    }

    void SetupCamera()
    {
        MainCamera.transform.position = new Vector3(0, 0, 30);
        MainCamera.transform.rotation = Quaternion.Euler(0, 180, 0);

        MainCamera.orthographic = true;
        MainCamera.orthographicSize = 1;
    }

    Mesh GenerateQuad()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = new Vector3[]
        {
            new Vector3(-1,-1,0),
            new Vector3(1,-1,0),
            new Vector3(1,1,0),
            new Vector3(-1,1,0)
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
        if(_library != null)
        {
            _library.Release();
        }
    }
}