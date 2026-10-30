# ForestGrass001 patch scatter

`GrassScatter.cs` still scatters once on the saved Terrain3D surface into the existing spatial MultiMeshes. `Configuration/GrassMaster.tres` / `GrassSettings.cs` own tuning. The current committed A/B/C models, buffers, textures, imports, wind shader and shared material are preserved; no grass art was regenerated or recoloured.

## Natural growth areas

- Fewer candidate centres (18,000 attempts, previously 22,000), at least 7m apart instead of 5m. Broad noise/woodland masks retain bare woodland and the central clearing.
- Main growth areas use variable 2.3–5.4m radius, rotated/stretched overlapping cores, local noise gaps, variable density and up to 18–52 target tufts depending on size/fullness. These are irregular growth areas, not uniformly filled circles. About 14% of accepted centres request only 1–3 isolated tufts.
- Local tuft spacing (0.65m) limits tight overlap; some attempts fail because of rock, terrain, trunks or noise. A 40,000-tuft hard budget prevents accidental excessive growth during tuning; it is not a density target. The focused startup produced **21,121 tufts at 1,046 accepted centres across 640 cells**. Counts depend on saved terrain and settings; a centre can have fewer/no tufts after rejection.
- Source height remains unchanged. Runtime scales 0.48–0.72 give roughly 0.33–0.65m grass across source variants/LODs. Independent width multipliers 1.1–1.45 add fullness without tall blades. Random yaw, modest partial normal alignment and the existing subtle patch/instance tints break repetition.
- Native `surface.gd` `grass_sample` checks saved height, normal, hole and rock-paint blend. Steep or unsuitable ground is rejected. The baked channel profile suppresses centre growth to about 6% probability, and slightly enriches suitable banks. Later hand-sculpted channel rerouting requires updating habitat metadata separately; arbitrary brush edits are not inferred as rivers.
- `ForestPath` explicitly references the existing pine owner. After its deferred build, grass caches kept trunk proxy positions in small buckets, uses a 1.1m soil clearance, then frees that temporary cache. There are no per-tuft rays or forest updates. Keep the forest ahead of grass in scene/deferred initialization order if reusing this owner.

Patch-centre spatial buckets now retain **all** accepted points in each cell so spacing cannot lose an earlier centre; see root `LESSONS_LEARNED.md` VEG-002.

## Rendering / cost

Unchanged: three source variants, LOD1 0–26m / LOD2 26–72m, shared material fade 44–68m, 32m batching, no grass shadows/collision or per-frame CPU callbacks. Wind remains GPU-side. Source geometry is 54–66 near / 24–30 far triangles per tuft. Low triangles do not remove alpha overdraw/batch/shader costs. The added suitability/trunk checks cost initialization only; no measured GPU/performance improvement is claimed.

## Validation

Passed .NET build and a headless scene startup: source models/material load and both scatter owners finish without script errors. Native checks cover saved terrain/collision, cliff rock coverage and sparse channel-centre habitat. In Godot locally inspect patch silhouettes, spacing, colour under your lighting/fog, blade height and bank/trunk contact, existing LOD/fade transitions and uncapped FPS. Visual realism and no substantial FPS regression remain owner acceptance checks, not headless-verified results.
