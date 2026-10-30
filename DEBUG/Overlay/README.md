# FM1 environment debug overlay

`environment_debug_overlay.tscn` is instanced once under the test world's `DebugUIRoot` CanvasLayer. The existing `HUD/FPS` remains independent and visible while the panel is hidden. TestWorld explicitly binds `EnvironmentController` and the player after their initialization; no global searches or recurring debug loops.

F1 opens/closes the panel; Escape closes it. Opening records capture state, releases the mouse and gates player look/movement/jump/recapture, including world clicks outside the panel. Closing restores capture only if it was captured before opening. Pending jump intent is cleared. GUI events remain available to controls and colour-picker popups. Player gravity/physics continue normally.

Scrollable, independently collapsible sections:

- **Environment / fog:** distance fog toggle, clear 5–150m, full 20–600m, atmospheric density 0–0.03, colour picker.
- **Time / lighting:** Sky3D time 00:00–24:00 (24:00 maps to the last instant before midnight), **day length 15–240 real minutes** (60-minute default), sunlight energy 0–4, shadow distance 10–300m.
- **Forest foliage / pine + grass:** shared colour picker and strength slider (0 = no global tint) for PineTree002 needles, ForestGrass001 and ForestGrass002 filler. Bark and ferns are unchanged; a comparison button resets tint strength to 0.

Numeric labels update with every slider signal. Crossing distance endpoints adjusts the other endpoint and its displayed slider value. Copy Settings copies a JSON snapshot of every exposed effective runtime value, including RGBA fog colour and shared forest foliage colour/strength. No disk writes or editor-default mutations occur. Future controls can reuse the section/slider shell and bind explicit feature owners; no future tabs are built now.

See `WORLD/Environment/README.md` for configuration, rendering limitations and validation. Compilation/headless F1, cursor, slider and clock/sun checks passed; interactive layout, clipboard and rendered visuals require local Godot testing.

The shared foliage parameters are declared in `project.godot` [shader_globals] and can also be edited in Project Settings → Shader Globals. The F1 controls call `RenderingServer.GlobalShaderParameterSet` only when changed. Current defaults: forest tint Color(0.43, 0.55, 0.37), tint strength 0.25, local per-material response 0.85. F1 changes are runtime-only; adjust project defaults to persist an accepted colour. New controls require local Godot visual and interaction verification.
