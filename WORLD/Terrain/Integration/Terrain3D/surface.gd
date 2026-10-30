# Terrain3D 1.0.2 boundary: forest queries always use saved/editable native data.
@tool
extends Terrain3D
const Landforms = preload("res://WORLD/Terrain/Landscape/landforms.gd")
var composition: RefCounted
var enhanced_habitat := false
# =========================================================
# Use the bake snapshot, not subsequently edited authoring defaults, for channel ecology.
func ensure_composition() -> void:
    if composition != null: return
    var snapshot := data_directory.path_join("LandscapeBake.cfg")
    enhanced_habitat = FileAccess.file_exists(snapshot)
    composition = Landforms.new(snapshot) if enhanced_habitat else Landforms.new()
func surface_height(x: float,z: float) -> float:
    return data.get_height(Vector3(x,0,z))
func surface_up(x: float,z: float) -> float:
    return data.get_normal(Vector3(x,0,z)).y
func woodland(x: float,z: float) -> float:
    ensure_composition()
    return composition.woodland(x,z)

# =========================================================
# One placement-only query: height, up, habitat probability and bank enrichment.
# Native paint/hole checks keep tufts off exposed rock even after manual terrain painting.
func grass_sample(x: float, z: float) -> Vector4:
    ensure_composition()
    var position := Vector3(x, 0.0, z)
    var height := data.get_height(position)
    var normal := data.get_normal(position)
    if not is_finite(height) or data.get_control_hole(position): return Vector4(NAN, 0.0, 0.0, 0.0)
    var blend := data.get_control_blend(position)
    var rock := (1.0 - blend if data.get_control_base_id(position) == 2 else 0.0)
    rock += blend if data.get_control_overlay_id(position) == 2 else 0.0
    var habitat := 1.0 - smoothstep(0.12, 0.5, rock)
    var bank := 0.0
    if enhanced_habitat:
        var stream: Vector2 = composition.stream_profile(x, z)
        habitat *= lerpf(1.0, lerpf(0.06, 1.0, smoothstep(0.2, 0.95, stream.x)), stream.y)
        bank = (1.0 - smoothstep(0.35, 1.3, absf(stream.x - 1.05))) * stream.y
    return Vector4(height, normal.y, habitat, bank)

func surface_normal(x: float, z: float) -> Vector3:
    return data.get_normal(Vector3(x, 0.0, z))
