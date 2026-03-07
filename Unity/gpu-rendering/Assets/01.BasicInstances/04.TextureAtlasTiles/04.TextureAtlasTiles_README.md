# 04.TextureAtlasTiles

This step introduces GPU instancing using a **texture atlas**.

Instead of storing tiles inside a `Texture2DArray`, multiple tile
textures are packed into a **single large texture (atlas)**.\
Each instance selects its tile by **remapping its UV coordinates** to a
region of the atlas.

This allows thousands of tiles to be rendered with:

-   **one mesh**
-   **one material**
-   **one texture**
-   **a small amount of per‑instance metadata**

All tiles can still be rendered in large **instanced batches**.

------------------------------------------------------------------------

# Concept

Texture Atlas

    +----+----+----+
    | T0 | T1 | T2 |
    +----+----+----+
    | T3 | T4 | T5 |
    +----+----+----+

Each tile occupies a rectangular region inside the atlas.

Every instance stores a UV transform describing that region.

    Instance
     ├ Transform Matrix
     └ AtlasUV (scaleX, scaleY, offsetX, offsetY)

Where:

    scale  = size of the tile inside the atlas
    offset = position of the tile inside the atlas

------------------------------------------------------------------------

# Rendering Pipeline

CPU

    generate transforms
    generate atlasUV per instance
    upload arrays via MaterialPropertyBlock
    submit draw call with Graphics.DrawMeshInstanced

GPU

    vertex shader
        read atlasUV for this instance
        remap mesh UV coordinates

    fragment shader
        sample atlas texture
        output tile color

------------------------------------------------------------------------

# Shader Architecture

The shader uses **URP instancing macros** to read per‑instance data.

### 1. Instance property declaration

    UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_DEFINE_INSTANCED_PROP(float4, _AtlasUV)
    UNITY_INSTANCING_BUFFER_END(Props)

Each instance receives a **UV transform**.

### 2. Vertex shader

The vertex shader remaps the UV coordinates:

    atlas = UNITY_ACCESS_INSTANCED_PROP(Props,_AtlasUV)

    uv = input.uv * atlas.xy + atlas.zw

Where:

    atlas.xy = scale
    atlas.zw = offset

### 3. Fragment shader

The fragment shader samples the atlas:

    color = SAMPLE_TEXTURE2D(_Atlas, sampler_Atlas, uv)

The GPU therefore renders **different tiles from the same texture**
without breaking batching.

------------------------------------------------------------------------

# Why Atlas‑Style Rendering Is Useful

Texture atlases are widely used in real‑time rendering because they
allow many visual variations while still keeping rendering extremely
efficient.

Benefits:

-   **Single texture binding** --- avoids expensive texture switches.
-   **Perfect batching** --- all instances share one material.
-   **Artist‑friendly workflow** --- artists can pack tiles or sprites
    into a single image.
-   **Memory efficient** --- fewer GPU resources are required.
-   **Flexible layouts** --- tiles do not have to be identical sizes.

------------------------------------------------------------------------

# Example Uses In Games

Texture atlas rendering appears in many real‑world systems.

Examples:

-   **Tilemap worlds** --- thousands of terrain tiles (grass, rock,
    sand) sampled from one atlas.
-   **Voxel engines** --- block faces referencing different atlas
    regions (stone, dirt, ore).
-   **2D sprite crowds** --- hundreds of characters sharing one sprite
    sheet.
-   **Particle systems** --- animated particles using atlas frames.
-   **UI icon libraries** --- many small icons packed into a single
    texture.
-   **Procedural worlds** --- GPU‑generated tiles selecting atlas
    regions dynamically.

Even very large environments can often be rendered with **only one or
two textures**.

------------------------------------------------------------------------

# What This Step Teaches

-   atlas‑based texture sampling
-   per‑instance UV transforms
-   GPU instancing with atlas textures
-   how shaders remap UV coordinates per instance

These techniques are widely used in **tile engines, voxel renderers, and
large sprite systems**.

------------------------------------------------------------------------

# Next Step

    02.GPUInstanceData


The next stage removes `MaterialPropertyBlock` arrays and moves
instance data into **GPU structured buffers**, allowing the renderer to scale
far beyond the `1023` instance limit of `DrawMeshInstanced`.

In the current approach the CPU must upload per-instance data (such as
transforms or metadata arrays) to the GPU every frame. As instance counts
grow, this becomes increasingly expensive in both **CPU time** and
**memory bandwidth**.

By storing instance data directly in **GPU buffers**, the renderer can:

- eliminate most per-frame CPU → GPU data uploads
- reduce CPU workload when rendering large instance counts
- allow the GPU to read instance data directly from buffers
- enable more advanced techniques such as **indirect drawing and GPU-driven rendering**

This transition is a key step toward building a **fully GPU-driven renderer**.