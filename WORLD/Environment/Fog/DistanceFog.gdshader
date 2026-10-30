// Camera-distance depth fog, with a continuous opaque horizon and clear upper sky.
shader_type spatial;
render_mode unshaded, fog_disabled, cull_disabled, depth_draw_never, depth_test_disabled;
uniform sampler2D depth_texture : hint_depth_texture, filter_nearest;
uniform float clear_distance = 60.0;
uniform float full_distance = 180.0;
uniform vec4 fog_color : source_color = vec4(0.57, 0.65, 0.72, 1.0);
void vertex() { POSITION = vec4(VERTEX.xy, 1.0, 1.0); }
void fragment() {
    float depth = texture(depth_texture, SCREEN_UV).r;
    vec3 ndc = vec3(SCREEN_UV * 2.0 - 1.0, depth);
    #if CURRENT_RENDERER == RENDERER_COMPATIBILITY
    ndc.z = depth * 2.0 - 1.0;
    #endif
    vec4 view = INV_PROJECTION_MATRIX * vec4(ndc, 1.0);
    vec3 position_view = view.xyz / view.w;
    vec3 ray = normalize((INV_VIEW_MATRIX * vec4(position_view, 0.0)).xyz);
    float amount = smoothstep(clear_distance, full_distance, length(position_view));
    // At the horizon the sky and fully fogged geometry share the exact same tint.
    // Gradually reveal the original Sky3D sky only above the horizon.
    #if CURRENT_RENDERER == RENDERER_COMPATIBILITY
    bool sky_pixel = depth >= 0.999999;
    #else
    bool sky_pixel = depth <= 0.000001;
    #endif
    if (sky_pixel) amount = 1.0 - smoothstep(0.15, 0.85, ray.y);
    ALBEDO = fog_color.rgb;
    ALPHA = amount;
}
