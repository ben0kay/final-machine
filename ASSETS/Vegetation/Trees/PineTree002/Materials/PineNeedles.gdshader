// Pine needle-only material: preserve imported atlas cutout and PBR lighting; share the forest palette.
shader_type spatial;
render_mode cull_disabled;

// The same global palette is read by ForestGrass001; 0 strength restores each source texture.
// Material-local uniforms avoid a project-global compile-time dependency.
// The F1 debug control updates the shared materials together, only on changes.
uniform vec4 forest_tint : source_color = vec4(0.43, 0.55, 0.37, 1.0);
uniform float forest_tint_strength : hint_range(0.0, 1.0) = 0.25;

uniform sampler2D needles_atlas : source_color, filter_linear_mipmap_anisotropic;
uniform float alpha_cutoff : hint_range(0.05, 0.95) = 0.29;
uniform float palette_response : hint_range(0.0, 1.0) = 0.85;
uniform float foliage_roughness : hint_range(0.5, 1.0) = 0.90;

void fragment() {
    vec4 needles = texture(needles_atlas, UV);
    vec3 original = needles.rgb * COLOR.rgb;
    // Harmonize hue while preserving the texture's light/dark needle detail.
    vec3 weights = vec3(0.2126, 0.7152, 0.0722);
    float original_luma = dot(original, weights);
    float palette_luma = max(dot(forest_tint.rgb, weights), 0.05);
    vec3 matched = clamp(forest_tint.rgb * (original_luma / palette_luma), vec3(0.0), vec3(1.0));
    ALBEDO = mix(original, matched, clamp(forest_tint_strength * palette_response, 0.0, 1.0));
    ALPHA = needles.a;
    ALPHA_SCISSOR_THRESHOLD = alpha_cutoff;
    ROUGHNESS = foliage_roughness;
    METALLIC = 0.0;
}
