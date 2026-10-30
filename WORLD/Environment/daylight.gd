# Static Sky3D 2.1 setup after its internal nodes exist; no time/weather simulation.
@tool
extends "res://addons/sky_3d/src/Sky3D.gd"
func _ready() -> void:
    environment.ambient_light_energy = 0.45
    environment.ssao_enabled = true
    environment.ssao_radius = 1.5
    environment.fog_enabled = true
    environment.fog_density = 0.00055
    environment.fog_light_color = Color(0.65,0.74,0.82)
