# FM0 ferns — independent woodland understory

`FernScatter.cs` runs once after `GrassScatter` signals completion, using actual populated grass patch centres and the saved Terrain3D habitat sample. `WORLD/Vegetation/Configuration/FernMaster.tres` is the editable settings resource.

Placement target: floor(actual grass tufts / GrassToFernRatio), default **6:1**, capped at 7,500 ferns. The resulting count can be lower after filtering, and the console prints the actual ratio. Only about 62% of populated grass patches are eligible for fern groups. Random 0–4.75m offsets, 1.3m fern spacing, variable scale 1.45–2.25, yaw, width and ±4% tint avoid uniform distribution.

Saved Terrain3D hole/rock paint, height/slope, woodland cover, dry streambed and pine trunks are respected. The owner caches approximate kept trunk positions only during one-shot scatter. No per-fern processing, physics collision or shadow casting.

Asset: `ASSETS/Vegetation/Plants/Ferns001/Models/ferns00` followed by a literal backtick and `.gltf`. Its four source meshes are selected directly without the sample-sheet node translations. The lightweight A and D shapes (~784/816 triangles) appear about 82% of the time; B and C (~2384/2248 triangles) add occasional fullness. No geometry or texture pixels were altered.

`Materials/FernWind.tres` now refers to a provided `WORLD/Vegetation/Wind/plant_wind.gdshader`: double-sided alpha cutout, subtle vertex wind, existing diffuse/normal/roughness maps and fade ~31–44m. Single-tier 32m spatial MultiMeshes cull at 46m. This is NOT a full authored fern LOD system. Ferns are significantly more expensive per plant than grass; uncapped FPS and overdraw must be evaluated in Godot.

Local checks pending: GLTF/material import, all four fern variations, natural grass-to-fern balance, material alpha and wind, trunk/rock/river exclusions, visual scale, no popping at ~44–46m and GPU performance. No Godot/.NET build was available in this editing environment.