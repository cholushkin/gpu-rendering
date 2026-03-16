# 03.MassiveTileWorlds

This tutorial step demonstrates how to render a **seemingly infinite
tile world** using a **camera‑centered GPU tile window**.

Instead of generating and rendering an entire world, the renderer
maintains a **fixed grid of tiles around the camera**.\
As the camera moves, the compute shader regenerates tile positions so
the grid always surrounds the viewer.

This creates the illusion of an **endless procedural terrain** while
rendering only a small number of tiles.

------------------------------------------------------------------------

# Concept

Traditional tilemaps store the entire world in memory.

Example:

    World size = 10,000 × 10,000 tiles

Rendering this directly would require huge buffers and expensive
updates.

Instead, this system renders only a **view window**:

    ViewWidth × ViewHeight

Example:

    128 × 64 = 8192 tiles

The compute shader regenerates tiles relative to the **camera tile
coordinate**, so the world appears infinite.

Pipeline:

    Camera Movement
            ↓
    Compute Shader Regenerates Tiles Around Camera
            ↓
    Instance Buffer Updated
            ↓
    DrawMeshInstancedIndirect

------------------------------------------------------------------------

# Camera‑Centered Tile Window

Each frame the compute shader calculates which **world tiles**
correspond to the view window.

    cameraTile = floor(cameraPosition / tileSpacing)

Each thread computes:

    tileX = cameraTileX + (localX - ViewWidth/2)
    tileZ = cameraTileZ + (localY - ViewHeight/2)

This shifts the grid as the camera moves.

Visually:

    Before movement

       [tile grid]
          C

    After movement

          C
       [tile grid]

The grid follows the camera.

------------------------------------------------------------------------

# Instance Data Layout

Each tile instance stores only minimal data:

    struct InstanceData
    {
        float2 position
        float rotation
        float scale
        float noise
    }

Total memory:

    5 floats = 20 bytes per tile

Example memory cost:

  Tiles   Memory
  ------- -----------
  8k      \~160 KB
  16k     \~320 KB
  64k     \~1.25 MB

This keeps GPU buffers extremely small.

------------------------------------------------------------------------

# Noise‑Based Terrain

The compute shader generates **procedural terrain variation** using a
simple noise function.

    noise(worldPosition * NoiseScale)

The noise value is used for two things:

### 1. Terrain Height

    height = noise * HeightScale

This displaces tiles vertically, creating rolling terrain.

### 2. Color Tint

The fragment shader uses noise to tint tiles:

    tint = lerp(0.6, 1.4, noise)

This produces natural variation across the landscape.

------------------------------------------------------------------------

# Rendering Method

Rendering uses:

    Graphics.DrawMeshInstancedIndirect

The instance count is stored in a **GPU argument buffer**, allowing the
draw call to be completely **data‑driven**.

Indirect arguments layout:

    uint[5] args =
    {
        indexCountPerInstance,
        instanceCount,
        startIndex,
        baseVertex,
        startInstance
    }

This avoids CPU draw‑call management.

------------------------------------------------------------------------

# Camera‑Relative Rendering

The shader subtracts the camera position from world coordinates:

    world -= cameraOffset

This technique improves numerical precision for very large worlds and
keeps coordinates close to the origin.

Many large‑world engines use this strategy.

------------------------------------------------------------------------

# Why This Technique Scales

This approach allows rendering extremely large environments because:

• world size is **not stored anywhere**\
• tile data is **generated procedurally**\
• memory usage depends only on **view size**\
• rendering uses **one indirect draw call**

The renderer cost becomes:

    O(ViewWidth × ViewHeight)

Instead of:

    O(WorldSize²)

------------------------------------------------------------------------

# Example Performance

Example configuration:

    ViewWidth  = 128
    ViewHeight = 64
    Tiles      = 8192

This entire terrain renders with:

    1 compute dispatch
    1 indirect draw call

Even modest GPUs can handle much larger view windows.

------------------------------------------------------------------------

# What This Step Teaches

This tutorial introduces several key concepts used in large‑scale
rendering:

• camera‑centered procedural worlds\
• compute‑generated instance buffers\
• GPU‑driven indirect rendering\
• noise‑based terrain generation\
• camera‑relative rendering

These techniques appear in many modern rendering systems including:

• voxel engines\
• open‑world terrain systems\
• procedural world generators\
• GPU‑driven scene renderers

------------------------------------------------------------------------

# Next Steps

Possible extensions include:

### GPU Frustum Culling

Cull tiles outside the camera view before rendering.

### Chunk‑Based Streaming

Group tiles into chunks (for example **16×16 tiles**) and update only
visible chunks.

### Biome Systems

Use multiple noise layers to generate different terrain types.

### Lighting & Normals

Compute normals from the noise gradient for realistic terrain shading.

------------------------------------------------------------------------

# Summary

This step demonstrates how to build a **scalable infinite world
renderer** using:

    Compute Shader + GPU Instancing + Procedural Generation

By keeping the tile window fixed around the camera, the renderer
achieves the illusion of an **endless terrain** while maintaining very
small memory and rendering costs.
