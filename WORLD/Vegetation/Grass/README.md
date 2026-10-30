> **Active in the test world:** `Grass.RenderGrass=true`; the Terrain3D GPU-particle demo is disabled through `DemoGrassMaster.Enabled=false`. The existing A/B/C ForestGrass001 MultiMesh scatter renders again, and still provides the placement metadata for ferns.

# ForestGrass001 forest-floor grass — diffuse growth pass

`GrassScatter.cs` builds one deterministic, Terrain3D-grounded scatter into A/B/C LOD1/LOD2 MultiMesh spatial batches. `Configuration/GrassMaster.tres` owns the editable density, patch shape, height, width and 50,000-tuft hard cap. No new vegetation species, source meshes, forest/fern controller, sky, fog, terrain or tree positions were changed in this pass.

## Latest wider-tuft appearance pass

With ForestGrass001 re-enabled, the test world uses **1.75–2.25 width multipliers** (formerly 1.18–1.48) for substantially broader, lower-looking tufts at the **same vertical scale**. Individual nominal width is roughly 1.5× the prior value, so the approximate horizontal footprint can be over twice as large; real ground coverage depends on mesh silhouette and habitat. Patch-centre minimum spacing is modestly reduced from 6.0m to 5.5m to reach more of the forest floor. The cap remains **50,000 tufts**, and existing slope/rock/trunk filtering, lod distances and ferns are preserved. Spatial MultiMesh bounds now allow 3.5m side padding for the wider visuals. This does not increase per-tuft triangles or claim an FPS improvement.

The newly authored **fine-blade replacement ZIP is not installed by this configuration-only GitHub change**: the game uses whichever `ForestGrass001/Models`, shader and atlas are present locally. Extract that ZIP into the Godot project root if the finer replacement files have not yet been applied, and inspect the active `ForestFloorGrass.tres` material. After applying the ZIP, commit its updated binary assets separately if they should persist on GitHub.

## Previous diffuse-growth pass

## Why this pass

The previous 2–4 offset cores per patch concentrated even generous tuft budgets into isolated dark, round mounds, leaving large bare-soil gaps between them. This pass samples tufts across the full ellipse using a softened 3/5-lobe irregular perimeter. Patches still rotate/stretch, vary density and overlap; local woodland/ground habitat noise, rock/stream exclusion and isolated growth areas remain. It **does not** promise a mathematically measured ground-cover percentage.

- Keep 22,000 patch-centre attempts, 0.82 candidate density, 6m centre spacing and 3.4–8m patch radius.
- Increase tuft targets from 24–68 to **36–90 per patch**, reduce the minimum within-patch tuft separation from 0.52 to **0.40m**, and raise the global safety cap from 45,000 to **50,000**. This is a modest population allowance, not a request to spawn 50,000 everywhere.
- Prefer an uneven height mix: ~32% short, 54% mid, 14% occasional taller clumps; scale **0.46–0.96** versus 0.72–1.20. Slightly wider relative blade-clump scale (1.18–1.48) helps shorter tufts read as connected ground vegetation rather than tall weeds.
- Grounded positions, patch/individual tint variance, partial slope alignment, tree trunk clearance, painted rock/hole exclusion and dry channel protection are preserved.

## New forest-floor colour treatment

The active `GrassScatter` runtime material is now `Grass/Materials/ForestFloorGrass.tres`. It keeps the existing, corrected ForestGrass001 source geometry, vertex normals, UVs, wind and fade shader, but uses the **existing greener GrassMeadow002 colour/alpha atlas** with a muted olive `foliage_tint` and modest directional backlight. Original `ForestGrass001Wind_DistanceFade.tres` remains intact; the shader's new tint uniform defaults to pure white, so the original material is unchanged. Green-atlas UV/alpha matching is based on the models' matching vertex/UV/index counts and source lineage, but **has not yet been visually verified in Godot**; inspect all A/B/C and near/far models for correct leaf texture mapping and alpha.

To compare against the previous dark appearance, point the `GD.Load<ShaderMaterial>` material path in `GrassScatter.cs` back to `ASSETS/Vegetation/Grass/ForestGrass001/Materials/ForestGrass001Wind_DistanceFade.tres`.

## Rendering/performance and validation

Existing grass remains two-tier MultiMesh (LOD1 0–26m, LOD2 26–72m) with shader fade 44–68m, 32m batches, no collision or shadow casting, one-time scatter and GPU wind. The increase in tuft allowance can increase placement queries, alpha overdraw and GPU load even with 54–66 near / 24–30 far triangles per tuft. Ferns still target roughly one per six **actual placed** grass tufts up to the separate fern cap; actual ratio can differ because of habitat rejection. The pine forest, fern scatter logic and materials are unchanged.

**Pending owner acceptance:** Pull/reimport, compile and walk the same camera view as the supplied reference screenshot; inspect softer/greener ground coverage, connected patch edges, natural open spaces, leaf atlas/alpha, all three A/B/C and LOD transitions, sun-front/back shading, dark fog/lighting and uncapped FPS. No Godot build, in-engine render or FPS measurement was performed in this pass.


## Active secondary layer: ForestGrass002 woodland filler

`GrassFiller` in `WORLD/Scenes/test.tscn` uses the existing lightweight ForestGrass002 A/B/C glTF meshes and alpha/wind atlas **between** ForestGrass001's larger patches, without changing ForestGrass001 or fern anchor data. It uses one seeded, jittered-grid placement across suitable woodland, independent of existing patch centres. Mild coherent noise creates natural thinner and denser pockets. Terrain3D's `grass_sample` keeps grass off painted rock, terrain holes, steep slopes and the dry stream centre. Existing pine-trunk proxies are consulted for clearance.

`Configuration/ForestGrass002FillerMaster.tres` provides independent on/off, seed, grid spacing (1.95m default), coverage (0.88 before terrain tests), random jitter, width (1.50–2.10), short height (0.62–1.05), cap (65,000 **maximum**, not an expected count), batching and two LOD distances (22m switch, 56m cull). A and B account for 88% of variants; C is a taller accent. The capped jittered grid is visited in a pseudorandom coprime order to avoid filling only the map's first rows when saturated.

A dedicated `Grass/Materials/ForestGrass002Filler.tres` uses the original 002 shader/atlas with a muted olive tint, alpha-to-coverage, GPU wind and 36–54m fade. Each spatial cell is rendered in two Godot MultiMesh visual LODs, with no shadows, collision or per-frame C# processing. Lower `GridSpacing` for denser cover (increases build queries approximately quadratically); reduce `Coverage` or `CullDistance` for lower cost, and adjust material fade to match cull. Set `Enabled=false` to compare with only 001 after restarting. No engine visual or FPS benchmark has been run yet.

## Shared pine/grass palette controls

The current ForestGrass001 shader also reads the project Shader Globals `forest_tint` and `forest_tint_strength`, alongside the new pine-needle shader and ForestGrass002 filler shader. Its material retains existing local `foliage_tint`, wind, opaque narrow ribbons, distances, batching and every scatter setting. The global tint preserves source pixel luminance, with `palette_response=0.85` independently editable on the material. At strength 0, the global adjustment is disabled. Open F1 → Forest foliage / pine + grass for runtime changes; make lasting edits under Project Settings → Shader Globals. Local appearance/FPS acceptance pending.
