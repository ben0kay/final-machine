// Event-driven Sky3D/native haze boundary and one camera-owned distance-fog draw.
using Godot;
public partial class EnvironmentController : Node
{
    [Export] public EnvironmentSettings Defaults { get; set; }
    public EnvironmentSettings Settings { get; private set; }
    private WorldEnvironment _sky;
    private ShaderMaterial _material;
    private MeshInstance3D _fog;
    private bool _initialTimeApplied;
    // =========================================================
    // Called once after the world, Sky3D and camera have finished entering the tree.
    public void Initialize(WorldEnvironment sky, Camera3D camera)
    {
        _sky = sky;
        Settings = (EnvironmentSettings)Defaults.Duplicate(true);
        // Sky3D creates this Environment at runtime and retains its reference for sky updates.
        _material = new ShaderMaterial { Shader = GD.Load<Shader>("res://WORLD/Environment/Fog/DistanceFog.gdshader"), RenderPriority = 127 };
        _fog = new MeshInstance3D { Name = "DistanceFog", Mesh = new QuadMesh { Size = Vector2.One * 2 },
            Position = new Vector3(0, 0, -1), MaterialOverride = _material, ExtraCullMargin = 16384,
            IgnoreOcclusionCulling = true, CastShadow = GeometryInstance3D.ShadowCastingSetting.Off };
        camera.AddChild(_fog);
        Apply();
    }
    // =========================================================
    // Explicit time changes must not be undone by unrelated fog or day-length edits.
    public void SetHour(float hour)
    {
        Settings.Hour = Mathf.Clamp(hour, 0f, 23.9998f);
        _sky.Set("current_time", Settings.Hour);
    }
    // =========================================================
    // No polling: controls apply only when edited. Keep at least a metre between endpoints.
    public void Apply()
    {
        Settings.FullDistance = Mathf.Max(Settings.ClearDistance + 1, Settings.FullDistance);
        _fog.Visible = Settings.FogEnabled;
        _material.SetShaderParameter("clear_distance", Settings.ClearDistance);
        _material.SetShaderParameter("full_distance", Settings.FullDistance);
        _material.SetShaderParameter("fog_color", Settings.FogColor);
        _sky.Environment.FogEnabled = Settings.AtmosphericDensity > 0;
        _sky.Environment.FogDensity = Settings.AtmosphericDensity;
        _sky.Environment.FogLightColor = Settings.FogColor;
        // The authored hour is the initial hour; changing day length must not reset the running clock.
        _sky.Set("minutes_per_day", Mathf.Clamp(Settings.MinutesPerDay, 1f, 1440f));
        _sky.Set("game_time_enabled", true);
        if (!_initialTimeApplied) { _sky.Set("current_time", Mathf.Min(Settings.Hour, 23.9998f)); _initialTimeApplied = true; }
        _sky.Set("sun_energy", Settings.SunEnergy);
        _sky.GetNode<DirectionalLight3D>("SunLight").DirectionalShadowMaxDistance = Settings.ShadowDistance;
    }
}
