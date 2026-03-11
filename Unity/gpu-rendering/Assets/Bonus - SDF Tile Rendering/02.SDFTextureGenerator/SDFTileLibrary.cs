using UnityEngine;

/// <summary>
/// SDFTileLibrary
///
/// Generates Signed Distance Field textures
/// using a compute shader.
/// </summary>
public class SDFTileLibrary
{
    /*
    TODO ROADMAP

    Step 02.SDFTextureGenerator
    ✔ compute shader SDF generation
    ✔ configurable resolution
    ✔ reusable tile generator

    Step 03.SDFCommandInterpreter
    - command buffer
    - multiple shape composition

    Step 04.SDFTextureArrayTiles
    - generate multiple tiles
    - build Texture2DArray
    - feed tile renderer
    */

    ComputeShader _compute;
    RenderTexture _texture;

    int _kernel;
    int _resolution;

    public RenderTexture Texture => _texture;

    public int Resolution => _resolution;

    public SDFTileLibrary(
        ComputeShader compute,
        int resolution)
    {
        _compute = compute;
        _resolution = resolution;

        _kernel = _compute.FindKernel("GenerateShape");

        AllocateTexture();
    }

    void AllocateTexture()
    {
        _texture = new RenderTexture(
            _resolution,
            _resolution,
            0,
            RenderTextureFormat.ARGBFloat
        );

        _texture.enableRandomWrite = true;

        _texture.filterMode = FilterMode.Bilinear;
        _texture.wrapMode = TextureWrapMode.Clamp;

        _texture.Create();
    }

    public void GenerateShape(
        int shapeType,
        Vector4 paramsA,
        Vector4 paramsB)
    {
        _compute.SetTexture(
            _kernel,
            "Result",
            _texture
        );

        _compute.SetInt(
            "Resolution",
            _resolution
        );

        _compute.SetInt(
            "ShapeType",
            shapeType
        );

        _compute.SetVector(
            "ShapeParamsA",
            paramsA
        );

        _compute.SetVector(
            "ShapeParamsB",
            paramsB
        );

        int groups = Mathf.CeilToInt(
            _resolution / 8.0f
        );

        _compute.Dispatch(
            _kernel,
            groups,
            groups,
            1
        );
    }

    public void Release()
    {
        if(_texture != null)
        {
            _texture.Release();
        }
    }
}