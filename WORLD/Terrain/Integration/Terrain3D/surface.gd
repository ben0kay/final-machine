# Terrain3D 1.0.2 boundary: forest queries always use saved/editable native data.
@tool
extends Terrain3D
const Landforms = preload("res://WORLD/Terrain/Landscape/landforms.gd")
var composition = Landforms.new()
func surface_height(x: float,z: float) -> float:
    return data.get_height(Vector3(x,0,z))
func surface_up(x: float,z: float) -> float:
    return data.get_normal(Vector3(x,0,z)).y
func woodland(x: float,z: float) -> float:
    return composition.woodland(x,z)
