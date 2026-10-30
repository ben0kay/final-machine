shader_type spatial;
render_mode cull_disabled;

// Shared lightweight cutout and wind for the original Ferns001 atlas and PBR maps.
// MultiMesh instance COLOR supplies subtle per-plant colour variation.
uniform sampler2D plant_color : source_color, filter_linear_mipmap_anisotropic;
uniform sampler2D plant_normal : hint_normal, filter_linear_mipmap_anisotropic;
uniform sampler2D plant_roughness : hint_default_white, filter_linear_mipmap_anisotropic;
uniform float alpha_cutoff : hint_range(0.1, 0.9) = 0.42;
uniform float wind_response : hint_range(0.0, 2.0) = 0.7;
uniform float wind_plant_height : hint_range(0.1, 2.0) = 0.35;
uniform float wind_speed : hint_range(0.0, 4.0) = 1.2;
uniform float fade_start = 31.0;
uniform float fade_end = 44.0;

varying float distance_presence;

void vertex() {
    vec3 root_world = (MODEL_MATRIX * vec4(0.0, 0.0, 0.0, 1.0)).xyz;
    float d = distance(root_world.xz, CAMERA_POSITION_WORLD.xz);
    distance_presence = 1.0 - smoothstep(fade_start, fade_end, d);
    float anchoring = smoothstep(0.025, max(0.1, wind_plant_height), max(0.0, VERTEX.y));
    float wave = sin(TIME * wind_speed + dot(root_world.xz, vec2(0.23, 0.59)))
        + 0.24 * sin(TIME * wind_speed * 0.67 + root_world.x * 0.57);
    VERTEX.xz += vec2(0.86, 0.51) * wave * 0.024 * wind_response * anchoring * anchoring;
    VERTEX.y *= distance_presence;
}

void fragment() {
    if (distance_presence < 0.001) discard;
    vec4 albedo = texture(plant_color, UV);
    ALBEDO = albedo.rgb * COLOR.rgb;
    ALPHA = albedo.a;
    ALPHA_SCISSOR_THRESHOLD = alpha_cutoff;
    NORMAL_MAP = texture(plant_normal, UV).rgb;
    NORMAL_MAP_DEPTH = 0.7;
    ROUGHNESS = texture(plant_roughness, UV).r;
    METALLIC = 0.0;
    BACKLIGHT = ALBEDO * 0.07;
}
