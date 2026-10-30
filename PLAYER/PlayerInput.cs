// Local controls deliberately use physical keys, independent of project InputMap changes.
using Godot;
public static class PlayerInput
{
    public static Vector2 Movement => new Vector2(
        (Input.IsPhysicalKeyPressed(Key.Right) ? 1 : 0) - (Input.IsPhysicalKeyPressed(Key.Left) ? 1 : 0),
        (Input.IsPhysicalKeyPressed(Key.Down) ? 1 : 0) - (Input.IsPhysicalKeyPressed(Key.Up) ? 1 : 0)).LimitLength();
    public static bool Sprint => Input.IsPhysicalKeyPressed(Key.Shift);
}
