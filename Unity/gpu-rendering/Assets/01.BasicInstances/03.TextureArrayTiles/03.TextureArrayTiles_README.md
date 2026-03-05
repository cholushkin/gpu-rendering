# 03.TextureArrayTiles

This step extends GPU instancing by allowing **each instance to choose
its own texture**.

In the previous step (`02.InstanceColor`) every tile shared the same
material but could vary in color. Now we replace color variation with
**texture variation**, allowing each instance to render a different tile
from a shared texture set.

The technique uses a **Texture2DArray** combined with **per‑instance
metadata**.

------------------------------------------------------------------------

# Concept

A **Texture2DArray** stores multiple textures inside a single GPU
resource.

Instead of binding many textures to different materials, we store all
tile textures in one array:

    Texture2DArray
    ├─ layer 0 → tile texture
    ├─ layer 1 → tile texture
    ├─ layer 2 → tile texture
    └─ ...

Each instance contains a **tile index** that selects which layer of the
texture array to sample.

    Instance
    ├─ Transform Matrix
    └─ TileIndex → selects texture layer

------------------------------------------------------------------------

# Rendering Pipeline

    CPU
      generate transforms
      generate random tile indices
      build Texture2DArray
          ↓
    MaterialPropertyBlock
          ↓
    Graphics.DrawMeshInstanced
          ↓
    GPU
      vertex shader reads tile index
      fragment shader samples Texture2DArray layer
      tile texture rendered

------------------------------------------------------------------------

# Key Techniques

## Texture2DArray

Instead of switching textures per draw call, all tile textures are
packed into a single array:

``` csharp
Texture2DArray tileArray =
    new Texture2DArray(width, height, layers, format, false);
```

Each texture is copied into one layer of the array.

This allows the GPU to sample different textures **without breaking
batching**.

------------------------------------------------------------------------

## Per‑Instance Tile Index

Each instance receives a tile index:

    0 → first tile texture
    1 → second tile texture
    2 → third tile texture
    ...

The CPU sends this array to the shader:

``` csharp
_propertyBlock.SetFloatArray("_TileIndex", _tileIndices);
```

------------------------------------------------------------------------

## URP Texture Sampling

URP shaders use special macros for texture arrays:

``` hlsl
TEXTURE2D_ARRAY(_Tiles);
SAMPLER(sampler_Tiles);
```

Sampling a tile texture:

``` hlsl
SAMPLE_TEXTURE2D_ARRAY(_Tiles, sampler_Tiles, uv, tileIndex);
```

These macros ensure correct compilation across graphics APIs.

------------------------------------------------------------------------

# Shader Flow

    Vertex Shader
        read instance tile index
            ↓
        pass UV and tileIndex
            ↓
    Fragment Shader
        sample texture array layer
            ↓
        output tile color

------------------------------------------------------------------------

# Result

The grid now displays:

    • same mesh
    • same material
    • different texture per instance
    • still rendered using GPU instancing

This is a fundamental technique used by:

    tilemap renderers
    voxel engines
    procedural terrain
    GPU particle systems

------------------------------------------------------------------------

# What This Step Teaches

    • Texture2DArray creation
    • GPU texture arrays
    • per‑instance metadata
    • sampling array textures in shaders
    • maintaining batching with many textures

These concepts are important for the next stage, where instance data
will be moved entirely to **GPU buffers**.

------------------------------------------------------------------------

# Next Step

    04.StructuredInstanceBuffer

In that step we will remove `MaterialPropertyBlock` and Unity instancing
macros, and send instance data directly to the GPU using **structured
buffers**.
