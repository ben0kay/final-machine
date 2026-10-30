// Defaults for the installed Terrain3D GPU-particle grass trial; source grass presets remain intact.
using Godot;
[GlobalClass]
public partial class DemoGrassSettings : Resource
{
    [ExportGroup("Distribution")]
    [Export] public bool Enabled { get; set; } = true;
    [Export(PropertyHint.Range, "0.125,1,0.015625")] public float Spacing { get; set; } = 0.3125f;
    [Export(PropertyHint.Range, "8,48,1")] public float CellWidth { get; set; } = 24f;
    [Export(PropertyHint.Range, "3,7,2")] public int GridWidth { get; set; } = 5;
    [ExportGroup("Blades")]
    [Export] public float MinimumHeight { get; set; } = 0.40f;
    [Export] public float MaximumHeight { get; set; } = 0.70f;
    [Export] public float MinimumWidth { get; set; } = 0.085f;
    [Export] public float MaximumWidth { get; set; } = 0.13f;
    [Export] public float WindStrength { get; set; } = 0.32f;
    [Export] public Color RootColor { get; set; } = new(0.22f, 0.30f, 0.12f);
    [Export] public Color TipColor { get; set; } = new(0.45f, 0.56f, 0.25f);
}
