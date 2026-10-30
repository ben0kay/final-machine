// Scene composition waits for physical ground before enabling the reusable player.
using Godot;
public partial class TestWorld : Node3D
{
    private Label _fps;
    private PlayerController _player;
    private int _attempts;
    public override void _Ready()
    {
        _player = GetNode<PlayerController>("Player");
        _player.SetPhysicsProcess(false);
        _fps = GetNode<Label>("HUD/FPS");
        var timer = new Timer { WaitTime = 0.5, Autostart = true };
        timer.Timeout += () => _fps.Text = $"{Engine.GetFramesPerSecond()} FPS";
        AddChild(timer);
        var environment = GetNode<EnvironmentController>("Environment/EnvironmentController");
        environment.Initialize(GetNode<WorldEnvironment>("Environment"), _player.GetNode<Camera3D>("Camera3D"));
        GetNode<EnvironmentDebugOverlay>("DebugUIRoot/EnvironmentDebugOverlay").Initialize(environment, _player);
    }
    // =========================================================
    // A real collision ray honors later sculpting; stop polling after successful placement.
    public override void _PhysicsProcess(double delta)
    {
        var query = PhysicsRayQueryParameters3D.Create(new Vector3(0, 350, 20), new Vector3(0, -100, 20), 1);
        var hit = GetWorld3D().DirectSpaceState.IntersectRay(query);
        if (hit.Count > 0)
        {
            _player.GlobalPosition = hit["position"].AsVector3() + Vector3.Up * 0.15f;
            _player.SetPhysicsProcess(true);
            SetPhysicsProcess(false);
        }
        else if (++_attempts > 600)
        {
            GD.PushError("FM0: no terrain collision at spawn. Player remains disabled; inspect Terrain3D imports/collision.");
            _fps.Text = "Terrain collision unavailable";
            SetPhysicsProcess(false);
        }
    }
}
