using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Step 03 Example
///
/// Demonstrates GPU SDF command interpreter.
/// </summary>
public class SDFCommandInterpreterExample : MonoBehaviour
{
    public ComputeShader Interpreter;

    public Material PreviewMaterial;

    public Camera MainCamera;

    public int Resolution = 128;

    RenderTexture _texture;

    ComputeBuffer _commandBuffer;

    int _kernel;

    Mesh _quad;

    void Start()
    {
        SetupCamera();

        _quad = GenerateQuad();

        _kernel = Interpreter.FindKernel("GenerateFromCommands");

        CreateTexture();

        CreateCommands();

        Dispatch();
    }

    void Dispatch()
    {
        int groups = Mathf.CeilToInt(Resolution / 8f);

        Interpreter.SetTexture(_kernel,"Result",_texture);

        Interpreter.SetInt("Resolution",Resolution);

        Interpreter.SetBuffer(_kernel,"Commands",_commandBuffer);

        Interpreter.SetInt("CommandCount",3);

        Interpreter.Dispatch(_kernel,groups,groups,1);

        PreviewMaterial.SetTexture("_MainTex",_texture);
    }

    void CreateTexture()
    {
        _texture = new RenderTexture(
            Resolution,
            Resolution,
            0,
            RenderTextureFormat.ARGBFloat
        );

        _texture.enableRandomWrite = true;
        _texture.Create();
    }

    void CreateCommands()
    {
        SDFCommand[] commands = new SDFCommand[3];

        // circle
        commands[0] = new SDFCommand
        {
            shapeType = 0,
            operation = 0,
            paramsA = new Vector4(0.4f,0,0,0),
            paramsB = Vector4.zero
        };

        // box union
        commands[1] = new SDFCommand
        {
            shapeType = 1,
            operation = 0,
            paramsA = new Vector4(0.4f,0.3f,0,0),
            paramsB = Vector4.zero
        };

        // line subtract
        commands[2] = new SDFCommand
        {
            shapeType = 2,
            operation = 1,
            paramsA = new Vector4(0,0,0.03f,0),
            paramsB = Vector4.zero
        };

        _commandBuffer = new ComputeBuffer(
            commands.Length,
            Marshal.SizeOf(typeof(SDFCommand))
        );

        _commandBuffer.SetData(commands);
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

    void SetupCamera()
    {
        MainCamera.transform.position = new Vector3(0,0,5);
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
}