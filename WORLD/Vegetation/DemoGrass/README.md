# Terrain3D demo grass trial

The test world now renders the **actual installed Terrain3D 1.0.2 demo GPU-particle grass**: `demo/Demo.tscn` → `addons/terrain_3d/extras/particle_example/Terrain3DParticles.tscn`. This uses 10-triangle RibbonTrailMesh blades and GPU terrain-map grounding, not the demo's separate TextureCard example or ForestGrass001 alpha cards.

## Ownership and styling

`demo_grass.tscn` is instanced beside Terrain with an explicit terrain_path. `demo_grass.gd` extends the installed demo owner as a thin native-addon boundary. Typed C# `DemoGrassSettings` / `Configuration/DemoGrassMaster.tres` own spacing, grid size, blade size, wind and colours. Materials are private runtime copies. Upstream addon/demo files and original grass art remain unchanged. Local shader derivatives retain upstream MIT attribution; see LICENSE-Terrain3D.txt.

The demo's tapered ribbons, random yaw, normal alignment, wind, distance growth/fade and terrain sampling remain. Styling adds muted green root/tip colours, restrained directional backlight and rougher highlights, with no emission or normal flips. Clod boost drops from 2.0 to 0.08; low patch thresholds and greater positional jitter give broader woodland coverage instead of exaggerated mounds. Scale 0.40–0.70 plus a small noise boost on the original 0.9m ribbon gives approximately 0.36–0.70m blades. Width varies independently.

Native paint IDs are adapted to Final Machine: meadow 0, woodland 1, rock 2. Soil coverage, slope and holes filter placement. The enhanced dry channel centre is sparse; its current path mirrors landforms.gd. A future channel reroute must update this shader mask too. Unlike legacy CPU placement, this trial does not use a separate per-trunk clearance mask.

## Cost and rollback

Defaults: 5×5 camera-following emitters, each with 76×76 slots: **144,400 potential blade slots**. Fade ends around 60m. The original demo widens blades with distance. Rejected particles still occupy GPU slots; this is not a promise of higher FPS.

The inherited GPU process runs at 30Hz. The CPU checks camera position each physics tick and recycles the bounded grid after roughly 1m movement; static terrain-map bindings upload only once. No per-blade nodes, CPU per-blade frame updates, shadows or collisions. Restart after preset/terrain rebinding changes; live terrain edits are outside this trial. The adapter corrects the example's reversed vertical AABB, and the local process shader handles missing finite-map layers safely.

`Grass.RenderGrass=false` skips legacy grass mesh/material loading and all MultiMesh draws. Its original one-shot placement still supplies counts/anchors to the unchanged FernScatter; its temporary setup work remains. The fern ratio refers to retained legacy metadata, not individual GPU blades.

**Restore the old appearance:** set Grass.RenderGrass=true in WORLD/Scenes/test.tscn, set DemoGrassMaster.Enabled=false, and restart. Original GrassMaster settings/materials/models remain intact. Trees, saved terrain, environment, controls and HUD are unchanged.

## Validation

Passed .NET build (zero warnings/errors), headless Godot shader-language parsing and scene startup. Focused assertions confirmed 25 emitters, positive bounds, shadows off, private shaders, zero legacy grass children, retained metadata and populated fern batches. Ribbon geometry reports 10 triangles/blade. Startup retained 5,927 pines and 7,500 ferns.

Local checks still required: actual Forward+/GPU execution, grass coverage, roots, front/back sunlight, movement/recycling, fade, colours under fog and FPS. Headless parsing cannot prove rendered appearance or performance. The editor import also reported host MSBuild initialization and unrelated missing demo MTL warnings; those were left outside this task.
