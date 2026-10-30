# Pure, deterministic geography shared by the explicit bake and forest composition mask.
extends RefCounted
var seed_value: int = 41703
var plateau_height: float = 38.0
var mountain_height: float = 118.0
var cliff_height: float = 106.0
var detail_strength: float = 1.0
var stream_depth: float = 1.65
var noise := FastNoiseLite.new()
func _init(config_path: String = "res://WORLD/Terrain/Configuration/LandscapeMaster.cfg") -> void:
    var config := ConfigFile.new()
    config.load(config_path)
    seed_value = config.get_value("landscape", "seed", 41703)
    plateau_height = config.get_value("landscape", "plateau_height", 38.0)
    mountain_height = config.get_value("landscape", "mountain_height", 118.0)
    cliff_height = config.get_value("enhancement", "cliff_height", 106.0)
    detail_strength = config.get_value("enhancement", "detail_strength", 1.0)
    stream_depth = config.get_value("enhancement", "stream_depth", 1.65)
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

# =========================================================
# Signed additive authoring pass; base sample/woodland remain unchanged for compatibility.
# West-facing layered scarp, broken along its length with broad gullies, not octave hill noise.
func enhancement(x: float, z: float) -> float:
    var protected_meadow := smoothstep(48.0, 105.0, Vector2(x * 0.9, z * 1.15).length())
    var north_south := smoothstep(-330.0, -260.0, z) * (1.0 - smoothstep(-105.0, -55.0, z))
    var edge := -188.0 + 13.0 * sin(z / 43.0) + 6.0 * sin(z / 17.0 + 0.9)
    var across := x - edge
    var length_breaks := 0.92 + 0.08 * sin(z / 31.0 + 0.4)
    var face := 0.53 * smoothstep(-12.0, 3.0, across)
    face += 0.28 * smoothstep(7.0, 17.0, across)
    face += 0.19 * smoothstep(23.0, 32.0, across)
    var back_slope := 1.0 - smoothstep(48.0, 145.0, across)
    # Narrow recesses and uneven bedding make connected ledges and interrupted ridgelines.
    var gullies := pow(maxf(0.0, sin(z / 18.0 + sin(z / 49.0))), 8.0)
    var cliff := (cliff_height * length_breaks * face - 11.0 * gullies * face) * back_slope * north_south
    var footing := 7.0 * bell(x, z, -225.0, -185.0, 42.0, 104.0)
    var relief := (0.65 * sin((x + z * 0.32) / 23.0) + 0.42 * sin((z - x * 0.2) / 36.0))
    relief += 1.3 * bell(x, z, 135.0, -65.0, 28.0, 68.0) - 1.1 * bell(x, z, -90.0, 110.0, 35.0, 46.0)
    var edge_fade := 1.0 - smoothstep(430.0, 490.0, maxf(absf(x), absf(z)))
    var stream := stream_profile(x, z)
    var channel := stream_depth * (0.88 + 0.12 * sin(z / 28.0))
    channel *= (1.0 - smoothstep(0.08, 1.0, stream.x)) * stream.y
    return ((cliff + footing + relief * detail_strength) * protected_meadow - channel) * edge_fade

# =========================================================
# Normalized cross-channel distance and longitudinal envelope, shared with habitat placement.
# Centreline runs north-to-south through the eastern woodland, away from the spawn clearing.
func stream_profile(x: float, z: float) -> Vector2:
    var center := 96.0 + 22.0 * sin(z / 48.0) + 7.0 * sin(z / 21.0 + 0.7)
    var derivative := (22.0 / 48.0) * cos(z / 48.0) + (7.0 / 21.0) * cos(z / 21.0 + 0.7)
    var half_width := 2.5 + 0.55 * sin(z / 37.0) + 0.3 * sin(z / 14.0 + 0.6)
    var distance := absf(x - center) / sqrt(1.0 + derivative * derivative)
    var envelope := smoothstep(-165.0, -140.0, z) * (1.0 - smoothstep(115.0, 140.0, z))
    return Vector2(distance / half_width, envelope)
