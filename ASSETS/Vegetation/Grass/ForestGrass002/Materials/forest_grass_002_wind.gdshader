// ForestGrass002: irregular multi-plane foliage cards with alpha-faded roots and light wind.
// Static foliage mesh authored in the GLTF; the runtime scatters entire tufts, not individual cards.
shader_type spatial;
render_mode cull_disabled, alpha_to_coverage;
global uniform vec4 forest_tint;
global uniform float forest_tint_strength;
uniform sampler2D grass_atlas : source_color, filter_linear_mipmap_anisotropic, repeat_disable;
uniform float alpha_cutoff : hint_range(0.1, 0.8) = 0.40;
uniform vec3 foliage_tint : source_color = vec3(1.0);
uniform float palette_response : hint_range(0.0, 1.0) = 0.85;
uniform float backlight_strength : hint_range(0.0, 0.3) = 0.11;
uniform float wind_strength : hint_range(0.0, 0.12) = 0.025;
uniform float wind_speed : hint_range(0.0, 4.0) = 1.0;
uniform vec2 wind_direction = vec2(0.82, 0.57);
uniform bool distance_fade_enabled = true;
uniform float sparse_distance = 44.0;
uniform float end_distance = 68.0;
varying flat float tuft_presence;
void vertex() {
    vec3 root = (MODEL_MATRIX * vec4(0.0, 0.0, 0.0, 1.0)).xyz;
    float d = distance(root.xz, CAMERA_POSITION_WORLD.xz);
    tuft_presence = distance_fade_enabled ? 1.0 - smoothstep(sparse_distance, end_distance, d) : 1.0;
    float flex = smoothstep(0.035, 0.67, VERTEX.y);
    float phase = dot(root.xz, vec2(0.21, 0.69));
    float wave = sin(TIME * wind_speed + phase + VERTEX.y * 1.7);
    VERTEX.xz += normalize(wind_direction) * wave * wind_strength * flex * flex;
}
void fragment() {
    vec4 texel = texture(grass_atlas, UV);
    vec3 original = texel.rgb * COLOR.rgb * foliage_tint;
    vec3 weights = vec3(0.2126, 0.7152, 0.0722);
    float original_luma = dot(original, weights);
    float palette_luma = max(dot(forest_tint.rgb, weights), 0.05);
    vec3 matched = clamp(forest_tint.rgb * (original_luma / palette_luma), vec3(0.0), vec3(1.0));
    ALBEDO = mix(original, matched, clamp(forest_tint_strength * palette_response, 0.0, 1.0));
    ALPHA = texel.a * tuft_presence;
    ALPHA_SCISSOR_THRESHOLD = alpha_cutoff;
    ALPHA_ANTIALIASING_EDGE = 0.22;
    ALPHA_TEXTURE_COORDINATE = UV * vec2(1024.0);
    ROUGHNESS = 0.95;
    SPECULAR = 0.2;
    METALLIC = 0.0;
    BACKLIGHT = ALBEDO * backlight_strength;
}
