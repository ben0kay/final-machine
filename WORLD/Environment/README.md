# FM1 — forest atmosphere and live tuning

`environment.tscn` retains installed Sky3D 2.1 and its sun/moon, now running a configurable day/night cycle. `daylight.gd` applies initial ambient lighting/SSAO after Sky3D initializes. `EnvironmentController` is the game-owned boundary: TestWorld binds the ready Sky3D instance and player camera once. No addon code or weather simulation is added.

## Defaults and ownership

Edit `Configuration/EnvironmentMaster.tres` (typed `EnvironmentSettings`) for authored defaults: distance fog enabled, clear 60m, fully obscured 180m, muted grey-blue tint, native atmospheric density 0.008, starting time 13:30, **day length 60 real minutes**, sunlight 1.2 and shadow range 100m. Runtime settings are a private duplicate. Debug edits never save resources, scenes or terrain. The full distance remains at least one metre beyond clear distance. Atmospheric haze is independently controlled: turning distance fog off leaves the density setting in effect; set density to zero to remove both.

## Time of day and cycle length

Sky3D's `game_time_enabled` is now on, with `minutes_per_day = 60` (slower than Sky3D's 15-minute default). F1 Time / lighting includes a **Day length (real minutes)** slider from 15–240 minutes in 5-minute steps. Both the cycle length and manual time-of-day slider apply live, without editing the authored master resource. The controller sets the initial clock only once: changing fog, sunlight or day length must not unexpectedly reset an already-running clock. Time changes use an explicit `SetHour` method. No extra per-frame code is added by Final Machine; Sky3D owns its regular clock update.

## Distance rendering

The installed Sky3D fog multiplies distance, density and height falloff, so it does not guarantee opacity at the chosen endpoint. Its screen-space fog remains disabled. Instead `Fog/DistanceFog.gdshader` uses one camera-owned, late transparent fullscreen quad and a depth read. It reconstructs spherical camera distance, smoothly blends from clear to full, and applies the same tint to the horizon with a gradual reveal of Sky3D overhead. This avoids a hard horizon line or clear distant backdrop. Native exponential haze adds the adjustable atmospheric component, so “clear distance” applies to the extra distance overlay, not removal of natural haze. Fog colour remains a fixed user tint when changing time.

No CPU process callbacks, player-position uploads, per-tree processing, far-plane changes or automatic culling are added. Disabled distance fog hides its draw. Hidden scenery can still cost rendering work; this is atmosphere, not streaming. Transparent objects use opaque depth behind them; opaque/alpha-scissored vegetation is supported. LOD placement/ranges/shadow policies and Terrain3D data are untouched. Sky3D retains its runtime Environment reference so clock changes continue updating the same sky.

## Validation

.NET build passed without warnings/errors. Headless .NET scene checks passed: defaults load, panel opens/closes via F1, visible pointer state restores (headless capture is unsupported), slider values apply, crossed endpoints stay valid, Sky3D clock and sun rotation change, FPS remains visible and authored defaults remain unchanged. Godot's existing Sky3D interpolation deprecation warning remains. The editor import attempt reported a host MSBuild initialization error and dummy-renderer exit leaks; the separate CLI build and runtime check passed.

Local Forward+ checks still required: shader compilation and actual fog appearance, camera movement under canopies, nearby clarity, slopes/horizon seams, LOD3 fading, time/lighting appearance, panel layout/scroll/picker/clipboard, Escape/open-state restoration and GPU overhead. Run `WORLD/Scenes/test.tscn`, press F1 and vary endpoints/density/time, then toggle fog off/on. Do not infer measured FPS improvement from fog obscuration.
