# Sky3D initialization only; EnvironmentController owns runtime fog/lighting defaults.
@tool
extends "res://addons/sky_3d/src/Sky3D.gd"
func _ready() -> void:
    environment.ambient_light_energy = 0.45
    environment.ssao_enabled = true
    environment.ssao_radius = 1.5
