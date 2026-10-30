# Focused native data/import/collision check; no visual or C# claims.
extends SceneTree
var terrain: Terrain3D
var ticks := 0
func _initialize() -> void:
    terrain = load("res://WORLD/Terrain/Integration/Terrain3D/terrain.tscn").instantiate()
    terrain.free_editor_textures = false
    root.add_child(terrain)
    root.add_child(load("res://WORLD/Environment/environment.tscn").instantiate())
    var camera := Camera3D.new()
    root.add_child(camera)
    camera.current = true
    call_deferred("check_data")
func check_data() -> void:
    var pines := 0
    for lod in range(1,4):
        var scene = load("res://ASSETS/Vegetation/Trees/PineTree002/Models/pine_tree_002_a_LOD%d.gltf" % lod)
        assert(scene != null)
        var model = scene.instantiate()
        model.free()
        pines += 1
    assert(terrain.assets.get_texture_count() == 3)
    assert(terrain.data.get_regions_active().size() == 16)
    assert(is_finite(terrain.data.get_height(Vector3(0,0,20))))
    print("FM0 native: 16 regions, 3 material sets, %d imported tree LODs, saved height %f" % [pines,terrain.data.get_height(Vector3(0,0,20))])
func _physics_process(_delta: float) -> bool:
    ticks += 1
    if ticks < 5: return false
    var query := PhysicsRayQueryParameters3D.create(Vector3(0,350,20),Vector3(0,-100,20),1)
    var hit := root.world_3d.direct_space_state.intersect_ray(query)
    assert(not hit.is_empty(), "No terrain collision at spawn")
    print("FM0 native: collision ray hit at ",hit.position)
    quit()
    return false
