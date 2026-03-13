# 03.GPUAnimationAndProcedural --- 02.GPUProceduralTiles

This step demonstrates **procedural instance placement entirely on the
GPU**.

In previous steps the CPU generated instance transforms and uploaded
them to the GPU. Here we remove the instance buffer completely and
derive the tile transform directly from the GPU **instance ID
(`SV_InstanceID`)**.

This technique is widely used in real‑time rendering systems for:

-   terrain tiles
-   vegetation placement
-   particle systems
-   crowds
-   procedural worlds

------------------------------------------------------------------------

# Core Idea

Every instance rendered by the GPU has a unique **instance ID**.

instanceID = 0,1,2,3,4,...

From this value we can compute a **grid coordinate**:

x = instanceID % GridSize\
y = instanceID / GridSize

These coordinates allow the shader to procedurally compute:

-   world position
-   rotation
-   tile type
-   animation

No CPU instance data is required.

------------------------------------------------------------------------

# Rendering Pipeline

CPU

set shader parameters\
DrawMeshInstancedProcedural(instanceCount)

GPU

vertex shader derive grid position from instanceID compute tile
transform compute tile variation

fragment shader sample tile texture

The GPU becomes responsible for generating instance transforms.

------------------------------------------------------------------------

# Procedural Effects Demonstrated

This tutorial step uses **structured procedural patterns** instead of
random noise.

### Radial Rotation

Tiles rotate to face outward from the center of the grid.

direction = normalize(grid - center)\
rotation = atan2(direction.y, direction.x)

This produces a visually organized radial field.

------------------------------------------------------------------------

### Distance Rings

Tile textures are selected based on distance from the center.

tileIndex = floor(distance) % TileCount

This produces circular rings of different tiles.

------------------------------------------------------------------------

### Ripple Animation

A wave propagates through the grid using a sine function.

height = sin(distance \* frequency - time)

This creates animated ripple rings across the tile grid.

------------------------------------------------------------------------

# Why This Matters

Procedural instance generation allows the GPU to create large numbers of
objects without storing transforms for every instance.

Benefits:

-   no CPU transform generation
-   no instance buffers
-   minimal memory bandwidth
-   highly scalable

Many modern rendering systems rely on procedural GPU placement for large
scenes.

------------------------------------------------------------------------

# What This Step Teaches

This example introduces several key techniques:

-   using SV_InstanceID for procedural placement
-   deriving transforms in the vertex shader
-   procedural material selection
-   procedural animation

These concepts prepare for the next stage where **compute shaders
generate instance data**.

------------------------------------------------------------------------

# Next Stage

04.ComputeDrivenRendering 01.ComputeGeneratedTiles

In that stage compute shaders will:

-   generate instance buffers
-   update indirect draw arguments
-   perform GPU culling
-   render massive instance counts
