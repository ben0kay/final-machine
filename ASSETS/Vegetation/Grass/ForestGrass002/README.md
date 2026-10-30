# ForestGrass002 — crossed/tilted alpha grass cards (independent test asset)

**Purpose:** Compare cheaper forest-floor grass cards against ForestGrass001 without replacing any existing asset, materials, scene paths or source files. These are **fixed angled/offset alpha cards, not camera-facing billboards**. A/B/C have different amounts of splay and height. The authoring assembly—yaw, tilt, stagger, different sizes, atlas selection—lives *inside the glTF mesh*. In Godot, randomize the **whole tuft** (world position, yaw, and non-uniform scale) only.

## Design techniques from the user-provided billboard-grass video

- **Five to six near cards**, offset rather than centered, with 360-degree irregular yaw.
- **Differing heights and widths**, some lower, some taller; cards lean outward and bend slightly instead of being uniformly vertical.
- **Four UV atlas regions** with distinct natural grass sprays, used in different combinations in A/B/C.
- **Base transparency fade** is painted into the RGBA atlas. For the Godot runtime material, the shader uses alpha scissor plus alpha-to-coverage for smoother edges (best with Forward+ MSAA 2x+; otherwise it degrades to dithered edges).
- **Three far cards** retain a similar silhouette at the same ground-root origin.
- No per-blade scripts, colliders or grass shadows; no new runtime CPU update loop. Uses the standard grass wind shader, physically grounded normals, and light-dependent backlighting.

## Geometry

| Variant | LOD1 cards | LOD1 tris | LOD2 cards | LOD2 tris | Native max height |
| --- | ---: | ---: | ---: | ---: | ---: |
| A | 5 | 20 | 3 | 6 | 0.541m |
| B | 5 | 20 | 3 | 6 | 0.657m |
| C | 6 | 24 | 3 | 6 | 0.702m |

One LOD1 card has two quads (four triangles) to allow a gently bowed shape; LOD2 has one quad (two triangles) per card. Source heights are pre-scatter and will be multiplied by Godot instance scale. One mesh/material surface per asset. This saves model triangles relative to ForestGrass001's ~90-triangle recent replacement, **not guaranteed GPU frame time**: broad alpha cards can cause overdraw even with low triangle count.

## Install and preview — does not override ForestGrass001

1. Extract this ZIP into the Godot project root containing `project.godot`.
2. Open Godot and allow the GLTF and PNG resources to import.
3. Open `res://ASSETS/Vegetation/Grass/ForestGrass002/ForestGrass002_Preview.tscn`, then inspect it in the 3D editor or press **F6** to view the A/B/C models. This preview uses their imported glTF alpha-mask material and a simple light/ground, without depending on the existing forest.
4. The separate shader material `Materials/ForestGrass002Wind_DistanceFade.tres` is ready for use by a MultiMesh grass scatter, but the **current** `GrassScatter.cs` still references ForestGrass001 models and `WORLD/Vegetation/Grass/Materials/ForestFloorGrass.tres`. This package deliberately **does not switch** the main world to the new asset. To do so, change the model filename stem and the loaded shader material path in the scatter, or add an inspector-driven asset selection feature separately. Do not replace working ForestGrass001 until you've compared them.

## Blender editing

The `.gltf` files are editable in Blender using File → Import → glTF 2.0. A native `.blend` was **not** produced here because Blender was not installed. As an optional convenience, run `Source/blender_import_and_save.py` with Blender 4.x to import the six models and save `Source/forest_grass_002_source.blend` on your computer. Godot does **not** require this source file at runtime.

## Validation and tradeoffs

- Created all six glTF/`.bin` pairs with a shared RGBA atlas, encoded alpha-mask imported PBR material, and separate Godot wind material.
- Verified vertex bounds, UV ranges, binary lengths, face winding, calculated normals and per-variant/LOD triangle budgets.
- No local Godot import, shader rendering, sun-direction test, MSAA review or performance benchmark was available. The screenshot/visual acceptance must be done in your Godot editor.
- If the base appears too thin under alpha cutout, adjust `alpha_cutoff` from `0.40` down to `0.30` as a test; don't mask invalid winding or normal direction with extra brightness. Look from multiple viewing directions and compare LOD change around your existing ~26m switch.


## Main-world use (2026-10-30)

Now used additively by `WORLD/Vegetation/Grass/ForestGrass002Filler.cs`, as `GrassFiller` in the test scene. The original six models, alpha atlas, shader and standalone preview remain unchanged. Whole-tuft world rotation/scale is randomized at scatter time; the individual card yaw/tilt/offsets remain authored in each glTF. Independent density/render settings live in `WORLD/Vegetation/Configuration/ForestGrass002FillerMaster.tres`.
