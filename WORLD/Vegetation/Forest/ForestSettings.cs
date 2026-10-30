// Shared forest density, batch size and authored LOD distances.
using Godot;
[GlobalClass]
public partial class ForestSettings : Resource
{
    [Export] public int Seed { get; set; } = 41703;
    [Export] public int Attempts { get; set; } = 18000;
    [Export] public float MinimumSpacing { get; set; } = 5.5f;
    [Export] public float BatchSize { get; set; } = 24f;
    [Export] public float NearDistance { get; set; } = 70f;
    [Export] public float MiddleDistance { get; set; } = 280f;
    [Export] public float CullDistance { get; set; } = 600f;
}
