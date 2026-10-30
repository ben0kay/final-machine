# Code Preferences — Final Machine

> **Applies to:** `ben0kay/final-machine` **only**. Read [README.md](README.md) first.  
> **Development branch:** `main` unless the owner explicitly names another branch. Never inherit old repository branch policies.  
> **References:** [terra-machine](https://github.com/ben0kay/terra-machine) and [war-machine](https://github.com/ben0kay/war-machine) are **reference projects**, not the target repository.

This file adapts the user's established coding preferences from Terra Machine/War Machine to a **fresh Terrain3D-first project**. The reference projects contain useful patterns but may also contain superseded decisions, paths or implementations. **Never carry across War Machine's `implement-td0-td1` branch instruction, Terra Machine's Voxel Tools architecture, or either repository's assumed files.**

## Before every AI task

1. Confirm the current repository, checked-out branch and user-authorized scope. Read root README, this file, feature README/plan and relevant source code. Validate actual file presence rather than guessing.
2. In Final Machine, prefer **Godot 4 .NET/C#**, the native Godot scene/resource system, and the installed **Terrain3D** addon. Do not reintroduce Voxel Tools for terrain or silently port prior terrain implementations.
3. Keep unrelated work intact. No broad asset migrations, scene reorganizations or new gameplay systems without explicit approval.
4. Prefer **one Git commit per authorized job**; report commit SHA and files changed. Do not push unrequested additional work. Never force reset/checkout or overwrite others' changes.
5. Validation should be **brief and proportional**: code/source review and basic build/essential test when practical. Leave extensive visual FPS testing to the owner unless requested. Clearly label anything unverified.

## Coding style and organization

- Put a brief explanatory comment at the top of every new script.
- Prefer comments **above** methods, with a `// =========================================================` divider for easy navigation.
- Use `#region` / `#endregion` to organize substantial related method groups when helpful.
- Favor moderately **vertically compact**, readable C# rather than sprawling or overly abstract code.
- Put a small `README.md` in each major owning feature folder describing responsibility, data flow, configuration and limitations. Consult the feature README before editing; update it when behavior changes.
- Use a grouped **master configuration resource** (`Configuration/<Feature>Master.tres` plus typed C# `Resource` when appropriate) for frequently adjusted knobs. Group exports in the Godot inspector.
- Organize files into modular, discoverable, reusable feature folders. Use helpers for shared or staggered work; avoid gratuitous managers and unnecessarily cluttered scene trees. A plain shared Node helper is fine when it genuinely simplifies organization.
- Prefer simple implementations with clearly owned responsibilities and replaceable art/graphics. Do not overengineer for imaginary future features.
- Keep gameplay/domain logic separate from addon/native integrations and from imported visual assets.

### Godot scene tree organization (mandatory preference)

- Keep **root scene trees clean and easy to navigate**. A main world scene should show only major responsibilities at its top level (for example `Terrain`, `Environment`, `Vegetation`, `Entities`, `Structures`, `Player`, `Debug`), not dozens or hundreds of individual objects.
- Group related nodes under clearly named **plain organizational `Node`/`Node3D` parents** or reusable subscenes. For example, put forest instances and their helpers under `Vegetation/Forest` instead of scattering trees across the world root.
- Prefer **instanced reusable scenes** for self-contained systems (player, terrain, environment, forest manager, buildings). Keep the owning system's internal nodes inside that scene rather than exposing its internals at the world root.
- Use descriptive, consistent names and sensible hierarchy depth: avoid both a flat cluttered root **and** unnecessary nesting/wrapper nodes. Do not add managers or processing scripts to organizational nodes that only need to group children.
- **Never make thousands of individual tree/grass/prop scene nodes merely for editor organization.** Prefer appropriate MultiMesh/native Terrain3D instancing or streamed batches, grouped under a small number of meaningful scene owners.
- When adding a feature, place it under the correct existing root group; create a new top-level category only if it represents a genuinely separate responsibility. Preserve stable scene paths/signals and update references when a hierarchy must change.
- Keep the Godot scene tree intuitive enough that the owner can quickly expand a top-level group, find a system, and toggle visibility/debug it without hunting through unrelated nodes.

### Reference repository conventions

- **[Terra Machine](https://github.com/ben0kay/terra-machine):** look at foliage authoring, pine-tree LOD/shadow budgets, instancing, shaders, performance comparisons, C# scene/config organization and landscape experiments. Reuse **knowledge** selectively; don't import Voxel Tools classes or old excavation API into Terrain3D.
- **[War Machine](https://github.com/ben0kay/war-machine):** use object catalogue, building/prop authoring notes, zombie/anatomy/AI, physical interactions, modular master-resource and C# documentation patterns **as design references**, not authoritative architecture. The code and branch policies belong to that project.
- Other optional examples from established preferences: [godot-tower-defense](https://github.com/ben0kay/godot-tower-defense), [isometric-new-2d](https://github.com/ben0kay/isometric-new-2d), [Ben_MonsterExpansion](https://github.com/ben0kay/Ben_MonsterExpansion) for ideas only.

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

Regular Chat is useful for exploring and drafting scoped implementation plans; Codex/Work for significant multi-file implementation, C# integration, scene or system work; Copilot for local build/runtime checks if the owner delegates them. These roles are preferences, not permission to proceed.

- For substantial work, propose short independently reviewable phases with IDs, current vs proposed behavior, owner paths, configuration, performance cost, prerequisites and acceptance checks.
- Do not automatically implement later phases. Use Planned / In Progress / Implemented / Awaiting Validation / Accepted and keep README statuses accurate.
- Keep tests proportional. If the owner wants local checks delegated to Copilot, hand off exact commands, prerequisites and expected results; do not imply checks were already run.
- Update owning documents after authorized work, remove contradictory superseded guidance and leave future tasks clearly marked.

**Reminder:** The game direction is a **Sons of the Forest-like believable landscape first**, with DayZ-style survival and Project Zomboid-like simulation complexity later. Don't rush into AI, vehicles, crafting or world-scale changes while the visual benchmark is unfinished.
