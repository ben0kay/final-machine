// Independent one-shot coverage, appearance and LOD controls for ForestGrass002 filler grass.
using Godot;

[GlobalClass]
public partial class ForestGrass002FillerSettings : Resource
{
    [ExportGroup("Enable")]
    [Export] public bool Enabled { get; set; } = true;
    [ExportGroup("Distribution")]
    [Export] public int Seed { get; set; } = 92641;
    [Export(PropertyHint.Range, "1,5,0.05")] public float GridSpacing { get; set; } = 1.95f;
    [Export(PropertyHint.Range, "0,1,0.01")] public float Coverage { get; set; } = 0.88f;
    [Export(PropertyHint.Range, "0,0.49,0.01")] public float Jitter { get; set; } = 0.44f;
    [Export(PropertyHint.Range, "0,1,0.01")] public float MinimumWoodland { get; set; } = 0.15f;
    [Export(PropertyHint.Range, "0,1,0.01")] public float MinimumSurfaceUp { get; set; } = 0.94f;
    [Export] public float MaximumHeight { get; set; } = 115f;
    [Export(PropertyHint.Range, "0.5,3,0.1")] public float TrunkClearance { get; set; } = 1.0f;
    [Export(PropertyHint.Range, "1000,100000,1000")] public int MaximumTufts { get; set; } = 65000;
    [ExportGroup("Appearance")]
    [Export(PropertyHint.Range, "0.2,1.5,0.05")] public float MinimumScale { get; set; } = 0.62f;
    [Export(PropertyHint.Range, "0.2,1.5,0.05")] public float MaximumScale { get; set; } = 1.05f;
    [Export(PropertyHint.Range, "0.5,3,0.05")] public float MinimumWidth { get; set; } = 1.50f;
    [Export(PropertyHint.Range, "0.5,3,0.05")] public float MaximumWidth { get; set; } = 2.10f;
    [Export(PropertyHint.Range, "0,0.15,0.005")] public float TintVariation { get; set; } = 0.04f;
    [Export(PropertyHint.Range, "0,1,0.05")] public float NormalAlignment { get; set; } = 0.30f;
    [ExportGroup("Rendering")]
    [Export(PropertyHint.Range, "16,64,1")] public float BatchSize { get; set; } = 40f;
    [Export(PropertyHint.Range, "5,40,1")] public float NearDistance { get; set; } = 22f;
    [Export(PropertyHint.Range, "25,90,1")] public float CullDistance { get; set; } = 56f;
}
