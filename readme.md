# GPU Instancing Tutorial Series

This repository contains a **step‑by‑step tutorial series for GPU
instancing in Unity**.

The goal is to **gradually increase complexity** while building a deeper
understanding of modern GPU‑driven rendering techniques.

Each phase introduces **one major concept at a time**, starting from
simple CPU‑driven instancing and progressing toward fully GPU‑driven
rendering pipelines using compute shaders and indirect drawing.

The structure is intentionally organized so that each lesson is **small,
isolated, and easy to understand**, while still building toward a
scalable rendering architecture.

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

# Tutorial Stages

### 01.BasicInstances

Introduction to GPU instancing using Unity's built‑in APIs. Examples
show CPU‑generated transforms and basic instance variation.

### 02.GPUInstanceData

Moves instance data into GPU buffers and introduces indirect drawing to
remove Unity's instancing limits.

### 03.GPUAnimationAndProcedural

Demonstrates how instance behavior can be driven entirely on the GPU
using shader logic and deterministic procedural generation.

### 04.ComputeDrivenRendering

Introduces compute shaders to generate and filter instance data,
enabling large‑scale rendering with GPU culling.

### 05.AdvancedRendering

Explores more advanced rendering techniques such as SDF tiles and a
fully GPU‑driven tilemap architecture.
