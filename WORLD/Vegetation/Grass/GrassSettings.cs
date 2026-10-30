// Editable one-shot grass patch scatter, appearance, and visibility settings.
using Godot;

[GlobalClass]
public partial class GrassSettings : Resource
{
    [ExportGroup("Placement")]
    [Export] public int Seed { get; set; } = 41704;
    [Export(PropertyHint.Range, "0,40000,100")] public int PatchAttempts { get; set; } = 18000;
    [Export(PropertyHint.Range, "0,1,0.01")] public float PatchDensity { get; set; } = 0.65f;
    [Export(PropertyHint.Range, "0,1,0.01")] public float MinimumWoodland { get; set; } = 0.18f;
    [Export(PropertyHint.Range, "0,1,0.01")] public float NoiseThreshold { get; set; } = 0.38f;
    [Export(PropertyHint.Range, "2,12,0.5")] public float MinimumPatchSpacing { get; set; } = 7.0f;
    [Export(PropertyHint.Range, "1,6,0.1")] public float MinimumPatchRadius { get; set; } = 2.3f;
    [Export(PropertyHint.Range, "1,8,0.1")] public float MaximumPatchRadius { get; set; } = 5.4f;
    [Export(PropertyHint.Range, "1,60,1")] public int MinimumTuftsPerPatch { get; set; } = 18;
    [Export(PropertyHint.Range, "1,60,1")] public int MaximumTuftsPerPatch { get; set; } = 52;
    [Export(PropertyHint.Range, "0,1,0.01")] public float MinimumSurfaceUp { get; set; } = 0.94f;
    [Export] public float MaximumHeight { get; set; } = 115f;

    [Export(PropertyHint.Range, "0,0.4,0.01")] public float IsolatedPatchFraction { get; set; } = 0.14f;
    [Export(PropertyHint.Range, "0.3,1.5,0.05")] public float MinimumTuftSpacing { get; set; } = 0.65f;
    [Export(PropertyHint.Range, "0.5,2.5,0.1")] public float TrunkClearance { get; set; } = 1.1f;
    [Export(PropertyHint.Range, "1000,100000,1000")] public int MaximumTufts { get; set; } = 40000;

    [ExportGroup("Appearance")]
    [Export(PropertyHint.Range, "0.1,2,0.01")] public float MinimumScale { get; set; } = 0.48f;
    [Export(PropertyHint.Range, "0.1,2,0.01")] public float MaximumScale { get; set; } = 0.72f;
    [Export(PropertyHint.Range, "0,0.1,0.005")] public float TintVariation { get; set; } = 0.035f;

    [Export(PropertyHint.Range, "0.5,2,0.05")] public float MinimumWidth { get; set; } = 1.1f;
    [Export(PropertyHint.Range, "0.5,2,0.05")] public float MaximumWidth { get; set; } = 1.45f;
    [Export(PropertyHint.Range, "0,1,0.05")] public float NormalAlignment { get; set; } = 0.35f;

    [ExportGroup("Rendering")]
    [Export(PropertyHint.Range, "8,64,1")] public float BatchSize { get; set; } = 32f;
    [Export(PropertyHint.Range, "5,60,1")] public float NearDistance { get; set; } = 26f;
    [Export(PropertyHint.Range, "15,120,1")] public float CullDistance { get; set; } = 72f;
}
