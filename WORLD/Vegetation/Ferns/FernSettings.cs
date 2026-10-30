// One-shot woodland fern distribution, size and short-range rendering controls.
using Godot;

[GlobalClass]
public partial class FernSettings : Resource
{
    [ExportGroup("Distribution")]
    [Export] public int Seed { get; set; } = 41733;
    [Export(PropertyHint.Range, "2,20,1")] public float GrassToFernRatio { get; set; } = 6f;
    [Export(PropertyHint.Range, "0.1,1,0.05")] public float FernPatchFraction { get; set; } = 0.62f;
    [Export(PropertyHint.Range, "1,15,1")] public int AttemptsPerFern { get; set; } = 10;
    [Export(PropertyHint.Range, "500,10000,100")] public int MaximumFerns { get; set; } = 7500;
    [Export(PropertyHint.Range, "1,10,0.25")] public float ScatterRadius { get; set; } = 4.75f;
    [Export(PropertyHint.Range, "0.5,4,0.1")] public float MinimumSpacing { get; set; } = 1.3f;
    [Export(PropertyHint.Range, "0,1,0.05")] public float MinimumWoodland { get; set; } = 0.32f;
    [Export(PropertyHint.Range, "0,1,0.01")] public float MinimumSurfaceUp { get; set; } = 0.91f;
    [Export(PropertyHint.Range, "0.5,3,0.1")] public float TrunkClearance { get; set; } = 0.95f;
    [Export] public float MaximumHeight { get; set; } = 115f;

    [ExportGroup("Appearance")]
    [Export(PropertyHint.Range, "0.5,3,0.05")] public float MinimumScale { get; set; } = 1.45f;
    [Export(PropertyHint.Range, "0.5,3,0.05")] public float MaximumScale { get; set; } = 2.25f;
    [Export(PropertyHint.Range, "0.5,2,0.05")] public float MinimumWidth { get; set; } = 0.88f;
    [Export(PropertyHint.Range, "0.5,2,0.05")] public float MaximumWidth { get; set; } = 1.18f;
    [Export(PropertyHint.Range, "0,0.1,0.005")] public float TintVariation { get; set; } = 0.04f;
    [Export(PropertyHint.Range, "0,1,0.05")] public float DetailedVariantFraction { get; set; } = 0.18f;

    [ExportGroup("Rendering")]
    [Export(PropertyHint.Range, "8,64,1")] public float BatchSize { get; set; } = 32f;
    [Export(PropertyHint.Range, "15,80,1")] public float CullDistance { get; set; } = 46f;
}
