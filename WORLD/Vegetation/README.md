# FM0 vegetation — pines and forest-floor grass
Forest/PineForest.cs scatters once using Configuration/ForestMaster.tres. Saved Terrain3D height/normal queries are isolated behind surface.gd. Seeded rejection sampling, minimum spacing, irregular woodland boundaries/glades, upright modestly scaled trees, root burial and steep-slope/high-mountain rejection avoid grids and floating trunks. Grass is owned separately by Grass/GrassScatter.cs; no saplings/other tree species.

The original PineTree002.tscn is preserved: its Terra Machine LOD/static-collision helper dependencies are absent here. The isolated forest loads the three source glTF scenes and shares their meshes/materials. Native Terrain3D scene instancing was bypassed for this first pass to preserve explicit three-model LODs without addon-specific scene conversion or duplicating source art.

24m spatial MultiMesh batches use identical conservative bounds and Godot native visibility ranges: 0–70 / 70–280 / 280–600m, 3m hysteresis. All tiers use exactly the same transforms; batch-level switching can still be visible and needs local review. LOD3 casts no shadows; the sun limits all shadows to 100m. Tree height stays approximately 22.6m ±12%. One static body holds shared approximate trunk capsules (no individual processing); collision does not switch with visual LOD. The capsule is intentionally approximate and does not match every branch or modest scale variation.

Cost: one initialization scatter and MultiMesh build, then no forest process callbacks. Rendering, alpha overdraw, spatial batch count and static collision proxies still have costs. This is a finite benchmark, not streaming. Tune attempts/spacing/batch size and distances together after measuring uncapped FPS; no measured FPS improvement is claimed.

## Active forest floor: ForestGrass001 and ForestGrass002 filler

Two independent, one-shot mesh-grass systems run in `WORLD/Scenes/test.tscn`. The original `GrassScatter` keeps ForestGrass001's clustered patches, two LODs and fern-growth metadata. The new `GrassFiller` fills bare woodland between those patches with the lower-poly ForestGrass002 multi-plane cards; its density, appearance, 65,000-tuft hard cap and 22m/56m LOD distances are independent (`Configuration/ForestGrass002FillerMaster.tres`).

Both respect Terrain3D habitat, painted rock/hole exclusion, native height/slope and pine-trunk clearance. ForestGrass002 builds batched MultiMeshes once, with no per-frame C# callbacks or grass shadows/collisions. The Terrain3D GPU-particle demo remains disabled; its files are retained for comparison. Fern scatter still uses only ForestGrass001's population/anchors. See [Grass/README.md](Grass/README.md) for tuning. Godot visuals and FPS are pending owner testing.
