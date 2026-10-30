// Capsule locomotion uses ordinary Godot physics; world composition owns safe placement.
using Godot;
public partial class PlayerController : CharacterBody3D
{
    [Export] public PlayerSettings Settings { get; set; }
    private Camera3D _camera;
    private float _pitch;
    private bool _jump;
    public override void _Ready()
    {
        Settings ??= new PlayerSettings();
        _camera = GetNode<Camera3D>("Camera3D");
        _camera.Fov = Settings.FieldOfView;
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }
    // =========================================================
    // Event-based view and jump input; Escape also clears pending jump intent.
    public override void _UnhandledInput(InputEvent e)
    {
        if (e is InputEventKey key && key.Pressed && key.Keycode == Key.Escape)
        { Input.MouseMode = Input.MouseModeEnum.Visible; _jump = false; }
        if (e is InputEventMouseButton button && button.Pressed)
        {
            if (button.ButtonIndex == MouseButton.Left) Input.MouseMode = Input.MouseModeEnum.Captured;
            if (button.ButtonIndex == MouseButton.Right && Input.MouseMode == Input.MouseModeEnum.Captured) _jump = true;
        }
        if (e is InputEventMouseMotion mouse && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            RotateY(-mouse.Relative.X * Settings.MouseSensitivity);
            _pitch = Mathf.Clamp(_pitch + mouse.Relative.Y * Settings.MouseSensitivity * (Settings.InvertMouseY ? 1 : -1), -1.48f, 1.48f);
            _camera.Rotation = new Vector3(_pitch, 0, 0);
        }
    }
    // =========================================================
    // Floor snapping follows downhill slopes; MoveAndSlide handles capsule contacts.
    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        bool captured = Input.MouseMode == Input.MouseModeEnum.Captured;
        Vector2 input = captured ? PlayerInput.Movement : Vector2.Zero;
        Vector3 target = GlobalBasis * new Vector3(input.X, 0, input.Y) * Settings.WalkSpeed * (captured && PlayerInput.Sprint ? Settings.SprintMultiplier : 1);
        Vector3 velocity = Velocity;
        velocity.X = Mathf.MoveToward(velocity.X, target.X, Settings.Acceleration * dt);
        velocity.Z = Mathf.MoveToward(velocity.Z, target.Z, Settings.Acceleration * dt);
        velocity.Y = IsOnFloor() ? (_jump ? Settings.JumpVelocity : 0) : velocity.Y - Settings.Gravity * dt;
        _jump = false;
        Velocity = velocity;
        MoveAndSlide();
    }
}
