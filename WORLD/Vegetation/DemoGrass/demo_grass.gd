# Thin game-owned boundary around Terrain3D 1.0.2's actual demo GPU grass grid.
extends "res://addons/terrain_3d/extras/particle_example/terrain_3D_particles.gd"
@export var settings: Resource
@export var terrain_path: NodePath
var maps_bound := false

# =========================================================
# Private material copies; no editor preset or native region changes.
func _ready() -> void:
    if settings == null or not settings.get("Enabled"):
        set_physics_process(false)
        return
    var native_terrain := get_node_or_null(terrain_path) as Terrain3D
    if native_terrain == null:
        push_error("DemoGrass: Terrain3D binding missing.")
        set_physics_process(false)
        return
    process_material = load("res://addons/terrain_3d/extras/particle_example/process_material.tres").duplicate(true)
    process_material.shader = load("res://WORLD/Vegetation/DemoGrass/Materials/forest_particles.gdshader")
    mesh_material_override = ShaderMaterial.new()
    mesh_material_override.shader = load("res://WORLD/Vegetation/DemoGrass/Materials/forest_blade.gdshader")
    instance_spacing = settings.get("Spacing")
    cell_width = settings.get("CellWidth")
    grid_width = settings.get("GridWidth")
    process_material.set_shader_parameter("min_scale", Vector3(settings.get("MinimumWidth"), settings.get("MinimumHeight"), settings.get("MinimumWidth")))
    process_material.set_shader_parameter("max_scale", Vector3(settings.get("MaximumWidth"), settings.get("MaximumHeight"), settings.get("MaximumWidth")))
    process_material.set_shader_parameter("clod_scale_boost", 0.08)
    process_material.set_shader_parameter("clod_min_threshold", 0.25)
    process_material.set_shader_parameter("clod_max_threshold", 0.8)
    process_material.set_shader_parameter("patch_min_threshold", 0.015)
    process_material.set_shader_parameter("patch_max_threshold", 0.12)
    process_material.set_shader_parameter("random_spacing", 0.8)
    process_material.set_shader_parameter("wind_strength", settings.get("WindStrength"))
    process_material.set_shader_parameter("surface_slope_min", 0.94)
    process_material.set_shader_parameter("condition_dither_range", 0.03)
    process_material.set_shader_parameter("stream_filter_enabled", FileAccess.file_exists(native_terrain.data_directory.path_join("LandscapeBake.cfg")))
    mesh_material_override.set_shader_parameter("root_color", settings.get("RootColor"))
    mesh_material_override.set_shader_parameter("tip_color", settings.get("TipColor"))
    # This setter builds the inherited 5x5 emitter grid, exactly as in the installed demo.
    terrain = native_terrain
    _update_process_parameters()
    print("DemoGrass: ", particle_count, " GPU blade slots; legacy grass meshes disabled in test scene.")

# =========================================================
# The native helper calls this each physics tick. Static native bindings need uploading only once.
func _update_process_parameters() -> void:
    if maps_bound: return
    if terrain == null or terrain.data == null or process_material == null: return
    super._update_process_parameters()
    maps_bound = true

# =========================================================
# Correct conservative vertical bounds for the full saved height range, plus blade/wind extent.
func _create_grid() -> void:
    super._create_grid()
    if terrain == null or terrain.data == null: return
    var heights := terrain.data.get_height_range()
    for emitter in particle_nodes:
        emitter.custom_aabb = AABB(Vector3(-cell_width * 0.5 - 2.0, heights.x - 2.0, -cell_width * 0.5 - 2.0), Vector3(cell_width + 4.0, heights.y - heights.x + 6.0, cell_width + 4.0))
