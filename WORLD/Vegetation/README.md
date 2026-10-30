# FM0 vegetation — pines and forest-floor grass
Forest/PineForest.cs scatters once using Configuration/ForestMaster.tres. Saved Terrain3D height/normal queries are isolated behind surface.gd. Seeded rejection sampling, minimum spacing, irregular woodland boundaries/glades, upright modestly scaled trees, root burial and steep-slope/high-mountain rejection avoid grids and floating trunks. Grass is owned separately by Grass/GrassScatter.cs; no saplings/other tree species.

The original PineTree002.tscn is preserved: its Terra Machine LOD/static-collision helper dependencies are absent here. The isolated forest loads the three source glTF scenes and shares their meshes/materials. Native Terrain3D scene instancing was bypassed for this first pass to preserve explicit three-model LODs without addon-specific scene conversion or duplicating source art.

24m spatial MultiMesh batches use identical conservative bounds and Godot native visibility ranges: 0–70 / 70–280 / 280–600m, 3m hysteresis. All tiers use exactly the same transforms; batch-level switching can still be visible and needs local review. LOD3 casts no shadows; the sun limits all shadows to 100m. Tree height stays approximately 22.6m ±12%. One static body holds shared approximate trunk capsules (no individual processing); collision does not switch with visual LOD. The capsule is intentionally approximate and does not match every branch or modest scale variation.

Cost: one initialization scatter and MultiMesh build, then no forest process callbacks. Rendering, alpha overdraw, spatial batch count and static collision proxies still have costs. This is a finite benchmark, not streaming. Tune attempts/spacing/batch size and distances together after measuring uncapped FPS; no measured FPS improvement is claimed.

## Active forest floor: Terrain3D demo grass trial

The world now renders the installed Terrain3D demo's GPU-particle ribbon grass with a muted forest palette, varied size and much less clod amplification. It samples saved native terrain maps around the camera, avoids rock/holes/steep ground and thins the dry stream centre. See [DemoGrass defaults and rollback](DemoGrass/README.md).

The existing Grass node has RenderGrass=false: no legacy ForestGrass001 meshes/material load or draw batches. Its one-shot metadata still supplies the unchanged fern owner's population/anchors. Original art and [legacy tuning](Grass/README.md) remain available. No tree, terrain, atmosphere, player or HUD changes.

Focused .NET build, shader-language parsing and headless startup passed: 25 emitters / 144,400 potential blade slots, zero old grass draws, 5,927 pines and 7,500 ferns. These are counts, not FPS measurements. Actual GPU execution, shading, coverage, fade and performance remain local checks.
