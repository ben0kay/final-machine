// Reusable event-driven debug shell; explicit owner binding keeps future sections independent.
using Godot;
using System;
using System.Text.Json;
public partial class EnvironmentDebugOverlay : Control
{
    private EnvironmentController _environment;
    private PlayerController _player;
    private PanelContainer _panel;
    private bool _restoreCapture;
    private HSlider _clear, _full;
    // =========================================================
    // The FPS label remains a separate HUD sibling, visible with this panel closed.
    public void Initialize(EnvironmentController environment, PlayerController player)
    {
        _environment = environment; _player = player;
        MouseFilter = MouseFilterEnum.Ignore;
        _panel = new PanelContainer { Position = new Vector2(16, 80), Size = new Vector2(410, 510), Visible = false };
        AddChild(_panel);
        var scroll = new ScrollContainer { CustomMinimumSize = new Vector2(410, 510), HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled };
        _panel.AddChild(scroll);
        var content = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        scroll.AddChild(content);
        content.AddChild(new Label { Text = "FM1 • F1 / Esc to close" });
        var fog = AddSection(content, "Environment / fog");
        var settings = environment.Settings;
        var enabled = new CheckBox { Text = "Distance fog enabled", ButtonPressed = settings.FogEnabled };
        enabled.Toggled += value => { settings.FogEnabled = value; environment.Apply(); };
        fog.AddChild(enabled);
        _clear = AddSlider(fog, "Clear distance (m)", 5, 150, 1, settings.ClearDistance, value =>
        {
            settings.ClearDistance = value;
            environment.Apply();
            _full.Value = settings.FullDistance;
        });
        _full = AddSlider(fog, "Full fog distance (m)", 20, 600, 1, settings.FullDistance, value =>
        {
            settings.FullDistance = value;
            if (settings.FullDistance <= settings.ClearDistance)
            { settings.ClearDistance = settings.FullDistance - 1; _clear.Value = settings.ClearDistance; }
            environment.Apply();
        });
        AddSlider(fog, "Atmospheric density", 0, 0.03, 0.0001, settings.AtmosphericDensity, value => { settings.AtmosphericDensity = value; environment.Apply(); }, "0.0000");
        var color = new ColorPickerButton { Text = "Fog colour", Color = settings.FogColor, EditAlpha = false };
        color.ColorChanged += value => { settings.FogColor = value; environment.Apply(); };
        fog.AddChild(color);
        var light = AddSection(content, "Time / lighting");
        AddSlider(light, "Time of day", 0, 24, 0.05, settings.Hour, value => { settings.Hour = value; environment.Apply(); }, "clock");
        AddSlider(light, "Sunlight energy", 0, 4, 0.05, settings.SunEnergy, value => { settings.SunEnergy = value; environment.Apply(); }, "0.00");
        AddSlider(light, "Shadow distance (m)", 10, 300, 1, settings.ShadowDistance, value => { settings.ShadowDistance = value; environment.Apply(); });
        var copy = new Button { Text = "Copy Settings" };
        copy.Pressed += () => DisplayServer.ClipboardSet(JsonSerializer.Serialize(new {
            settings.FogEnabled, settings.ClearDistance, settings.FullDistance, settings.AtmosphericDensity,
            FogColor = new[] { settings.FogColor.R, settings.FogColor.G, settings.FogColor.B, 1f },
            settings.Hour, settings.SunEnergy, settings.ShadowDistance
        }, new JsonSerializerOptions { WriteIndented = true }));
        content.AddChild(copy);
    }
    // =========================================================
    // Section shell can host other explicitly bound owners later; no speculative tabs.
    private static VBoxContainer AddSection(VBoxContainer parent, string title)
    {
        var heading = new Button { Text = "▼ " + title, ToggleMode = true, ButtonPressed = true };
        var body = new VBoxContainer();
        parent.AddChild(heading); parent.AddChild(body);
        heading.Toggled += open => { body.Visible = open; heading.Text = (open ? "▼ " : "▶ ") + title; };
        return body;
    }
    // =========================================================
    private static HSlider AddSlider(VBoxContainer parent, string title, double min, double max, double step,
        float initial, Action<float> apply, string format = "0")
    {
        string Display(double value) => format == "clock"
            ? $"{(int)value:00}:{(int)Math.Round((value % 1) * 60):00}"
            : value.ToString(format);
        var label = new Label { Text = $"{title}: {Display(initial)}" };
        var slider = new HSlider { MinValue = min, MaxValue = max, Step = step, Value = initial, CustomMinimumSize = new Vector2(370, 24) };
        parent.AddChild(label); parent.AddChild(slider);
        slider.ValueChanged += value => { label.Text = $"{title}: {Display(value)}"; apply((float)value); };
        return slider;
    }
    // =========================================================
    // Input-stage gate protects world clicks even outside the panel and clears queued jumps.
    public override void _Input(InputEvent e)
    {
        if (_panel == null) return;
        if (e is InputEventKey key && key.Pressed && !key.Echo &&
            (key.Keycode == Key.F1 || (_panel.Visible && key.Keycode == Key.Escape)))
        {
            bool open = !_panel.Visible;
            if (open) _restoreCapture = Input.MouseMode == Input.MouseModeEnum.Captured;
            _panel.Visible = open;
            _player.SetDebugInputBlocked(open);
            Input.MouseMode = open || !_restoreCapture ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
            if (!open) GetViewport().GuiReleaseFocus();
            GetViewport().SetInputAsHandled();
        }
    }
    // =========================================================
    public override void _ExitTree()
    {
        if (IsInstanceValid(_player)) _player.SetDebugInputBlocked(false);
    }
}
