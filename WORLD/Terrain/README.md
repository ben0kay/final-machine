# Native Terrain3D landscape

Terrain3D **1.0.2**, sixteen 256m regions at 1m spacing: 1024 × 1024m. Terrain3D owns saved heights, paint and collision. No voxel code or custom terrain shader. Game startup never generates, imports, repaints or saves terrain.

# Test-world Ground037 override — 2026-10-30

`WORLD/Scenes/test.tscn` overrides the instanced Terrain3D node's `assets` property with `Configuration/TerrainAssetsGround037_Test.tres`. This is only a material palette change in the existing Terrain3D test world: no heights, saved painted texture IDs, geometry, native regions, collision, procedural grass or tree placement data were changed.

**Ground texture slots 0 and 1** retain their original `Meadow` and `Woodland` names and numeric IDs to protect Terrain3D painting and the existing grass habitat rules. Both now sample the copied War Machine/ambientCG CC0 **Ground037** albedo-height and normal-roughness maps under `ASSETS/Terrain/WarMachineGround037/Textures/`; the tint `Color(0.67451, 0.74902, 0.686275, 1)`, `uv_scale=0.2`, `detiling_rotation=0.161`, AO strength 2, and normal depth 1 follow War Machine's grass ground reference. The two ground slots intentionally share the same maps: the former meadow/woodland colour contrast is temporarily absent while comparing the simplified ground look.

**Rock texture slot 2 is identical** to the original `Configuration/TerrainAssets.tres`: `GrayRocksAlbedoHeight.png`, `GrayRocksNormalRoughness.png`, ID 2, existing UV/detiling and normal strength. Painted rock/cliff areas remain rock. Original terrain shader/auto-slope behaviour remains unchanged; this pass does not add new rock painting or copy War Machine's cliff shader/rock art.

**Revert:** remove the local `assets = ExtResource("18")` property override from the `Terrain` instance in `WORLD/Scenes/test.tscn` (and remove the matching ext_resource declaration), returning it to the original `WORLD/Terrain/Configuration/TerrainAssets.tres` referenced by `Integration/Terrain3D/terrain.tscn`. The source asset collection and Test2 asset collection remain untouched.

The separate `WarMachineTerrainMaterial.tres` with War Machine's macro variation/dual scaling is **not activated**; this test reproduces the Ground037 material asset settings, not the entire War Machine terrain look. Review the original painted rock boundary and ground detail at normal first-person height after pulling. The switch and rendering have not been verified inside Godot; a Git commit is not a render test.

## Active geography and recoverable baseline

`Integration/Terrain3D/terrain.tscn` now loads **Landscape/RegionsEnhancedV1**. The original **Landscape/Regions** files are retained byte-for-byte as the recoverable pre-enhancement baseline. To roll terrain back, change only the scene's `data_directory` to `res://WORLD/Terrain/Landscape/Regions`; the integration disables stream habitat rules when the enhancement snapshot is absent. Grass distribution tuning is independent.

The enhanced version adds a west-facing, irregular layered escarpment west/northwest of spawn, around X=-190, Z=-220…-140. Sampled cross-sections rise 112–133m, with broken ledges and gullies, steep rock faces and broad transitions behind the ridge. Heightmaps cannot form overhangs; no separate cliff meshes were needed. Low-amplitude woodland ridges/depressions leave the central spawn clearing unchanged.

A dry meandering channel runs through eastern woodland, approximately X=80…120, Z=-165…140. Its main section varies about 3.3–6.7m wide and 1.25–1.65m deep, tapering at its ends. Look east of the clearing or follow it north toward the mountain shoulder. Banks are sloped; water, hydraulic simulation and water-level authoring are not implemented. Final water placement will still need a local channel/profile review.

## Explicit, safe authoring

For the original initial generator, `bake_landscape.gd` still refuses any saved destination. For this enhancement use, from the project root:

```sh
godot --headless --path . --script res://WORLD/Terrain/Integration/Terrain3D/enhance_landscape.gd -- --source=res://WORLD/Terrain/Landscape/Regions --output=res://WORLD/Terrain/Landscape/RegionsEnhancedV2
```

- Both directories must be explicit and distinct; output must be empty. Existing output is never deleted or overwritten.
- The source must be the sixteen-region unenhanced baseline. A source with an enhancement snapshot is rejected to prevent adding the delta twice.
- Read the current source's saved height/control/colour maps. Deep-copy native regions, add deterministic `landforms.gd` enhancement deltas to **saved heights**, preserve native colour/holes/navigation/texture rotation/scale data and increase rock paint where the new slopes demand it. Existing authored deviations are retained as the source of truth, rather than replaced by `sample()`.
- Save all sixteen regions to the new directory, plus `LandscapeBake.cfg` (seed/authoring settings snapshot) and `BaselineManifest.json` (source region SHA-256 hashes). Never switch the active scene automatically. Review and explicitly change `terrain.tscn` after a successful bake.
- Do not rerun into RegionsEnhancedV1: it is now authored terrain. Use a fresh output directory and review the diff; preserve any later edits before replacing the active version. This tool is not a merge of edits made independently to two terrain versions.

`Configuration/LandscapeMaster.cfg` controls the authoring seed, original landforms and enhancement height/detail/channel depth. Editing it does **not** change saved geography on launch. The active integration reads the baked snapshot for woodland/channel habitat rules so later default changes do not move the grass channel away from the saved streambed.

## Editing, materials and vegetation

Open `WORLD/Scenes/test.tscn`, select Terrain and sculpt/paint with Terrain3D, then save terrain data through Terrain3D and commit changed native regions. Collision uses those same saved heights. Trees and grass query the actual saved surface once at launch; runtime sculpt/re-grounding is outside scope. Pine models, seed, thinning/density, spacing and LOD ranges are unchanged. Changed slope/height acceptance naturally changes accepted trees near reshaped ground.

`TerrainAssets.tres` retains grass_ground, forest_ground_06 and gray_rocks: native base/overlay paint, packed 1024px albedo-height/normal-roughness maps, ~4m repeat, mipmaps and existing texture settings. Steep new faces receive rock coverage. Grass now reads native rock paint and holes as well as heights/normals; woodland grass remains sparse in the dry channel centre, with richer suitable banks. No new terrain material/shader or runtime painting.

## Focused validation

Passed: .NET build (zero warnings/errors), native sixteen-region load, original SHA-256 verification, saved additive-height samples, 112–133m cliff transects/steep normals/rock paint, sparse stream-centre habitat, and real collision agreeing with saved heights at spawn/stream/cliff. Headless .NET scene startup built the pines and grass without script errors. No rendered-quality or FPS claim: locally inspect cliffs, near-field texture scale on rock, channel banks/continuity, tree roots, grass coverage and walking comfort. Existing `check_native.gd` remains the basic installed-addon check; the owner handles extended visual/FPS acceptance.
