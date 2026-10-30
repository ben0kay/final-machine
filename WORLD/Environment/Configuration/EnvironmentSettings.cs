// Authored FM1 defaults; the environment owner duplicates this resource for live tuning.
using Godot;
[GlobalClass]
public partial class EnvironmentSettings : Resource
{
    [ExportGroup("Distance fog")]
    [Export] public bool FogEnabled { get; set; } = true;
    [Export] public float ClearDistance { get; set; } = 60;
    [Export] public float FullDistance { get; set; } = 180;
    [Export] public Color FogColor { get; set; } = new(0.57f, 0.65f, 0.72f);
    [Export] public float AtmosphericDensity { get; set; } = 0.008f;
    [ExportGroup("Lighting")]
    [Export] public float Hour { get; set; } = 13.5f;
    [Export] public float MinutesPerDay { get; set; } = 60f;
    [Export] public float SunEnergy { get; set; } = 1.2f;
    [Export] public float ShadowDistance { get; set; } = 100;
}
