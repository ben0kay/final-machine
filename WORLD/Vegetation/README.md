# FM0 vegetation — pines and forest-floor grass
Forest/PineForest.cs scatters once using Configuration/ForestMaster.tres. Saved Terrain3D height/normal queries are isolated behind surface.gd. Seeded rejection sampling, minimum spacing, irregular woodland boundaries/glades, upright modestly scaled trees, root burial and steep-slope/high-mountain rejection avoid grids and floating trunks. Grass is owned separately by Grass/GrassScatter.cs; no saplings/other tree species.

The original PineTree002.tscn is preserved: its Terra Machine LOD/static-collision helper dependencies are absent here. The isolated forest loads the three source glTF scenes and shares their meshes/materials. Native Terrain3D scene instancing was bypassed for this first pass to preserve explicit three-model LODs without addon-specific scene conversion or duplicating source art.

24m spatial MultiMesh batches use identical conservative bounds and Godot native visibility ranges: 0–70 / 70–280 / 280–600m, 3m hysteresis. All tiers use exactly the same transforms; batch-level switching can still be visible and needs local review. LOD3 casts no shadows; the sun limits all shadows to 100m. Tree height stays approximately 22.6m ±12%. One static body holds shared approximate trunk capsules (no individual processing); collision does not switch with visual LOD. The capsule is intentionally approximate and does not match every branch or modest scale variation.

Cost: one initialization scatter and MultiMesh build, then no forest process callbacks. Rendering, alpha overdraw, spatial batch count and static collision proxies still have costs. This is a finite benchmark, not streaming. Tune attempts/spacing/batch size and distances together after measuring uncapped FPS; no measured FPS improvement is claimed.

## Forest floor: grass + ferns

`Grass/GrassScatter.cs` now spreads the existing ForestGrass001 A/B/C models throughout irregular overlapping woodland patches rather than concentrating them in 2–4 small cores. It deliberately includes shorter/wider ground-cover clumps, a modestly larger tuft budget and a new active muted-green forest-floor material using the existing GrassMeadow002 atlas. The original dark material and source models are preserved for quick comparison. See [grass authoring and tuning](Grass/README.md) and [vegetation asset guide](../../ASSETS/Vegetation/VISUAL_AUTHORING_GUIDE.md).

`Ferns/FernScatter.cs` is unchanged: it follows the actual populated grass patch centres and places Ferns001 at an approximate 6:1 grass-to-fern target, bounded by its own cap and terrain checks. See [fern tuning](Ferns/README.md).

Both use saved Terrain3D habitat/rock/hole/stream/trunk exclusions, spatial GPU MultiMesh batches, no foliage collisions or shadows and no per-frame CPU updates. More grass coverage may increase one-shot setup and alpha overdraw despite unchanged LOD distances. No trees, terrain, fog or environment settings changed. This appearance pass has not been compiled or visually validated in Godot; user acceptance and uncapped FPS remain pending.
