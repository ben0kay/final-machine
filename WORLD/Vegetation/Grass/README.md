# ForestGrass001 patch scatter

`GrassScatter.cs` builds a one-shot, deterministic spread of small grass patches on the native saved Terrain3D surface. `Configuration/GrassMaster.tres` owns the editable seed, density, shape, scale, tint and LOD settings. The world scene owns a standalone Grass node; the pine forest is not responsible for grass.

## Placement and appearance

- Independent seeded patch centers use a low-frequency noise field plus the terrain integration layer's `woodland` mask, with a minimum distance between accepted patch centers. The mask keeps most grass off the bright central clearing; it is **not** a query of Terrain3D's manually painted texture/control map.
- Each patch contains 12–28 tufts in an irregularly rotated elliptical area of approximately 1.5–3.5m radius. Each tuft has its own position, rotation, modest width variation and scale (normally 0.46–0.78 of the source asset's ~0.75–0.9m height).
- Tufts are grounded against native saved height/normal queries. Very steep ground, high elevations and positions outside the finite authored boundary are rejected. The root is buried 3.5cm for soil contact.
- Three ForestGrass001 mesh variants (A/B/C) are distributed per tuft. Subtle patch-level and per-tuft `MultiMesh.SetInstanceColor` tints multiply the asset's existing vertex colors in its shader; no duplicated materials. The existing dark atlas and wind/distance-fade material are preserved.

## Rendering / cost

- Grass is rendered as spatial `MultiMeshInstance3D` batches (default 32m cells), each with LOD1 (0–26m), LOD2 (26–72m), and native culling thereafter. The grass wind material further fades its geometry at approximately 44–68m.
- Grass does not create individual gameplay nodes or collision, does not cast shadows, and has no per-frame CPU scatter/update callbacks; shader wind is GPU-side. The finite scatter is built once on startup, so increasing patch count raises startup placement/terrain-query cost and resident instance/batch memory.
- ForestGrass001 LOD1 geometry is very light (about 54–66 triangles per tuft). Despite low triangles, alpha overdraw, wind shading, batch/draw overhead and overall coverage should be tested on the target GPU. Fog occlusion **does not** stop GPU work by itself.
- `PatchAttempts` is a sampling budget, not the final patch count. `PatchDensity`, `NoiseThreshold`, radius and tufts-per-patch should be tuned by local FPS/appearance. If the forest needs much denser coverage, consider viewport-cell streaming rather than multiplying the number of global batches.

The pine forest separately uses `ForestMaster.tres` `TreeKeepFraction = 0.85` to remove approximately 15% of original accepted pines while preserving original placement/spacing and model rotations of survivors. Both scatterers are deterministic and do not rewrite terrain or source assets.

## Local acceptance

Not yet compiled or viewed in Godot in this task (Godot/.NET unavailable in the editing environment). Pull, let models import, build C# and run `WORLD/Scenes/test.tscn`. Verify patch distribution/forest edges, individual grass tints, A/B/C geometry, wind and ~26m LOD transition, ~68–72m disappearance, and FPS during movement. The central clearing is intentionally sparse; walk into woodland to inspect. Adjust configuration in `GrassMaster.tres` rather than baking geometry. No runtime terrain painting or grass persistence/streaming is added.
