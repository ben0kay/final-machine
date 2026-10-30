// Test 2 only: place the reusable player on its own editable Terrain3D ground.
using Godot;

public partial class Test2World : Node3D
{
    private PlayerController _player;
    private int _attempts;

    // =========================================================
    // Pause player movement until the native terrain collision becomes available.
    public override void _Ready()
    {
        _player = GetNode<PlayerController>("WorldActorsRoot/PersistentEntitiesRoot/Player");
        _player.SetPhysicsProcess(false);
    }

    // =========================================================
    // One-time downward collision query, then disable this probe permanently.
    public override void _PhysicsProcess(double delta)
    {
        var query = PhysicsRayQueryParameters3D.Create(
            new Vector3(0f, 350f, 20f), new Vector3(0f, -100f, 20f), 1);
        var hit = GetWorld3D().DirectSpaceState.IntersectRay(query);
        if (hit.Count > 0)
        {
            _player.GlobalPosition = hit["position"].AsVector3() + Vector3.Up * 0.15f;
            _player.SetPhysicsProcess(true);
            SetPhysicsProcess(false);
        }
        else if (++_attempts > 600)
        {
            GD.PushError("Test2: terrain collision unavailable at spawn; check Terrain3D region imports.");
            SetPhysicsProcess(false);
        }
    }
}
