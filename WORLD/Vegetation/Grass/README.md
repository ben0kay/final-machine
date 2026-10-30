# ForestGrass001 patch scatter — fuller woodland coverage

`GrassScatter.cs` scatters once onto the saved Terrain3D landscape using spatial MultiMeshes. `WORLD/Vegetation/Configuration/GrassMaster.tres` owns its settings. Authored A/B/C mesh, atlas, winding fixes, imports and existing wind material are preserved without source modifications.

**Coverage pass:** 22,000 placement attempts at 0.82 density; minimum patch-centre spacing 6m; radius 3.4–8m; 24–68 target tufts per patch; 6% isolated patches and 0.52m within-patch tuft spacing. Unequal overlapping growth cores and local noise retain negative space and irregular patch edges. Patch radii can overlap naturally.

**Height and colour:** scale 0.72–1.20 (previously 0.48–0.72), with short/medium/tall weighted sizes (22% / 62% / 16%). Relative width multipliers 0.94–1.15 counteract the larger absolute scale, so nearby tufts are fuller but not all overly wide. Random yaw, partial normal alignment and subtle existing local/patch green tints remain.

The global MaximumTufts cap is now 45,000 as a safety bound, NOT a target. Visual **30–45% coverage is unmeasured** and must be judged in-engine. More size/density increases alpha overdraw and startup terrain queries; an acceptable uncapped FPS is not demonstrated by triangle counts or fog.

Native `grass_sample` continues checking saved height, normals, holes, rock paint and stream habitat. Tufts avoid kept pine trunks. The central clearing and rocky ground remain mostly free. After the one-shot build, populated patch centres and placed tuft count are exposed, and a completion event triggers the separate fern owner; no grass instance nodes or CPU frame processing are created.

Rendering remains A/B/C two-tier MultiMesh: LOD1 0–26m, LOD2 26–72m, material fades 44–68m, no grass collisions or shadows. Grass source triangles: 54–66 near and 24–30 far.

Godot build/rendering/FPS validation is pending. Check visible growth shapes and clearing/rock exclusions, mix of short and tall foliage, all three LODs/lighting directions, wind, fade and uncapped FPS against the prior 21,121-grass-tuft benchmark. If needed reduce PatchDensity/MaximumTufts before increasing draw distance.