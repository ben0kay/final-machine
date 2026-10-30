# FM0 native Terrain3D landscape
Terrain3D **1.0.2**, inspected against installed importer.gd and upstream tag `v1.0.2-stable` (0077405b52e353c5e5dc3a094e7ede49833ba6fe). Native `import_images`, `save_directory`, `get_height`, `get_normal`, texture asset fields and control-map bit layout are version-specific. No voxel code or custom terrain shader.

## Ownership
- Landscape/landforms.gd: deterministic initial geography and woodland composition; no rendering. Inspired by Terra Machine's TestLandscape composition, independently implemented for native heightmaps.
- Configuration/LandscapeMaster.cfg: seed and large landform heights, used only for explicit authoring bake; changing it does not modify existing regions.
- Landscape/Regions: sixteen committed native 256m regions, 1m vertex spacing, total 1024×1024m centered on origin. Gentle central clearing, rolling shoulders, western escarpment/plateau, northern mountain silhouettes. The central ~256m is the initial benchmark.
- Integration/Terrain3D/terrain.tscn and surface.gd: native rendering/collision/saved-data boundary; forest samples saved heights/normals, never analytic heights.
- Configuration/TerrainAssets.tres: grass_ground, forest_ground_06 and gray_rocks. Native painted base/overlay blend; steepness selects rock, the irregular woodland mask blends grass/soil. No runtime repainting.

## Edit and preserve
Open WORLD/Scenes/test.tscn, select Terrain, use Terrain3D sculpt/paint brushes and save terrain data through Terrain3D. **Game startup never imports/generates/saves terrain.** Commit changed region .res files. Forest is re-grounded once on the current saved surface at launch. Runtime sculpting/re-grounding is outside FM0.

The explicit initial authoring command is:
`godot --headless --path . --script res://WORLD/Terrain/Integration/Terrain3D/bake_landscape.gd`
It refuses to write if ANY .res already exists in the destination. For a different landscape, duplicate the project/output destination deliberately; never delete authored regions merely to rerun this command. Keep seed unchanged if matching the existing woodland paint/placement composition.

## Materials
Packed 1024px PNGs preserve source RGB, displacement as albedo alpha, converted runtime OpenGL normal RGB and roughness as normal alpha. All sets use mipmaps, matching VRAM high-quality compression and **normal-map compression disabled** so roughness alpha survives. uv_scale=0.25 gives an initial ~4m repeat; inspect at eye level. No EXRs are required by the active material. Existing sparse_grass, forest_leaves_04 and brown_mud_leaves_01 source sets were inspected but are not used in FM0.

Full native collision is appropriate for this finite test. No streaming system. Headless native data/import/collision check: `godot --headless --path . --script res://WORLD/Terrain/Integration/Terrain3D/check_native.gd`. It checks region count, material count, all three models, finite spawn height and real collision. It does not validate rendering.
