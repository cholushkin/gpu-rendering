// --- Source Blob ---

// --- Start File: Unity\gpu-rendering\Assets\Runtime\GPUInstancing.cs ---

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

// --- End File: Unity\gpu-rendering\Assets\Runtime\GPUInstancing.cs ---

// --- Start File: Unity\gpu-rendering\Assets\Runtime\GPUInstancingMinimal.cs ---

using UnityEngine;

public class GPUInstancingMinimal : MonoBehaviour
{
    public Material material;
    public Camera mainCamera;

    const int instanceCount = 100;

    Matrix4x4[] matrices;
    Mesh quad;

    void Start()
    {
        // Camera setup
        mainCamera.transform.position = new Vector3(0, 0, 30);
        mainCamera.transform.rotation = Quaternion.Euler(0,180,0);
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 20;

        quad = GenerateQuad();

        matrices = new Matrix4x4[instanceCount];

        // deterministic grid
        int index = 0;

        for (int y = -5; y < 5; y++)
        {
            for (int x = -5; x < 5; x++)
            {
                matrices[index++] =
                    Matrix4x4.TRS(
                        new Vector3(x * 2f, y * 2f, 0),
                        Quaternion.identity,
                        Vector3.one
                    );
            }
        }
    }

    void Update()
    {
        Graphics.DrawMeshInstanced(
            quad,
            0,
            material,
            matrices
        );
    }

    Mesh GenerateQuad()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = new Vector3[]
        {
            new Vector3(-0.5f,-0.5f,0),
            new Vector3( 0.5f,-0.5f,0),
            new Vector3( 0.5f, 0.5f,0),
            new Vector3(-0.5f, 0.5f,0)
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

        mesh.RecalculateBounds();

        return mesh;
    }
}

// --- End File: Unity\gpu-rendering\Assets\Runtime\GPUInstancingMinimal.cs ---

// --- Start File: Unity\gpu-rendering\Assets\00.BatchForEach1024Tile\instanced_rendering_summary.md ---


# CPU Instanced Grid Rendering – Summary

## Current Approach
The example renders a grid of quads using **Unity's `Graphics.DrawMeshInstanced`** with a list of `Matrix4x4` transforms that are recomputed every frame on the CPU.

### Pipeline
```
CPU
 ├ compute transforms (Matrix4x4 per instance)
 ├ upload matrices to GPU
 └ DrawMeshInstanced

GPU
 └ render instances
```

---

# Pros

### 1. Very simple architecture
Only requires:
- `Mesh`
- `Material`
- `List<Matrix4x4>`

### 2. Few draw calls
Unity batches up to **1023 instances per draw call**.

| Instances | Draw Calls |
|-----------|------------|  
| 1,024 | 2 |
| 10,000 | ~10 |
| 100,000 | ~98 |

Modern GPUs can easily handle **100–300 draw calls**, so this approach is viable for moderate instance counts.

### 3. Easy CPU control
All transforms are directly editable on CPU.

Example operations:
- wave animation
- rotations
- dynamic positioning

---

# Cons

### 1. CPU transform cost
Each frame computes:

```
gridSize² Matrix4x4
```

Example:

| Grid | Instances |
|-----|-----------|
32×32 | 1,024 |
128×128 | 16,384 |
256×256 | 65,536 |
512×512 | 262,144 |

CPU cost grows **O(N)** per frame.

---

### 2. Matrix upload bandwidth

Each matrix = **64 bytes**

Example bandwidth:

| Instances | Upload / frame |
|-----------|----------------|
10k | ~0.64 MB |
100k | ~6.4 MB |
200k | ~12.8 MB |

At **60 FPS**:

```
100k instances → ~384 MB/s
```

Not catastrophic, but unnecessary work.

---

### 3. 1023 instance limit per draw call

Unity splits automatically, but internally uses:

```
unity_ObjectToWorld[1023]
```

which forces batching.

---

# Performance Envelope

