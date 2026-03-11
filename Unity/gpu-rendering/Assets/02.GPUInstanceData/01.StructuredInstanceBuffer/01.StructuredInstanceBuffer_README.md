# 01.StructuredInstanceBuffer

This step transitions the renderer from **CPU-managed instance arrays**
to **GPU-resident instance data** using a **StructuredBuffer**.

In the previous examples, instance data such as transforms or tile
indices were uploaded every frame using `MaterialPropertyBlock` arrays.

While that approach works well for moderate instance counts, it has
several limitations:

-   Unity internally splits instances into **batches of 1023**
-   large instance arrays must be **uploaded every frame**
-   the CPU remains responsible for managing instance data

This step moves instance data into a **GPU buffer**, allowing the shader
to access instance information directly.

------------------------------------------------------------------------

# Concept

Instead of passing instance data through Unity's instancing system, we
store all instance information in a **StructuredBuffer**.

    GPU Memory

    StructuredBuffer<InstanceData>
     ├ Instance 0
     ├ Instance 1
     ├ Instance 2
     ├ ...
     └ Instance N

Each instance contains:

    InstanceData
     ├ transform matrix
     └ tileIndex

The GPU then selects the correct instance data using **SV_InstanceID**.

------------------------------------------------------------------------

# Rendering Pipeline

Previous steps used this pipeline:

    CPU
      generate transforms
      generate instance metadata
      upload arrays via MaterialPropertyBlock
      DrawMeshInstanced

    GPU
      instance macros fetch metadata
      render instances

This step changes the pipeline to:

    CPU
      generate instance data
      upload once to GPU buffer
      DrawMeshInstancedProcedural

    GPU
      instanceID selects instance data
      vertex shader reads transform + metadata
      fragment shader renders tile

The shader now **pulls instance data directly from GPU memory**.

------------------------------------------------------------------------

# StructuredBuffer

A `StructuredBuffer` is a GPU buffer containing an array of custom
structs.

Shader declaration:

``` hlsl
StructuredBuffer<InstanceData> _InstanceData;
```

Example struct:

``` hlsl
struct InstanceData
{
    float4x4 transform;
    float tileIndex;
};
```

Each element of the buffer represents **one rendered instance**.

------------------------------------------------------------------------

# Instance Lookup

Instead of Unity instancing macros, the shader now uses
**SV_InstanceID**.

    instanceID
        ↓
    _InstanceData[instanceID]
        ↓
    instance transform + metadata

Vertex shader example:

``` hlsl
Varyings vert(Attributes input, uint instanceID : SV_InstanceID)
{
    InstanceData data = _InstanceData[instanceID];

    float4 world =
        mul(data.transform, float4(input.positionOS,1));
}
```

This allows the shader to access **any instance directly from the
buffer**.

------------------------------------------------------------------------

# DrawMeshInstancedProcedural

This example introduces a new rendering API:

    Graphics.DrawMeshInstancedProcedural

Unlike `DrawMeshInstanced`, this call:

-   does **not use Unity instancing arrays**
-   has **no 1023 instance limit**
-   reads instance data directly from GPU buffers

Example:

``` csharp
Graphics.DrawMeshInstancedProcedural(
    mesh,
    0,
    material,
    bounds,
    instanceCount
);
```

The `instanceCount` specifies how many instances the GPU should render.

------------------------------------------------------------------------

# Bounds Requirement

Procedural draws require an explicit **bounding box**.

    Bounds bounds

Unity uses this bounding box for **frustum culling**.

If the bounds intersect the camera frustum, the entire draw call is
executed.

Important:

    Culling happens per draw call,
    not per instance.

If a single instance is visible, **all instances in the buffer are
rendered**.

More advanced techniques such as **GPU culling** will address this
later.

------------------------------------------------------------------------

# Instance Data Layout

Each instance stores:

    Matrix4x4 transform
    float tileIndex

Total floats:

    16 + 1 = 17 floats

Each float is **4 bytes**, so the total size per instance is:

    17 × 4 = 68 bytes

The GPU buffer stride must match this size exactly:

``` csharp
int stride = sizeof(float) * (16 + 1);
```

Correct buffer layout is critical --- the C# struct and shader struct
**must match exactly**.

------------------------------------------------------------------------

# What This Step Teaches

This example introduces several key concepts used in modern rendering
systems:

-   GPU structured buffers
-   instance lookup using `SV_InstanceID`
-   procedural instanced rendering
-   removing Unity's 1023 instance batching limit
-   storing instance data directly on the GPU

These ideas are foundational for **GPU-driven rendering architectures**.

------------------------------------------------------------------------

# Limitations of This Approach

Although instance data is now stored on the GPU, the CPU still controls:

-   instance count
-   draw submission

Additionally, **all instances are rendered whenever the bounds intersect
the camera frustum**.

This means the GPU may still process many invisible instances.

------------------------------------------------------------------------

# Next Step

    02.DrawMeshInstancedIndirect

The next step removes the remaining CPU control by introducing an
**Indirect Draw Buffer**.

This allows the GPU to determine:

-   how many instances should be rendered
-   which instances are visible

Indirect drawing is the first step toward **fully GPU-driven
rendering**.
