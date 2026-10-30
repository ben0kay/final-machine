// Reusable first-person movement and view tuning; contains no terrain integration.
using Godot;
[GlobalClass]
public partial class PlayerSettings : Resource
{
    [ExportGroup("Movement")]
    [Export] public float WalkSpeed { get; set; } = 5f;
    [Export] public float SprintMultiplier { get; set; } = 1.6f;
    [Export] public float Acceleration { get; set; } = 24f;
    [Export] public float Gravity { get; set; } = 20f;
    [Export] public float JumpVelocity { get; set; } = 6f;
    [ExportGroup("View")]
    [Export] public float MouseSensitivity { get; set; } = 0.0017f;
    [Export] public bool InvertMouseY { get; set; } = true;
    [Export] public float FieldOfView { get; set; } = 75f;
}
