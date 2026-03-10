using UnityEngine;


/// <summary>
/// STEP 01 — SDF Preview + Shape Library
///
/// Demonstrates rendering multiple shapes using Signed Distance Fields.
/// Shapes are selected using a shader parameter.
/// </summary>
public class SDFPreviewExample : MonoBehaviour
{
    /*
    TODO ROADMAP

    Step 01.SDFPreview
    ✔ SDF shape library
    ✔ shape selection via parameter
    ✔ smooth SDF rendering

    Step 02.SDFTextureGenerator
    - GPU compute shader texture generation
    - configurable resolution
    - SDFTileLibrary

    Step 03.SDFCommandInterpreter
    - simple shape command DSL
    - interpreter pipeline

    Step 04.SDFTextureArrayTiles
    - procedural tile atlas
    - GPU instanced tile rendering
    */

    public Material Material;
    public Camera MainCamera;

    Mesh _quad;

    void Start()
    {
        SetupCamera();
        _quad = GenerateQuad();
    }

    void Update()
    {
        Graphics.DrawMesh(
            _quad,
            Matrix4x4.identity,
            Material,
            0
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
            new Vector3( 1,-1,0),
            new Vector3( 1, 1,0),
            new Vector3(-1, 1,0)
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