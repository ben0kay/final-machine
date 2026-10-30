# FM0 composition and validation
`test.tscn` is the main scene: native saved terrain, fixed Sky3D daylight, one-shot pine forest plus separate patchy grass scatter, reusable player and a compact, always-visible HUD FPS + rendered-primitives label (0.5-second sampling). TestWorld waits for terrain collision before placing the player at (0,ground+0.15,20); it stops polling after placement. No unrelated gameplay.

## Checks performed
- C# project restored and compiled with Godot.NET.Sdk 4.6.0 / .NET 8.
- Godot 4.6 headless: native region bake, sixteen saved regions, three packed material sets, all three pine glTF scenes and a real terrain collision ray at spawn.
- Original FM0 (before tree thinning/grass) Godot 4.6 .NET main-scene headless smoke check passed: 7,094 pines in 1,473 spatial batches; no script/runtime errors in that run. Visuals, input feel and GPU performance remain unverified.

Headless mode cannot establish visual acceptance. Locally use Godot .NET, allow imports, build and run: inspect meadow/forest/cliff blending at 1.65m, walk/sprint/jump on slopes, inspect tree rooting/trunk collision, cross both LOD distances from meadow and hill, check woodland shadow contrast and LOD3 shadow absence, sculpt/save/reopen to verify edit preservation, and measure uncapped FPS.

Bundled source/demo imports already contain missing demo MTL files and a duplicate unrelated grass shader UID. Blender-source auto-import is disabled because source .blend files are not runtime inputs and headless import cannot configure Blender. FM0 uses existing glTF/PNG/JPG inputs only. Do not infer an error-free full repository import from the targeted native check.

Environment limitation: the temporary editor could not auto-discover its .NET SDK (process metadata restrictions); direct MSBuild succeeded and the built assembly ran in Godot .NET. Headless editor shutdown also reported addon/dummy-renderer resource leaks. A clean local editor build/import and GPU shader/visual acceptance are still required.

## FM1 environment tuning
The existing environment has an event-driven EnvironmentController child, with authored EnvironmentMaster defaults and a runtime copy. TestWorld binds it and the reusable overlay under DebugUIRoot once. F1 releases the pointer and blocks player input; F1/Escape restores the prior capture state. HUD/FPS remains independent. The environment overlay does not control grass. Tree retention and grass patches have independent vegetation settings; the current grass/thinning changes have not been built or rendered locally. See ../Environment/README.md and ../../DEBUG/Overlay/README.md for validation.

## Always-visible rendering counters

`HUD/FPS` remains an independent top-left label visible while the F1 environment panel is closed or open. Its existing 0.5-second timer now displays `FPS | rendered primitives` using `Godot.Performance.Monitor.RenderTotalPrimitivesInFrame`. This is a built-in render-workload estimate, **not an exact count of unique visible mesh triangles**: it may count repeated depth/shadow passes or indices/vertices and varies with culling. Small non-interactive text; no extra per-frame processing or extra debug scene. If `Performance` counters are unavailable in some release configurations, check editor Monitors instead. Runtime Godot visual confirmation is left to the owner.
