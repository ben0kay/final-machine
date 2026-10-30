# ForestGrass001 — fine blade replacement (A/B/C, LOD1/LOD2)

Replaces the former broad photographically cut-out tuft cards with slim, individually modelled, non-transparent grass ribbons. The file naming, model paths and transforms are **unchanged** so the existing `GrassScatter.cs` remains the owner of placement, batching, instance scale, native LOD changes and culling. No density/trees/ferns/code changes are required.

## Geometry budget and source heights

| Variant | LOD1 triangles | LOD2 triangles | Native max height |
|---|---:|---:|---:|
| A — low spreading | 90 | 24 | 0.777 m |
| B — balanced | 96 | 28 | 0.867 m |
| C — taller | 92 | 30 | 0.952 m |

LOD1: asymmetrically spread fine blades with 18/20/18 bent main leaves (three triangles each) and 36/36/38 thinner low filler leaves (one triangle each); no duplicate back faces. LOD2: selected tallest and outer fine blades retained with the same original silhouette shape and rooted origin, reducing total blade population to 14/16/18. All six have geometric face normals agreeing with exported vertex normals (minimum dot >0.30), no negative-scale transforms and correct triangulated mesh data.

## Colour/material

Regenerated source `forest_grass_001_dark_atlas_1k.png` and `forest_grass_001_green_atlas_1k.png` with the **same filenames**. The two files now contain a brighter green 1K pigment/venation atlas. All pixels are opaque because the blade silhouettes come from their actual geometry, reducing the discarded-pixel cost of the old broad alpha cards. The third historical grass-meadow-named atlas remains untouched. Mipmaps are enabled in import metadata. Source mesh GLTF material is double-sided and opaque. The shared ForestGrass001 wind shader preserves existing wind, root anchoring, light-dependent backlight, foliage tint and 44–68m distance-collapse controls; it no longer writes ALPHA or uses alpha scissor. Use the supplied `WORLD/Vegetation/Grass/Materials/ForestFloorGrass.tres` so the **live forest**, not just asset previews, samples the new atlas instead of the old GrassMeadow002 atlas.

## Install and tests

Extract this ZIP **into the Godot project root containing `project.godot`**, not into `ForestGrass001` itself. Allow overwrite/merge; the target paths are `ASSETS/Vegetation/Grass/ForestGrass001/...` and `WORLD/Vegetation/Grass/Materials/ForestFloorGrass.tres`.

Back up or commit local changes first. Open Godot, let GLTF/texture imports update, build C#, then view `WORLD/Scenes/test.tscn`. Compare A/B/C from opposite sun directions, check short and far grass, wind/LOD fading and uncapped FPS. No local Godot/OpenGL run was available to verify visuals or performance; this is a first-pass authoring candidate, not a validated equivalent of commercial grass. Actual coverage still depends on unchanged scatter settings, and single ribbons may look sparse where there are too few tufts.

## Data validation performed

- GLTF parser checks: six `.gltf` files with correct `.bin` references and available textures; no missing bytes.
- Counts, source height, face winding/normal alignment, texture UV bounds and finite coordinates checked for all variants and LODs.
- Runtime material checked to reference the replacement atlas.