| Instances | Status |
|-----------|--------|
<10k | trivial |
10k–50k | still fine |
100k+ | CPU begins to dominate |
500k+ | unsuitable |



![performance.jpg](performance.jpg)

// --- End File: Unity\gpu-rendering\Assets\00.BatchForEach1024Tile\instanced_rendering_summary.md ---

// --- Start File: Unity\gpu-rendering\Assets\00.BatchForEach1024Tile\MinimalRenderTest.cs ---

using UnityEngine;
using System.Collections.Generic;

public class MinimalRenderTest : MonoBehaviour
{
    public Material material;
    public Camera mainCamera;
    public int gridSize = 32;

    
    public float zoomAmplitude = 10f;
    public float zoomBase = 20f;
    public float zoomSpeed = 1f;
        
    Mesh quad;

    List<Matrix4x4> matrices = new List<Matrix4x4>();

    void Start()
    {
        mainCamera.transform.position = new Vector3(0, 0, 30);
        mainCamera.transform.rotation = Quaternion.Euler(0,180,0);
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 20;

        quad = GenerateQuad();

        int count = gridSize * gridSize;

        // preallocate list
        for(int i=0;i<count;i++)
            matrices.Add(Matrix4x4.identity);
    }
    
    void OnGUI()
    {
        int tileCount = gridSize * gridSize;
        GUI.Box(new Rect(10, 10, 220, 120), "Renderer Stats");

        GUILayout.BeginArea(new Rect(20, 40, 200, 100));

        GUILayout.Label($"Tiles Rendered: {tileCount}");
        GUILayout.Label($"Grid Size: {gridSize} x {gridSize}");
        GUILayout.Label($"Instances: {tileCount}");

        int drawCalls = Mathf.CeilToInt(tileCount / 1023f);
        GUILayout.Label($"Draw Calls (approx): {drawCalls}");

        GUILayout.Label($"Zoom: {mainCamera.orthographicSize:F2}");

        GUILayout.EndArea();
    }

    void Update()
    {
        float t = Time.time;

        // animated zoom
        float zoom = zoomBase + Mathf.Sin(t * zoomSpeed) * zoomAmplitude;
        mainCamera.orthographicSize = zoom;

        int index = 0;

        for(int y = 0; y < gridSize; y++)
        {
            for(int x = 0; x < gridSize; x++)
            {
                float wave = Mathf.Sin(t + x * 0.3f + y * 0.3f);

                Vector3 pos =
                    new Vector3(
                        (x - gridSize * 0.5f) * 1.5f,
                        (y - gridSize * 0.5f) * 1.5f,
                        0);

                Quaternion rot =
                    Quaternion.Euler(
                        0,
                        0,
                        wave * 30f
                    );

                matrices[index] =
                    Matrix4x4.TRS(pos, rot, Vector3.one);

                index++;
            }
        }

        Graphics.DrawMeshInstanced(
            quad,
            0,
            material,
            matrices
        );
    }

    Mesh GenerateQuad()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = new Vector3[]
        {
            new Vector3(-0.5f,-0.5f,0),
            new Vector3( 0.5f,-0.5f,0),
            new Vector3( 0.5f, 0.5f,0),
            new Vector3(-0.5f, 0.5f,0)
        };

        mesh.triangles = new int[]
        {
            0,1,2,
            0,2,3
        };

        return mesh;
    }
}

// --- End File: Unity\gpu-rendering\Assets\00.BatchForEach1024Tile\MinimalRenderTest.cs ---


--------------------------------------------------
// --- Prompt ---

- Reviewing source files for a GPU instance based rendering tutorial.
- Your task is to create a well-structured and user-friendly `README.md` that includes the following sections:

## Introduction
- Provide a brief overview of the tool from the user's perspective.
- Explain its purpose and the problem it solves.

## Features
- List the key capabilities of the tool as bullet points.

## Summary
- Conclude with a concise summary highlighting the module’s value and primary use case.

