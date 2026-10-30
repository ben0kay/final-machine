# First-person player — FM0
Reusable C# CharacterBody3D scene. Standard 1.8m capsule, camera 1.65m above feet, 45° maximum walking slope and 0.4m floor snap. No Terrain3D references. TestWorld waits for an ordinary terrain physics ray before enabling movement.

Arrows move, Shift sprints, RMB jumps, Escape releases cursor, left click recaptures. Uncaptured cursor stops movement intent. Configuration/PlayerMaster.tres owns speeds, acceleration, gravity, jump, mouse sensitivity, Y inversion and FOV. Initial tuning follows Terra Machine (5m/s, 1.6× sprint, inverted Y). Change InvertMouseY in the resource for conventional look.

Runtime cost: one player physics callback and event-based mouse input. Local acceptance: walk uphill/downhill, jump, hit trunks, release/recapture cursor and change sensitivity/inversion.
