# Pure, deterministic geography shared by the explicit bake and forest composition mask.
extends RefCounted
var seed_value: int = 41703
var plateau_height: float = 38.0
var mountain_height: float = 118.0
var noise := FastNoiseLite.new()
func _init() -> void:
    var config := ConfigFile.new()
    config.load("res://WORLD/Terrain/Configuration/LandscapeMaster.cfg")
    seed_value = config.get_value("landscape", "seed", 41703)
    plateau_height = config.get_value("landscape", "plateau_height", 38.0)
    mountain_height = config.get_value("landscape", "mountain_height", 118.0)
    noise.seed = seed_value
    noise.frequency = 0.014
    noise.fractal_octaves = 3
# =========================================================
func bell(x: float, z: float, cx: float, cz: float, rx: float, rz: float) -> float:
    return exp(-pow((x-cx)/rx, 2)-pow((z-cz)/rz, 2))
func sample(x: float, z: float) -> float:
    var n := noise.get_noise_2d(x,z)
    var meadow := 1.0-smoothstep(20.0,95.0,Vector2(x*0.9+n*5.0,z*1.2).length())
    var h := -3.0+8.0*bell(x,z,100,-100,130,180)+6.0*bell(x,z,-165,-65,150,155)
    # Uneven escarpment; the eastern shoulder rises slowly to meet it.
    var edge := (x+95.0)/90.0
    var plateau := smoothstep(-1.0,-0.83,edge+n*0.025)*(1.0-smoothstep(0.68,1.0,edge))
    plateau *= smoothstep(-235.0,-215.0,z+n*4.0)*(1.0-smoothstep(-105.0,-80.0,z+n*6.0))
    h = lerpf(h,plateau_height+sin(x/55.0),plateau)
    var shoulder := 35.0*bell(x,z,45,-105,125,140)
    var blend := clampf(0.5+0.5*(h-shoulder)/12.0,0,1)
    h = lerpf(shoulder,h,blend)+12.0*blend*(1.0-blend)
    h += mountain_height*(bell(x,z,-135,-350,145,85)+0.8*bell(x,z,100,-385,100,95))
    h += n*2.2*(1.0-meadow)
    return lerpf(h,0.18*sin(x/22.0),meadow)
# =========================================================
# Soft, irregular woodland edge and two small nonconcentric glades.
func woodland(x: float,z: float) -> float:
    var n := noise.get_noise_2d(x*1.7,z*1.7)
    var edge := smoothstep(26.0,64.0,Vector2(x*0.9,z*1.15).length()+n*22.0)
    var glades := maxf(bell(x,z,-98,-62,20,30),bell(x,z,120,-140,23,18))
    return clampf(edge*(0.8+n*0.45)*(1.0-glades),0,1)
