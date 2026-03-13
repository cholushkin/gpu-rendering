# 02.DrawMeshInstancedIndirect

This step introduces **indirect GPU instanced rendering**.

Instead of passing the instance count directly from the CPU to the draw
call, the renderer now reads draw parameters from a **GPU argument
buffer**.

This technique is widely used in modern engines and is a critical step
toward **fully GPU‑driven rendering pipelines**.

------------------------------------------------------------------------

# Concept

In previous steps the CPU controlled the number of instances rendered.

Example:

    Graphics.RenderMeshPrimitives(mesh, material, instanceCount)

The CPU explicitly tells the GPU:

    "Render N instances"

With **DrawMeshInstancedIndirect**, this information is stored inside a
**GPU buffer** instead.

    Indirect Arguments Buffer

        indexCountPerInstance
        instanceCount
        startIndex
        baseVertex
        startInstance

The GPU reads these parameters when executing the draw call.

------------------------------------------------------------------------

# Indirect Argument Buffer Layout

Unity expects the argument buffer to contain **five integers**:

    uint[5] args =
    {
        indexCountPerInstance,
        instanceCount,
        startIndex,
        baseVertex,
        startInstance
    };

Example for rendering quads:

    args =
    {
        6,        // quad index count
        16384,    // instance count
        0,
        0,
        0
    }

The argument buffer is uploaded to the GPU and later read by the draw
call.

------------------------------------------------------------------------

# Rendering Pipeline

CPU

    create instance buffer
    create indirect argument buffer
    upload both to GPU
    issue DrawMeshInstancedIndirect

GPU

    read argument buffer
    determine instance count
    fetch instance data
    render instances

------------------------------------------------------------------------

# Key Differences From Previous Step

Previous step:

    CPU decides instance count
    → passed directly to draw call

This step:

    instance count stored in GPU memory
    → draw call reads arguments from buffer

This makes the draw call **data‑driven instead of parameter‑driven**.

------------------------------------------------------------------------

# Why Indirect Rendering Matters

Indirect rendering enables the GPU to control rendering behavior.

Future steps will allow **compute shaders** to modify the argument
buffer, making it possible to implement:

-   GPU frustum culling
-   GPU occlusion culling
-   GPU procedural generation
-   dynamic instance spawning
-   massive instance counts

This technique is used in modern rendering systems such as:

-   Unreal Engine GPU scene
-   Unity DOTS renderer
-   Frostbite GPU pipelines

------------------------------------------------------------------------

# Current Limitation

In this example the **CPU still writes the argument buffer**.

So while the draw call reads parameters from GPU memory, the CPU still
decides:

    instanceCount

The next tutorial stages will remove this limitation by letting the GPU
update instance buffers and draw arguments using **compute shaders**.

------------------------------------------------------------------------

# What This Step Teaches

This example introduces several key concepts used in GPU‑driven
rendering:

-   indirect draw calls
-   GPU argument buffers
-   DrawMeshInstancedIndirect API
-   data‑driven rendering pipelines

Understanding this step is essential before moving to compute‑driven
rendering.

------------------------------------------------------------------------

# Next Stage

The next tutorial stage moves instance generation to the GPU.

    04.ComputeDrivenRendering

Where compute shaders will:

    generate instances
    update argument buffers
    perform GPU culling

At that point the renderer becomes **fully GPU‑driven**.
