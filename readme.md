# GPU Instancing Tutorial Series

This repository contains a **step‑by‑step tutorial series for GPU
instancing in Unity**.

The goal is to **gradually increase complexity** while building a deeper
understanding of modern GPU‑driven rendering techniques.

Each lesson introduces **one focused concept** and builds toward a
scalable, GPU‑driven rendering architecture. The lessons are
intentionally small and isolated so that every step clearly demonstrates
a single idea.

The tutorial gradually evolves from:

CPU‑driven instancing → GPU instance buffers → procedural GPU logic →
compute‑driven rendering.

------------------------------------------------------------------------

# Directory Structure

    01.BasicInstances
    │
    ├─ 01.BatchForEach1024Tile
    ├─ 02.InstanceColor
    ├─ 03.TextureArrayTiles
    └─ 04.TextureAtlasTiles


    02.GPUInstanceData
    │
    ├─ 01.StructuredInstanceBuffer
    └─ 02.DrawMeshInstancedIndirect


    03.GPUAnimationAndProcedural
    │
    ├─ 01.GPUAnimatedTiles
    └─ 02.GPUProceduralTiles


    04.ComputeDrivenRendering
    │
    ├─ 01.ComputeGeneratedTiles
    ├─ 02.GPUCulling
    └─ 03.MassiveTileWorlds


    05.AdvancedRendering
    │
    ├─ 01.SDFTiles
    └─ 02.GPUDrivenTilemap

------------------------------------------------------------------------

# Tutorial Lessons

Each lesson lives in its own folder and demonstrates a **single
rendering concept**.

------------------------------------------------------------------------

### Basic Instancing

•
**[01.BatchForEach1024Tile](Unity/gpu-rendering/Assets/01.BasicInstances/01.BatchForEach1024Tile)**\
Baseline example showing CPU‑driven instancing using
`Graphics.DrawMeshInstanced`.\
Transforms are generated every frame on the CPU and submitted to the GPU
in batches.

• **[02.InstanceColor](Unity/gpu-rendering/Assets/01.BasicInstances/02.InstanceColor)**\
Introduces **per‑instance data**. Each tile receives its own color using
`MaterialPropertyBlock` and instancing shader properties.

• **[03.TextureArrayTiles](Unity/gpu-rendering/Assets/01.BasicInstances/03.TextureArrayTiles)**\
Replaces color variation with **texture variation** using
`Texture2DArray`.\
Each instance selects a tile texture using a per‑instance tile index.

• **[04.TextureAtlasTiles](Unity/gpu-rendering/Assets/01.BasicInstances/04.TextureAtlasTiles)**\
Uses a **texture atlas** instead of a texture array. Instances remap
their UVs to sample different atlas regions.

------------------------------------------------------------------------

### GPU Instance Data

•
**[01.StructuredInstanceBuffer](Unity/gpu-rendering/Assets/02.GPUInstanceData/01.StructuredInstanceBuffer)**\
Moves instance data from CPU arrays to a **GPU StructuredBuffer**.\
The shader reads instance transforms using `SV_InstanceID`, removing
Unity's 1023‑instance batching limit.

•
**[02.DrawMeshInstancedIndirect](Unity/gpu-rendering/Assets/02.GPUInstanceData/02.DrawMeshInstancedIndirect)**\
Introduces **indirect draw calls**. Rendering parameters are stored in a
GPU argument buffer, enabling data‑driven draw calls and preparing for
GPU‑driven rendering.

------------------------------------------------------------------------

### GPU Animation & Procedural Generation

•
**[01.GPUAnimatedTiles](Unity/gpu-rendering/Assets/03.GPUAnimationAndProcedural/01.GPUAnimatedTiles)**\
Moves tile animation from CPU transforms to the **vertex shader**,
allowing thousands of instances to animate without CPU updates.

•
**[02.GPUProceduralTiles](Unity/gpu-rendering/Assets/03.GPUAnimationAndProcedural/02.GPUProceduralTiles)**\
Demonstrates **procedural placement using `SV_InstanceID`**.\
Tile transforms, rotation, and animation are derived directly from the
instance ID without using an instance buffer.

------------------------------------------------------------------------

### Compute‑Driven Rendering *(upcoming)*

•
**[01.ComputeGeneratedTiles](Unity/gpu-rendering/Assets/04.ComputeDrivenRendering/01.ComputeGeneratedTiles)**\
Compute shaders generate instance transforms directly on the GPU and
populate instance buffers for rendering.

• **[02.GPUCulling](Unity/gpu-rendering/Assets/04.ComputeDrivenRendering/02.GPUCulling)**\
Adds GPU frustum culling where compute shaders determine which instances
are visible and update indirect draw arguments.

•
**[03.MassiveTileWorlds](Unity/gpu-rendering/Assets/04.ComputeDrivenRendering/03.MassiveTileWorlds)**\
Demonstrates scalable rendering of very large worlds using GPU instance
generation, culling, and indirect drawing.

------------------------------------------------------------------------

### Advanced Rendering *(planned)*

• **[01.SDFTiles](Unity/gpu-rendering/Assets/05.AdvancedRendering/01.SDFTiles)**\
Introduces **Signed Distance Field (SDF) tiles** for procedural
vector‑style tile rendering.

• **[02.GPUDrivenTilemap](Unity/gpu-rendering/Assets/05.AdvancedRendering/02.GPUDrivenTilemap)**\
Combines compute generation, GPU culling, and SDF tiles to build a
**fully GPU‑driven tilemap renderer**.

------------------------------------------------------------------------

# Learning Path

By completing the tutorial in order, readers will progressively learn:

• CPU instancing fundamentals\
• GPU instance buffers\
• indirect drawing\
• procedural GPU transforms\
• compute‑generated instances\
• GPU culling and large‑scale rendering

The final stages combine these ideas into a **fully GPU‑driven rendering
architecture**.
