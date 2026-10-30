# Ferns001 — authored fern collection

The source GLTF file has the **literal** basename `ferns00` plus a backtick character (ASCII 96). Keep that name and its matching `.bin` unchanged to preserve existing paths/URI references. The four source fern meshes have sheet-layout translations; runtime scatter takes the raw submeshes and grounds them independently.

Source triangle counts from GLTF: A 784, B 2,384, C 2,248, D 816. Approximate local heights (maxY-minY): A 0.29m, B 0.43m, C 0.35m, D 0.21m. Larger instance scales provide variation and readable forest-floor silhouettes.

Original textures, geometry, binary data, GLTF and PBR inputs are not modified. The scatter uses `Materials/FernWind.tres` with an added `WORLD/Vegetation/Wind/plant_wind.gdshader` (previously missing at this referenced path): masked cutout, slight vertex wind and normal/roughness sampling. It is intended for efficiently batching many ferns rather than original GLTF alpha blending.

Check all variants under sun from front/back/side, normal-map orientation, alpha cutoff, wind and root positions in Godot. These checks and FPS are not yet verified. Ferns have no authored far LOD, so runtime culling is short.