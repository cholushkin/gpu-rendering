# 01.ComputeGeneratedTiles

This step introduces **compute-driven instance generation**.

Instead of generating tile transforms on the CPU, a **compute shader
builds instance data directly on the GPU** and writes it into a
structured buffer. The renderer then draws all tiles using
`DrawMeshInstancedProcedural`.

This is the first step toward **fully GPU-driven rendering**.

------------------------------------------------------------------------

# Concept

In earlier steps:

CPU - generates instance transforms - uploads arrays every frame -
submits draw calls

In this step:

Compute Shader (GPU) - generates instance data - writes data into a
structured buffer

Renderer - reads instance data directly from the GPU buffer - renders
all tiles procedurally

Pipeline:

    Compute Shader
        ↓
    StructuredBuffer<InstanceData>
        ↓
    Vertex Shader (SV_InstanceID lookup)
        ↓
    Procedural Instanced Draw

------------------------------------------------------------------------

# Instance Data Design

To avoid matrix layout issues and reduce memory usage, each instance
stores minimal transform information instead of a full matrix.

Instance layout:

    float2 position
    float rotation
    float scale
    uint tileIndex

This is only **20 bytes per instance**, compared to **80 bytes** when
storing a full transform matrix.

Benefits:

-   smaller GPU buffers
-   simpler compute shader
-   easier debugging
-   better cache usage

------------------------------------------------------------------------

# Compute Shader Responsibilities

The compute shader generates tile positions in a grid and writes them
into the instance buffer.

Example logic:

    uint index = y * GridSize + x;

    float px = (x - GridSize * 0.5) * TileSpacing;
    float py = (y - GridSize * 0.5) * TileSpacing;

    InstanceData data;

    data.position = float2(px,py);
    data.rotation = 0;
    data.scale = TileScale;
    data.tileIndex = (x + y) % TileCount;

    Result[index] = data;

Each thread corresponds to **one tile instance**.

------------------------------------------------------------------------

# Vertex Shader Responsibilities

The vertex shader reconstructs the tile transform.

Steps:

1.  read instance data using `SV_InstanceID`
2.  scale mesh vertices
3.  rotate vertices
4.  translate to world position

```{=html}
<!-- -->
```
    pos *= data.scale;
    pos = Rotate(pos,data.rotation);
    pos += data.position;

This produces the final tile position.

------------------------------------------------------------------------

# Rendering Method

Rendering uses:

    Graphics.DrawMeshInstancedProcedural

This allows the GPU to render **arbitrary instance counts** without the
1023-instance limit of `DrawMeshInstanced`.

------------------------------------------------------------------------

# Advantages of Compute-Generated Instances

Compared to CPU instance generation:

-   eliminates CPU transform generation
-   avoids large CPU → GPU data uploads
-   scales to millions of instances
-   prepares the pipeline for GPU culling and LOD

This is the **foundation of GPU-driven rendering pipelines** used in
modern game engines.

------------------------------------------------------------------------

# What This Step Teaches

-   compute shader instance generation
-   GPU structured buffers
-   procedural instanced rendering
-   minimal instance data design
-   vertex shader transform reconstruction

------------------------------------------------------------------------

# Next Step

`02.GPUCulling`

The next step introduces **GPU frustum culling**.

Compute shaders determine which instances are visible and update
**indirect draw arguments**, allowing the renderer to skip invisible
tiles without CPU involvement.
