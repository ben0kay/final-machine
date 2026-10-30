# SmallBuilding — C# geometry preview

A **single-room, 6 × 5 m** small cuboid shell with 2.9 m high damaged-plaster walls, a tiled damaged-concrete floor and matching flat roof. The +Z facade has a real 1.4 m wide, 2.2 m high walk-through doorway. The roof has an actual open 1.8 × 1.6 m rectangular opening. There are no windows, doors, furniture, interactions or persistence yet.

## Try it

1. Allow Godot .NET to import the two existing texture sets under `ASSETS/Material/Floor/` and `ASSETS/Material/Wall/`.
2. Build the C# solution in Godot.
3. Open `SmallBuilding.tscn` to inspect its shape in the editor, or **run `SmallBuildingPreview.tscn` as the current scene (F6)** to walk outside and inside with the existing first-person controller.
4. To place the structure later, instance `SmallBuilding.tscn` under an appropriate persistent structures root. Its origin is at the finished ground-floor surface. It is intentionally **not** added to `WORLD/Scenes/test.tscn`.

## Ownership and tuning

- `C/SmallBuilding.cs` creates 11 closed box panels at scene initialization: floor, two doorway sides, lintel, three other walls and four roof pieces. No `_Process` or `_PhysicsProcess` callbacks.
- All panels render in **one MeshInstance3D with two material surfaces**. Each panel has a simple box collision shape owned by the root `StaticBody3D`; there is no doorway collision and no roof collision across the opening.
- `Materials/DamagedConcrete.tres` and `Materials/DamagedPlaster.tres` reference the existing diffuse, OpenGL normal and roughness texture files directly. Their shared UVs repeat according to physical face size (`TilesPerMetre`) rather than stretching a single texture over each long wall. The displacement maps are deliberately unused for this simple shell.
- The C# tool script previews geometry in the editor after building the solution. Generated children have no scene owner, so saving the scene does not bake duplicate generated nodes. Inspector dimensional changes take effect after reopening/reloading the scene or instancing it anew.

## Placement in the FM0 test world

`WORLD/Scenes/test.tscn` now instances the building under `WorldActorsRoot/PersistentPropsRoot/Structures` near the player spawn, at X=7, Z=4. The +Z doorway faces the spawn. On runtime player spawn readiness, `TestWorld.cs` samples the saved native terrain height at the structure's centre and aligns its finished floor once. This is a temporary visible benchmark; it does not alter the reusable building scene, flatten terrain, clear grass or add save/streaming. The separate `SmallBuildingPreview.tscn` still works.

**Scope:** This is a standalone appearance/proportion test, not a building authoring system. Foundation-to-uneven-Terrain3D contact, terrain paint masks, roof access, structural integrity, destruction and save/streaming rules remain outside this pass. On sloped ground the square foundation may float or intersect until it is positioned/leveled.

**Verification:** Source and resource paths reviewed. Local Godot .NET build, shader/normal texture import, editor preview and in-game collision/lighting must be checked by the owner.
