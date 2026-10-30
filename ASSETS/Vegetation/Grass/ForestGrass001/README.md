# ForestGrass001

Three authored variants (A/B/C), each with LOD1 and LOD2, plus a dark atlas and wind materials. All model, buffer, texture, material and shader filenames and resource references use ForestGrass001 / forest_grass_001. Internal glTF mesh/node names match each variant's filename. The green-named atlas is a retained identical copy of the dark atlas from the upload; existing materials use the dark atlas.

Renaming preserves geometry, texture bytes, import settings and UIDs. Corrected the uploaded glTF buffer URIs (previously meadow_grass_002) and material paths (previously ForestGrass003). In Final Machine, `WORLD/Vegetation/Grass/GrassScatter.cs` now scatters these A/B/C meshes as independent patches grounded to Terrain3D. The active material is `Materials/ForestGrass001Wind_DistanceFade.tres` using the dark atlas and wind/distance-fade shader. Its patch density, sizes, tint and two LOD distances are configured in `WORLD/Vegetation/Configuration/GrassMaster.tres`. Prior Meadow Grass 2 replacement/starting-fir notes were from the separate reference project and do not describe this new world.

Validation: parsed all six glTF files, checked buffer byte lengths and local texture/material/shader references. Godot import/rendering of this new scatter remains to be verified locally after pulling.

Visual acceptance for this atlas substitution is pending: source models have matching UV/vertex/index counts to Meadow002, but the exact leaf/alpha alignment and in-engine colour must be checked for A/B/C and both LODs before calling the treatment accepted.
