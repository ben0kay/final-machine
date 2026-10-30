# FM1 environment debug overlay

`environment_debug_overlay.tscn` is instanced once under the test world's `DebugUIRoot` CanvasLayer. The existing `HUD/FPS` remains independent and visible while the panel is hidden. TestWorld explicitly binds `EnvironmentController` and the player after their initialization; no global searches or recurring debug loops.

F1 opens/closes the panel; Escape closes it. Opening records capture state, releases the mouse and gates player look/movement/jump/recapture, including world clicks outside the panel. Closing restores capture only if it was captured before opening. Pending jump intent is cleared. GUI events remain available to controls and colour-picker popups. Player gravity/physics continue normally.

Scrollable, independently collapsible sections:

- **Environment / fog:** distance fog toggle, clear 5–150m, full 20–600m, atmospheric density 0–0.03, colour picker.
- **Time / lighting:** Sky3D time 00:00–24:00 (24:00 maps to the last instant before midnight), **day length 15–240 real minutes** (60-minute default), sunlight energy 0–4, shadow distance 10–300m.
- **Forest foliage / pine + grass:** shared colour picker and strength slider (0 = original foliage colour) for PineTree002 needles, ForestGrass001 and ForestGrass002 filler. Bark and ferns are unchanged; a comparison button resets tint strength to 0.

Numeric labels update with every slider signal. Crossing distance endpoints adjusts the other endpoint and its displayed slider value. Copy Settings copies a JSON snapshot of every exposed effective runtime value, including RGBA fog colour and shared forest foliage colour/strength. No disk writes or editor-default mutations occur. Future controls can reuse the section/slider shell and bind explicit feature owners; no future tabs are built now.

See `WORLD/Environment/README.md` for configuration, rendering limitations and validation. Compilation/headless F1, cursor, slider and clock/sun checks passed; interactive layout, clipboard and rendered visuals require local Godot testing.

The forest tint controls update four shared ShaderMaterial resources via `WORLD/Vegetation/ForestFoliagePalette.cs` only on UI events. They do **not** use Shader Globals in Project Settings, because the required globals were absent when Godot attempted to compile the shader. The authored initial palette is set in that helper and the default shader parameters (Color(0.43,0.55,0.37), strength 0.25), with `palette_response=0.85` on each material. No tree/grass batch traversal or per-frame updates. The shader compilation error reported by the owner led to this source-level fix, but it has not been re-tested locally in Godot.


## Runtime pine-tree art-style selector (2026-10-30)

F1 now contains **Pine tree model comparison** with a dropdown for **PineTree002 (original realistic)** and **PineTree001 (stylized faceted)**. The debug owner receives the existing `PineForest` directly from `TestWorld.Initialize`; it does not inspect the entire scene. The original 002 remains the startup default; switching back immediately restores it.

The selection swaps **only the Mesh on each already-created MultiMesh**. Trunk collision proxies, per-instance transforms and deterministic positions, batch placement, shadow policy, forest density, 70/280/600m native LOD distances and grass/fern systems are preserved. There is no second simultaneously rendered forest and no per-frame script processing. The stylized PineTree001 models are loaded/prepared only at first selection, then cached; subsequent switches reuse the same prepared meshes and original MultiMesh transforms. This incurs one-time import/material setup and a per-batch mesh setter call on the first toggle, not zero cost. The default PineTree002 remains the only visible set until changed.

PineTree001 source files are named `LOD0/LOD1/LOD2` (near/mid/far) and PineTree002 source files `LOD1/LOD2/LOD3`; the switching code maps both to the same three existing renderer tiers. The shared F1 colour-picker and strength slider also update PineTree001's **foliage** ShaderMaterial now; its bark uses the separate stylized vertex-colour shader. Copy Settings includes `PineTreeModel`. The dropdown and tint are runtime-only; restarting restores PineTree002.

**Pending Godot verification:** Compile, reimport the stylized GLTFs if needed, open F1, switch both ways while standing still, inspect all LOD distances, sunlight/shadows, shared tint, FPS and stable tree collision. Repository source checks do not establish engine acceptance.

## Startup model change

`WORLD/Scenes/test.tscn` now starts with PineTree001. The F1 model comparison dropdown still changes to PineTree002 and back; switching does not change tree placement or reconstruct collision. Only the selected startup model loads initially, with the other family cached on first selection. The runtime dropdown resets to PineTree001 when the test scene restarts. Visual confirmation is pending.
