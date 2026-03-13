# 02.InstanceColor

This step introduces **per‑instance data** in GPU instancing.

In the previous example (`01.BatchForEach1024Tile`) every tile shared
the same material and appearance.\
Here we extend the renderer so **each instance can have its own color**
while still being rendered in batches.

The goal is to demonstrate how **custom instance properties** are passed
from the CPU to the GPU.

------------------------------------------------------------------------

# Concept

GPU instancing allows many objects to be rendered using the same mesh
and material.

Each instance already receives its own transform matrix:

    Matrix4x4 → object position / rotation / scale

In this step we add a second per‑instance property:

    Color → visual variation per tile

So the instance data becomes:

    Instance
    ├─ Transform Matrix
    └─ Color

------------------------------------------------------------------------

# Rendering Pipeline

    CPU
      generate transforms
      generate random colors
      store instance data
          ↓
    MaterialPropertyBlock
          ↓
    Graphics.DrawMeshInstanced
          ↓
    GPU
      vertex shader reads instance color
      color interpolated to fragment shader
      fragment shader outputs color

------------------------------------------------------------------------

# Key Techniques

## MaterialPropertyBlock

Unity uses `MaterialPropertyBlock` to send per‑instance arrays to the
GPU.

Example:

``` csharp
_propertyBlock.SetVectorArray("_InstanceColor", _colors);
```

This allows each instance to access its own color inside the shader.

------------------------------------------------------------------------

## Instancing Shader Macros

The shader declares per‑instance properties using Unity instancing
macros:

``` hlsl
UNITY_INSTANCING_BUFFER_START(Props)
    UNITY_DEFINE_INSTANCED_PROP(float4, _InstanceColor)
UNITY_INSTANCING_BUFFER_END(Props)
```

Each instance can then access its value:

``` hlsl
UNITY_ACCESS_INSTANCED_PROP(Props, _InstanceColor)
```

------------------------------------------------------------------------

# Vertex Shader Flow

    instanceID
        ↓
    read instance color
        ↓
    apply object transform
        ↓
    pass color to fragment stage

Because the fragment shader only receives interpolated color, we **do
not need** `UNITY_TRANSFER_INSTANCE_ID`.

------------------------------------------------------------------------

# Result

The rendered grid now shows:

    • same mesh
    • same material
    • different color per instance
    • still rendered with GPU instancing

This demonstrates how **instance metadata** can control appearance
without increasing draw calls.

------------------------------------------------------------------------

# What This Step Teaches

    • passing per‑instance data to shaders
    • using MaterialPropertyBlock
    • Unity instancing macros
    • vertex → fragment interpolation

These concepts are essential for the next step, where instance data will
be used to select textures from a **texture array**.

------------------------------------------------------------------------

# Next Step

    03.TextureArrayTiles

Instead of colors, each instance will contain:

    tileIndex

which selects a texture from a **Texture2DArray**, enabling a fully
GPU‑instanced tile renderer.
