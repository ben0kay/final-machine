# FINAL MACHINE

> **Working title:** Final Machine  
> **Direction:** Hardcore first-person wilderness survival simulator; **Sons of the Forest-style immersion and believable landscape are the immediate priorities**.  
> **Terrain foundation:** **Terrain3D**, a finite, seed-generated and selectively authored heightmap world. **Do not introduce Voxel Tools** unless the owner explicitly revisits that decision.  
> **Status:** New Godot project / design and technical foundation. Aspirations below are **not implemented features** or blanket authorization to build them.

## Read first — project identity

Final Machine is a **fresh repository**, not a migration-in-place of [Terra Machine](https://github.com/ben0kay/terra-machine) or [War Machine](https://github.com/ben0kay/war-machine). They are **reference repositories only**: reuse lessons, suitable ideas, assets or modular patterns selectively and with explicit task scope. Do **not** assume their scripts, addons, paths, current functionality or Git branches exist here. Do not silently copy their older voxel or zombie-game architectures.

**Core fantasy:** Alone in a large, believable wilderness, the player moves cautiously through forest, searches for shelter before dark, quietly secures a rural house, avoids drawing zombies to noise or light, and ventures out at dawn to track and hunt deer. Environmental awareness, preparation, risk and physical interaction should matter more than combat spectacle. Realism means **credible systems and consequences**, not simulating everything at all times.

### Creative references — distinct jobs

1. **Sons of the Forest — immediate visual and immersive reference.** Dense, mature forests; layered vegetation, believable terrain, natural clearings, filtered sunlight, shadow, ground detail, geology and intimate first-person presence. Seek a **semi-realistic, cohesive** look without demanding that game's fidelity or assets.
2. **DayZ — future survival/exploration reference.** Scarcity, scavenging, equipment, exposure, injuries, careful travel, rural structures and eventual vehicles.
3. **Project Zomboid — future simulation reference.** Consequential, interconnected survival, environmental noise, zombie investigation, physical objects, shelter, injuries and persistence. Favor event-driven, locally simulated behavior over expensive always-running world simulation.
4. **7 Days to Die — world-generation pattern only.** A finite map generated from a seed, using procedural terrain/biomes/road layouts and carefully authored, reusable building/POI prefabs. This does **not** imply voxel mining or a tower-defense horde loop.

**Immediate quality bar:** Walking slowly through the forest at player eye level should already feel atmospheric. A forest that looks excellent at 256 × 256 m is more valuable than a bland, untested 20 × 20 km world.

## Terrain3D decision — why we changed direction

Terra Machine explored smooth Voxel Tools terrain because unrestricted ground excavation initially seemed essential. After clarifying the gameplay, **the desired digging is mostly shallow trenches and animal/zombie pits**, not player-carved tunnels through hillsides. The higher priorities are **beautiful open landscapes, efficient heightmap/texture authoring, foliage and an eventual large finite procedural world**.

**Terrain3D is the chosen terrain implementation** for the new project. Its heightmap/regions, landscape editing, material painting and foliage workflows are a better fit for the current goal. This is an **authoring/workflow decision**, not a claim that Voxel Tools inherently looks worse or that Terrain3D guarantees higher FPS.

- **Finite, not necessarily small or handcrafted.** The eventual map should be seed-generated, with authored roads, cabins, towns, landmarks and cave entrances. The extent is intentionally undecided. Do not promise infinite terrain or pick a giant area before testing memory and authoring workflows.
- **Terrain3D provides the outdoor ground surface.** It does not directly model ground overhangs, tunnels or stacked surfaces at one horizontal position. Such structures should use **separate 3D meshes**.
- **Caves:** use standalone cave/tunnel/chamber meshes joined to terrain holes with natural-looking rock/soil transition geometry; procedural cave-mesh generation is possible later.
- **Basements:** use ordinary building/interior meshes, foundations and stairs with an appropriately authored terrain opening. Handle terrain-to-building seams carefully.
- **Shallow trenches and pits:** investigate runtime localized heightmap edits, terrain collision refresh and durable saves through the **installed** Terrain3D API. A player shovel is **future scope**, not assumed working. Original terrain height should be retained if later hardness is measured relative to undug ground.
- **Physics:** Godot/Jolt owns vehicles, logs, props, ragdolls, joints and interactions; Terrain3D primarily provides landscape and ground collision. Complex physics is not an intrinsic voxel feature.
- **Very large worlds:** terrain LOD, loading/unloading terrain regions, foliage/object streaming and simulation activation are separate problems. Verify Terrain3D capabilities **for the exact installed version**. Do not assume that LOD means automatic disk-backed region streaming. Keep integration modular so a region manager can be added if required.

### Technical direction

Use Godot 4 .NET/C# with the project renderer/physics settings validated on the owner's machine. The repository currently starts as a minimal Godot project. **Terrain3D has not yet been installed or configured in this repo** as of this document's creation; inspect the actual addon and version before coding. Treat upstream APIs as version-specific; do not invent methods.

Proposed modules **only when the associated feature is authorized**:

```text
WORLD/
  Terrain/
    Integration/Terrain3D/      # Terrain3D API boundary, scene and data workflow
    Landscape/                  # seeded landscape, authored overrides, regions
    Configuration/              # tweakable terrain/material settings
    Excavation/                 # shallow digging rules and persistence later
  Environment/                  # sky, sunlight, fog, weather later
  Vegetation/                   # forests, trees, grass, undergrowth, rocks
  Structures/                   # cabins, houses, basements, cave entry meshes
PLAYER/                         # first-person movement, later equipment
ENTITIES/                       # wildlife and threats when authorized
ASSETS/                         # authored visual assets, GLBs, materials
DEBUG/                          # rendering/performance tools
```

Do not create empty placeholders or migrate entire old directory trees simply to match this sketch. Give major implemented features small, current README files and keep terrain-specific native calls inside the integration layer. Use game-owned interfaces for surface queries, composition and future excavation. Keep scene assets and independent gameplay reusable.

## Current priority: visual-first wilderness

**First playable benchmark** (suggested target, not implemented):

- One compelling **~256 × 256 m** forest/wilderness section: believable hills, a slope/cliff, a clearing, dense mature trees and natural woodland understory.
- Ground materials with convincing scale and blending: soil, sparse grass, leaf litter, moss, rock; avoid obvious texture tiling, bright painted circles and artificial vegetation clumps.
- Layered vegetation: mature canopy, saplings, bushes/ferns, grass, fallen branches and rocks. Mixed spacing, height, species and canopy density should make woodland feel naturally grown.
- Atmospheric sun/shadow/sky and restrained haze; forest canopy should genuinely block some light. Avoid using fog to disguise missing assets.
- Quality judged **in first person**: the view while moving slowly through forest matters more than a top-down overview.
- Profile the actual cost of trees, shadow rendering, alpha foliage, materials and ground shaders. Use instancing, suitable LODs, shadow distance rules, billboards/impostors where justified, and properly sleeping distant systems.

### Starting performance/art reference from Terra Machine

The previous project documented provisional **full-sized tree** budgets of approximately **LOD1 4,000–6,000 triangles**, **LOD2 1,000–1,500 triangles**, with proportionally smaller budgets for saplings, rocks and other props; grass should be very low-poly (roughly **20–100 near, 12–40 far** triangles where appropriate). These are **initial guidelines**, not hard engine limits. Judge visuals and FPS together. [Terra Machine vegetation guide](https://github.com/ben0kay/terra-machine/blob/main/ASSETS/Vegetation/VISUAL_AUTHORING_GUIDE.md) records useful texture, normals, winding and import lessons. It is a reference, not proof those assets have been copied here.

## Long-term survival and simulation aspirations

These are **design targets only**, not active tasks:

- **Stealth and sensory AI:** footsteps varying by terrain/material and movement, visibility/line of sight, uncertain investigation of gunshots, branches and building noises. Nearby zombies should search approximate areas rather than know exact player positions; distant threats should sleep or be abstracted.
- **Wildlife and hunting:** believable deer senses, caution/fleeing, tracking and stalking, potentially wind/scent systems if proven worthwhile.
- **Survival consequences:** thirst, hunger, warmth/wetness, fatigue, injury, recovery, sleep, supply scarcity and choices between risk and safety.
- **Shelter/scavenging:** rural homes, cabins, doors/windows, cautious entry, interiors, meaningful supplies and future construction with separate physical meshes.
- **Equipment/physics:** tools, carried logs, movable objects, falling trees, Jolt bodies, eventual vehicles and appropriate persistence. Physical consequences matter more than unnecessary simulated detail.
- **World evolution:** procedural landscapes/biomes plus authored roads/POIs, cave entrances, settlements and potentially very long driving journeys. Define the first map size through technical tests, not ambition alone.
- **Shallow excavation:** trenches, defensive pits and trap foundations by heightmap modification, eventually saved, collision-correct and limited by soil/rock rules. No unrestricted tunnel excavation or voxel mining system.

## Roadmap — independently authorized phases

| Phase | Goal | State |
| --- | --- | --- |
| **FM0** | Verify project/.NET, install and validate Terrain3D version; minimal scene, ground, player and collision | Planned |
| **FM1** | Beautiful forest/landscape visual benchmark, native terrain materials, foliage, atmosphere and FPS profiling | Planned |
| **FM2** | Finite seeded geography with authored-point-of-interest pipeline; evaluate terrain regions and scale | Planned |
| **FM3** | First quiet survival loop: forest walk, building shelter, sound perception, one wildlife/enemy prototype | Deferred |
| **FM4** | Shallow trench/pit excavation proof with terrain/collision updates, then persistence | Deferred |
| **FM5** | Vehicles, large-world loading/object streaming and advanced simulation | Deferred |

Phase identifiers are planning aids; **do not auto-implement the roadmap**. First confirm the actual project/addon state and have the owner approve a focused task.

## AI-agent implementation rules

Read [CodePreferences.md](CodePreferences.md) before changes. Favor C#, modular ownership, configuration resources, referenceable scenes/GLBs, comment headers and method regions, minimal idle processing, and targeted local tests. Use the installed addon’s documented API and preserve reproducible data/seed behavior. Keep separate authored sources and generated terrain data; do not overwrite user edits on startup.

**Reference repositories:**
- [terra-machine](https://github.com/ben0kay/terra-machine): previous forest art, vegetation, terrain experiments, shovel prototype and performance lessons. **Voxel Tools-specific implementations should not be carried over.**
- [war-machine](https://github.com/ben0kay/war-machine): broader C# modular patterns, object/structure authoring, physics, AI and detailed design notes. Do not assume its older branch, gameplay or code is active here.

**North star:** *A world worth moving through carefully: believable forests, real consequences, and the tension of finding shelter before nightfall.*
