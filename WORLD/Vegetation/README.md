# FM0 vegetation — pines and forest-floor grass
Forest/PineForest.cs scatters once using Configuration/ForestMaster.tres. Saved Terrain3D height/normal queries are isolated behind surface.gd. Seeded rejection sampling, minimum spacing, irregular woodland boundaries/glades, upright modestly scaled trees, root burial and steep-slope/high-mountain rejection avoid grids and floating trunks. Grass is owned separately by Grass/GrassScatter.cs; no saplings/other tree species.

The original PineTree002.tscn is preserved: its Terra Machine LOD/static-collision helper dependencies are absent here. The isolated forest loads the three source glTF scenes and shares their meshes/materials. Native Terrain3D scene instancing was bypassed for this first pass to preserve explicit three-model LODs without addon-specific scene conversion or duplicating source art.

24m spatial MultiMesh batches use identical conservative bounds and Godot native visibility ranges: 0–70 / 70–280 / 280–600m, 3m hysteresis. All tiers use exactly the same transforms; batch-level switching can still be visible and needs local review. LOD3 casts no shadows; the sun limits all shadows to 100m. Tree height stays approximately 22.6m ±12%. One static body holds shared approximate trunk capsules (no individual processing); collision does not switch with visual LOD. The capsule is intentionally approximate and does not match every branch or modest scale variation.

Cost: one initialization scatter and MultiMesh build, then no forest process callbacks. Rendering, alpha overdraw, spatial batch count and static collision proxies still have costs. This is a finite benchmark, not streaming. Tune attempts/spacing/batch size and distances together after measuring uncapped FPS; no measured FPS improvement is claimed.

## Forest floor: grass + ferns

`Grass/GrassScatter.cs` now places larger, more connected ForestGrass001 patches with three authored mesh variants and weighted height variation. Its `GrassMaster.tres` settings still respect saved terrain height, slopes, rock paint, holes, channels and kept pine trunks.

`Ferns/FernScatter.cs` adds Ferns001 into a subset of actual grass growth patches. It targets **one fern per six grass tufts**, where terrain and instance budgets permit. Ferns are larger, varied, mostly use lightweight source meshes and fade/cull closer than grass. Its own `FernMaster.tres` controls placement and rendering; see [grass tuning](Grass/README.md) and [fern tuning](Ferns/README.md).

Both use shared MultiMesh batches and shader wind without CPU per-frame work, collisions or shadows. Increased grass size/population and fern cards may increase alpha overdraw, initialization time and GPU cost. Local build/visual/FPS acceptance is pending; old benchmark 21,121 grass tufts / 5,927 pines is historical, not a new measurement. Pine trees, terrain data, fog and environment are unchanged.
