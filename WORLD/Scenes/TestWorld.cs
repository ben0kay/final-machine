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
        timer.Timeout += UpdateRenderStats;
        UpdateRenderStats();
        AddChild(timer);
        var environment = GetNode<EnvironmentController>("Environment/EnvironmentController");
        environment.Initialize(GetNode<WorldEnvironment>("Environment"), _player.GetNode<Camera3D>("Camera3D"));
        GetNode<EnvironmentDebugOverlay>("DebugUIRoot/EnvironmentDebugOverlay").Initialize(environment, _player);
    }
    // =========================================================
    // Sample engine render counters twice a second; no per-frame debug processing.
    // Godot's primitive monitor includes render passes and is not unique mesh triangles.
    private void UpdateRenderStats()
    {
        long primitives = (long)Godot.Performance.GetMonitor(Godot.Performance.Monitor.RenderTotalPrimitivesInFrame);
        _fps.Text = $"{Engine.GetFramesPerSecond()} FPS | {primitives:N0} prims";
    }

    // =========================================================
    // A real collision ray honors later sculpting; stop polling after successful placement.
    public override void _PhysicsProcess(double delta)
    {
        var query = PhysicsRayQueryParameters3D.Create(new Vector3(0, 350, 20), new Vector3(0, -100, 20), 1);
        var hit = GetWorld3D().DirectSpaceState.IntersectRay(query);
        if (hit.Count > 0)
        {
            AlignPreviewBuildingToTerrain();
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

    // =========================================================
    // Keep the preview structure's finished floor level with the saved Terrain3D surface.
    // Reuse the existing one-shot spawn readiness check; do not add an update loop.
    private void AlignPreviewBuildingToTerrain()
    {
        var building = GetNode<Node3D>("WorldActorsRoot/PersistentPropsRoot/Structures/SmallBuilding");
        var location = building.GlobalPosition;
        float groundY = GetNode("Terrain").Call("surface_height", location.X, location.Z).AsSingle();
        if (!float.IsFinite(groundY))
        {
            GD.PushWarning("SmallBuilding: terrain height unavailable; preview placement kept at its authored Y.");
            return;
        }
        building.GlobalPosition = new Vector3(location.X, groundY, location.Z);
    }
}
