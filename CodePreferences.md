# Code Preferences — Final Machine

> **Applies to:** `ben0kay/final-machine` **only**. Read [README.md](README.md) first.  
> **Development branch:** `main` unless the owner explicitly names another branch. Never inherit old repository branch policies.  
> **References:** [terra-machine](https://github.com/ben0kay/terra-machine) and [war-machine](https://github.com/ben0kay/war-machine) are **reference projects**, not the target repository.

**Terrain3D-first project.** Reference repos may contain obsolete paths and architecture: never import War Machine's branch policy, Terra Machine's Voxel Tools architecture or assumed files.

## Before every AI task

1. Confirm repo, branch, authorized scope and actual files; read `AGENTS.md`, root README, this file, relevant feature docs and code.
2. Prefer **Godot 4 .NET/C#**, native scenes/resources and installed **Terrain3D**; never silently port old terrain/Voxel Tools code.
3. Preserve unrelated work; no unsolicited migrations, scene restructuring or gameplay additions.
4. Prefer **one commit per authorized job**; report SHA/files. Push authorized work, never extra unrelated work. No forced resets/checkouts or overwriting others' changes.
5. **Brief, proportional checks only:** focused source review, basic build/essential test when practical; owner handles extensive visual/FPS validation. Mark unverified results.

## Coding style and organization

- Put a brief explanatory comment at the top of every new script.
- Prefer comments **above** methods, with a `// =========================================================` divider for easy navigation.
- Use `#region` / `#endregion` to organize substantial related method groups when helpful.
- Favor moderately **vertically compact**, readable C# rather than sprawling or overly abstract code.
- Major feature folders need a small `README.md` (responsibility, data flow, configuration, limitations). Read before edits; update when behaviour changes.
- Use a grouped **master configuration resource** (`Configuration/<Feature>Master.tres` plus typed C# `Resource` when appropriate) for frequently adjusted knobs. Group exports in the Godot inspector.
- Keep modular, discoverable feature folders and reusable, clearly owned logic. Use shared helpers/plain `Node` helpers when useful; avoid unnecessary managers and abstraction.
- Keep gameplay/domain logic separate from addon/native integrations and imported visuals; allow art replacement without rewriting mechanics.

### World scene-root ownership (mandatory)

Use [War Machine's actual `WORLD/World.tscn`](https://github.com/ben0kay/war-machine/blob/implement-td0-td1/WORLD/World.tscn) as the **conceptual root-container reference** for runtime spawn, persistence, streaming and debug ownership—not merely tidy folder names:

```text
World
├── TerrainRoot
│   └── Terrain3D
├── WorldActorsRoot
│   ├── PersistentEntitiesRoot
│   │   ├── Player
│   │   ├── Zombies
│   │   ├── Wildlife
│   │   └── RobotPatrol
│   └── PersistentPropsRoot
│       ├── Structures
│       └── Objects
│           └── Cars
├── Camera
├── GameplayHUD
├── DebugUIRoot
└── SystemsRoot
    ├── Audio
    ├── VegetationRoot
    └── EnvironmentRoot
```

- Adapt these **meaningful ownership roots** to Final Machine; **create only groups needed now**. Never port unused War Machine systems or add placeholders for distant plans. Preserve a `WorldActorsRoot/PersistentEntitiesRoot` player ownership pattern where applicable.
- Spawn actors (player, wildlife, zombies) under the entity root; structures, vehicles and physical props under appropriate persistent prop roots—**not scattered under `World`**. Separate terrain, actors, props and shared systems for category-specific save/stream/visibility/debug without global scans.
- Use reusable instanced subsystem scenes and lightweight organizational `Node`/`Node3D` parents; **container-only nodes need no processing scripts**. Avoid flat clutter and needless nesting; preserve/update stable paths and references on changes.
- Keep foliage under a responsible `VegetationRoot`/owner with Terrain3D or MultiMesh batching—not thousands of separate tree/grass scene nodes. Inspect the reference scene before inventing a different hierarchy; retain Terrain3D as terrain foundation.

### Reference repository conventions

- **[Terra Machine](https://github.com/ben0kay/terra-machine):** foliage authoring, tree LOD/shadow budgets, instancing, shaders, performance, C# organization and landscape lessons. **No Voxel Tools/excavation port.**
- **[War Machine](https://github.com/ben0kay/war-machine):** object catalogues, buildings/props, zombies/anatomy/AI, physics, master resources and C# documentation **as references only**, not active architecture/branch rules.
- Optional idea sources: [godot-tower-defense](https://github.com/ben0kay/godot-tower-defense), [isometric-new-2d](https://github.com/ben0kay/isometric-new-2d), [Ben_MonsterExpansion](https://github.com/ben0kay/Ben_MonsterExpansion).

## Performance-first implementation rules

**Ask before coding:** *What activates this feature? What happens while it is active? What makes it stop? What does it cost while idle?*

- **Inactive means dormant:** no `_Process` / `_PhysicsProcess` loop by default when no recurring work is necessary. Use signals, input events, `SetProcess(false)`, `SetPhysicsProcess(false)`, timers, explicit wake/sleep and effect shutdown where appropriate.
- Shared managers, reusable helpers, instancing, batched foliage, pooling and cached resources are preferred over huge numbers of individual per-frame node updates.
- Update changed shader/material settings on changes; do not rewrite the same uniforms every frame. Keep frequent paths allocation-conscious and avoid redundant scene-tree searches and resource loads.
- Only simulate nearby, relevant actors in detail. Design for future world/object streaming: register/unregister with region residency; do not scan the entire map each frame.
- Weather particles, fire, chopping effects, creature investigation, construction events etc. should have explicit activation and termination behavior. Continuous animation such as active wind/daylight may legitimately update, but minimize duplicate work.
- Terrain LOD, region residency, vegetation LOD, collision/physics activation, AI simulation and object persistence are **separate concerns**. Do not claim native Terrain3D mesh LOD automatically provides full region/object streaming.
- Report any meaningful FPS/memory measurement conditions; never fabricate improvements. Include wake/sleep, scaling strategy and unverified checks in each feature handoff.

## Assets, graphics and scene structure

- Prefer GLB/GLTF for authored art, retaining reusable game-facing `.tscn` prefabs; replace visuals without replacing gameplay contracts, collision and pivots/sockets.
- If generating geometry with C#, use an asset-local **`C/`** folder for mesh builders and an **`Artwork/`** sibling for future imported art, with `Configuration/` for editable settings where appropriate. `C/` is **not** where all C# gameplay scripts belong.
- Source-model triangle budgets are **starting guidance**, not mandatory engine limits. The old Terra Machine authoring guide suggests mature trees around 4k–6k triangles near and 1k–1.5k at a further LOD, with much smaller budget for grass. Test real frame time and shadow/alpha overdraw rather than judging triangles alone.
- Prefer simple colliders on trees and props unless precise contact matters. Separately consider static world geometry, interactive physics bodies and future simulation streaming.
- Use Terrain3D's actual terrain material/painting capabilities where appropriate; do not blindly port the old voxel triplanar shader.
- For caves, basements and overhangs, use separate meshes and intentionally hidden terrain-hole boundaries. Shallow dig edits remain a distinct future heightmap feature.

## C# syntax / build hygiene

- Collection enumeration uses `foreach (var item in items)`, **not** invalid `for (var item in items)`.
- Indexed loops use `for (int i = 0; i < count; i++)`. Inspect newly added loops, types and Godot API names before committing.
- Prefer an initial `dotnet build` for authorized C# work if the Godot .NET SDK/environment is available. If not, perform focused source review and explicitly state **compilation not verified**. A successful Git push is not proof of a working scene.
- Addon/native calls must match the **installed** Terrain3D version; inspect documentation or actual addon source and avoid imaginary dynamic method names.

## Planning / handoff

Regular Chat: exploration, plans and implementation when authorized. Codex/Work: multi-file C#/scene integration; Copilot: delegated local checks. **Tool choice never grants task authorization.**

- For substantial work, propose short independently reviewable phases with IDs, current vs proposed behavior, owner paths, configuration, performance cost, prerequisites and acceptance checks.
- Do not automatically implement later phases. Use Planned / In Progress / Implemented / Awaiting Validation / Accepted and keep README statuses accurate.
- Keep tests proportional. If the owner wants local checks delegated to Copilot, hand off exact commands, prerequisites and expected results; do not imply checks were already run.
- Update owning documentation after authorized changes; remove superseded contradictions, mark future tasks clearly. Document significant fixes centrally in `LESSONS_LEARNED.md` per `AGENTS.md`.

**Reminder:** The game direction is a **Sons of the Forest-like believable landscape first**, with DayZ-style survival and Project Zomboid-like simulation complexity later. Don't rush into AI, vehicles, crafting or world-scale changes while the visual benchmark is unfinished.
