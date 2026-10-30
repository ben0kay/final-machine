# Explicit additive bake into a NEW directory; preserves baseline heights, colour, holes and edits.
extends SceneTree
const Landforms = preload("res://WORLD/Terrain/Landscape/landforms.gd")
const CONFIG = "res://WORLD/Terrain/Configuration/LandscapeMaster.cfg"
var source_path := ""
var output_path := ""

func _initialize() -> void:
    for argument in OS.get_cmdline_user_args():
        if argument.begins_with("--source="): source_path = argument.trim_prefix("--source=").trim_suffix("/")
        elif argument.begins_with("--output="): output_path = argument.trim_prefix("--output=").trim_suffix("/")
    call_deferred("bake")

# =========================================================
# Never accept an implicit destination or a nonempty directory. A failed/partial output is not active.
func bake() -> void:
    var source_absolute := ProjectSettings.globalize_path(source_path).simplify_path()
    var output_absolute := ProjectSettings.globalize_path(output_path).simplify_path()
    if source_path.is_empty() or output_path.is_empty() or source_absolute == output_absolute:
        fail("Supply distinct --source=res://... and --output=res://... directories.")
        return
    if not DirAccess.dir_exists_absolute(source_absolute):
        fail("Source directory does not exist.")
        return
    if DirAccess.dir_exists_absolute(output_absolute) and (not DirAccess.get_files_at(output_path).is_empty() or not DirAccess.get_directories_at(output_path).is_empty()):
        fail("Output is not empty. Saved terrain will never be overwritten.")
        return
    if FileAccess.file_exists(source_path.path_join("LandscapeBake.cfg")):
        fail("Source already has an enhancement snapshot. Use the original baseline to avoid applying the delta twice.")
        return
    var terrain := Terrain3D.new()
    terrain.region_size = 256
    terrain.collision_mode = 0
    terrain.material = Terrain3DMaterial.new()
    terrain.assets = load("res://WORLD/Terrain/Configuration/TerrainAssets.tres")
    terrain.data_directory = source_path
    root.add_child(terrain)
    if terrain.data.get_region_count() != 16:
        fail("Expected the finite sixteen-region FM0 baseline.")
        return
    var land = Landforms.new()
    var regions: Array = []
    var sources := {}
    # Duplicate native resources/maps so writes cannot alias the loaded baseline.
    for region in terrain.data.get_regions_active():
        regions.append(region.duplicate(true))
        var name := "terrain3d%s%s.res" % [coord(region.location.x), coord(region.location.y)]
        sources[name] = FileAccess.get_sha256(source_path.path_join(name))
    for region in regions:
        var heights: Image = region.height_map
        var controls: Image = region.control_map
        var bytes := controls.get_data()
        for z in range(256):
            for x in range(256):
                var wx: float = region.location.x * 256 + x
                var wz: float = region.location.y * 256 + z
                var delta: float = land.enhancement(wx, wz)
                if absf(delta) < 0.00001: continue
                var position := Vector3(wx, 0.0, wz)
                var baseline: float = terrain.data.get_height(position)
                # Preserve holes rather than introducing finite heights at missing samples.
                if not is_finite(baseline): continue
                heights.set_pixel(x, z, Color(baseline + delta, 0.0, 0.0))
                var dx := enhanced_height(terrain, land, wx + 1.0, wz) - enhanced_height(terrain, land, wx - 1.0, wz)
                var dz := enhanced_height(terrain, land, wx, wz + 1.0) - enhanced_height(terrain, land, wx, wz - 1.0)
                var rock := smoothstep(0.65, 1.3, Vector2(dx, dz).length() * 0.5)
                var offset := (z * 256 + x) * 4
                var bits := bytes.decode_u32(offset)
                var blend := float((bits >> 14) & 255) / 255.0
                var old_rock := (1.0 - blend if ((bits >> 27) & 31) == 2 else 0.0) + (blend if ((bits >> 22) & 31) == 2 else 0.0)
                if rock > old_rock + 0.015:
                    var base := 1 if land.woodland(wx, wz) > 0.35 else 0
                    # Preserve angle/scale/hole/navigation/auto flags, replace only texture fields.
                    bits = (bits & 0x3fff) | (base << 27) | (2 << 22) | (int(rock * 255.0) << 14)
                    bytes.encode_u32(offset, bits)
        region.control_map = Image.create_from_data(256, 256, false, Image.FORMAT_RF, bytes)
        region.calc_height_range()
    if DirAccess.make_dir_recursive_absolute(output_absolute) != OK:
        fail("Cannot create output directory.")
        return
    # Terrain3D save_directory uses native location naming and marks native resources for saving.
    var result := Terrain3D.new()
    result.region_size = 256
    result.collision_mode = 0
    result.material = Terrain3DMaterial.new()
    result.assets = terrain.assets
    root.add_child(result)
    for region in regions:
        region.modified = true
        result.data.add_region(region, false)
    result.data.save_directory(output_path)
    var saved_regions := 0
    for name in DirAccess.get_files_at(output_path):
        if name.ends_with(".res"): saved_regions += 1
    if saved_regions != 16:
        fail("Incomplete output; scene must not be switched.")
        return
    var config := ConfigFile.new()
    config.load(CONFIG)
    config.save(output_path.path_join("LandscapeBake.cfg"))
    var manifest := {"source": source_path, "source_sha256": sources, "operation": "additive forest-floor enhancement v1", "configuration": "LandscapeBake.cfg"}
    var file := FileAccess.open(output_path.path_join("BaselineManifest.json"), FileAccess.WRITE)
    file.store_string(JSON.stringify(manifest, "  ") + "\n")
    print("Enhanced sixteen native regions into ", output_path, "; baseline untouched. Switch terrain.tscn data_directory explicitly after review.")
    terrain.free()
    result.free()
    quit()

# =========================================================
func enhanced_height(terrain: Terrain3D, land: RefCounted, x: float, z: float) -> float:
    return terrain.data.get_height(Vector3(clampf(x, -512.0, 511.0), 0.0, clampf(z, -512.0, 511.0))) + land.enhancement(x, z)
func coord(value: int) -> String:
    return ("_%02d" if value >= 0 else "-%02d") % absi(value)
func fail(message: String) -> void:
    push_error(message)
    quit(1)
