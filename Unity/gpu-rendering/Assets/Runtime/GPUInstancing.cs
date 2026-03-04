using UnityEngine;
using System.Runtime.InteropServices;

public class GPUInstancing : MonoBehaviour
{
    public Material material;
    public Camera mainCamera;

    Mesh quad;

    ComputeBuffer instanceBuffer;
    ComputeBuffer argsBuffer;

    struct InstanceData
    {
        public Matrix4x4 transform;
    }

    void Start()
    {
        mainCamera.transform.position = new Vector3(0,0,30);
        mainCamera.transform.rotation = Quaternion.Euler(0,180,0);
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 20;

        quad = GenerateQuad();

        int gridSize = 64;
        int instanceCount = gridSize * gridSize;

        InstanceData[] instances = new InstanceData[instanceCount];

        int index = 0;

        for(int y=0;y<gridSize;y++)
        {
            for(int x=0;x<gridSize;x++)
            {
                Matrix4x4 transform =
                    Matrix4x4.TRS(
                        new Vector3(
                            (x-gridSize*0.5f)*1.5f,
                            (y-gridSize*0.5f)*1.5f,
                            0),
                        Quaternion.identity,
                        Vector3.one
                    );

                instances[index].transform = transform;
                index++;
            }
        }

        instanceBuffer = new ComputeBuffer(
            instanceCount,
            Marshal.SizeOf(typeof(InstanceData))
        );

        instanceBuffer.SetData(instances);

        material.SetBuffer("_Instances",instanceBuffer);

        uint[] args = new uint[5]
        {
            quad.GetIndexCount(0),
            (uint)instanceCount,
            quad.GetIndexStart(0),
            quad.GetBaseVertex(0),
            0
        };

        argsBuffer = new ComputeBuffer(1,5*sizeof(uint),ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);
    }

    void Update()
    {
        Graphics.DrawMeshInstancedIndirect(
            quad,
            0,
            material,
            new Bounds(Vector3.zero,Vector3.one*1000),
            argsBuffer
        );
    }

    void OnDestroy()
    {
        instanceBuffer.Release();
        argsBuffer.Release();
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