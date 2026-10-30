# Lessons Learned — Final Machine

> **One central record of significant mistakes and reusable fixes.**
> AI agents must read this file before changing relevant systems and update it when a meaningful, verified lesson emerges. Do not create separate per-folder lessons files unless the owner requests it.

## How to use this file

- Search by subsystem, asset name, symptom or ID **before implementing** a related task.
- Add a lesson only for a significant recurring failure, difficult bug or broadly reusable prevention rule—not small typos, ordinary preference tuning or every iteration.
- Keep each entry short: **symptom → root cause → fix/prevention → verification**. Explicitly distinguish confirmed facts from suspected causes and untested fixes.
- If a new mistake crosses systems, keep **one entry** with multiple subsystem tags; don't duplicate it.
- If a later investigation changes a diagnosis, **correct the existing entry** and note the revision instead of accumulating contradictory rules.
- Record only checks actually performed. The owner handles extended Godot visual/FPS checks, following `CodePreferences.md`; maintaining this file **does not authorize extensive testing**.
- Existing detailed authoring guidance remains in its owning guide. This file is the central searchable memory, not a replacement for `CodePreferences.md` or the feature docs.

## Known lessons

### VEG-001 — Forest grass illuminated incorrectly from opposite directions

**Area:** Vegetation / mesh export / Godot lighting  
**Historical asset:** `ForestGrass001` A/B/C, LOD1 and LOD2  
**Evidence:** [Vegetation visual authoring guide](ASSETS/Vegetation/VISUAL_AUTHORING_GUIDE.md#texture-normals-and-lighting-failures-that-must-not-recur)

- **Symptom:** Grass looked unusually pale toward sunlight and very dark from the opposite viewing direction; changing albedo was not a reliable fix.
- **Confirmed finding:** In the earlier exported models, vertex normals disagreed with the geometric face normals derived from triangle winding across all six variants/LODs. The normal vectors themselves had valid lengths, which concealed the problem during superficial checks.
- **Fix / prevention:** Inspect exported winding, face direction, vertex normals and mirrored transforms **for every variant and LOD**. Correct genuinely inconsistent export data rather than hiding the issue with brighter/darker textures, emission, forced two-sided shading or repeated shader normal flips. Preserve intentional foliage-normal art direction where valid.
- **Verification:** Check glTF normal-versus-face alignment, then inspect the imported asset in **Godot** with the sun in front, behind and to both sides, and at different elevations. Compare near/far LODs. **In-engine verification remains necessary for any newly authored asset; do not assume this historical finding proves current runtime acceptance.**

### VEG-002 — A spacing bucket must retain all accepted centres

**Area:** Vegetation / deterministic patch placement
**Affected:** `WORLD/Vegetation/Grass/GrassScatter.cs`
**Status:** Confirmed by source inspection; build and scene initialization verified

- **Symptom:** The configured minimum patch spacing was not guaranteed, allowing unintended tight overlapping growth areas.
- **Cause:** A spatial cell stored only one centre. Two points near opposite corners of a cell can be farther apart than the spacing and both be accepted; assigning the second overwrote the first. Later candidates could then miss that first neighbour.
- **Fix / prevention:** Store a list of all accepted centres per spatial cell and test every neighbour in the bounded adjacent cells. Do not assume a cell whose side equals the spacing can hold only one accepted point.
- **Verification:** Reviewed the corner-case geometry and new bucket/list traversal; .NET build and headless scatter initialization passed. Patch appearance/overdraw still needs local Godot review.

### BLDG-001 — Building scene cannot load when referenced normal-map imports are invalid

**Area:** Structures / PBR textures / Godot imported resources  
**Affected files:** `WORLD/Structures/SmallBuilding/Materials/*.tres`, `ASSETS/Material/{Floor,Wall}/**/textures/*_nor_gl_1k.exr.import`  
**Status:** Confirmed missing usable import output; underlying EXR import failure mechanism not diagnosed

- **Symptom:** Godot refuses to open `test.tscn` after instancing the building; missing dependencies name two EXR normal textures.
- **Cause:** Both referenced normal EXRs have checked-in `.import` records with `valid=false`, so the material's required `Texture2D` resources cannot be resolved. Source files existing in Git does not imply successful engine import.
- **Fix / prevention:** Remove the invalid normal-map references from the small building's two materials, retaining diffuse and already-imported roughness, and preserve original EXRs for later valid PNG conversion/reimport. Before adding texture paths to a gameplay-required scene, verify actual Godot import success; do not equate a correct repository path with a working engine resource.
- **Verification performed / still needed:** Inspected repository material paths and normal/roughness `.import` metadata; both normal maps have `valid=false` and roughness maps show generated cache paths. Reopen/import the project in Godot and inspect lighting to verify the fix; it is not yet locally rendered.

### VEG-003 — GPU particle start processors cannot exit with return

**Area:** Vegetation / Godot particle shader integration
**Affected:** WORLD/Vegetation/DemoGrass/Materials/forest_particles.gdshader
**Status:** Confirmed by Godot shader parser

- **Symptom:** Emitters exist but Godot reports shader compilation failure.
- **Cause:** Early return statements are invalid inside the start() processor, including after ACTIVE=false for rejection.
- **Fix / prevention:** Set ACTIVE=false; use a valid-region conditional around map sampling instead of processor returns. Avoid invalid array layers and nonfinite positions.
- **Verification:** Initial startup reported this exact parser error; corrected shader parsing and the focused scene check passed. Actual GPU rendering/performance remain local checks.

### VEG-004 — Required shader global is not registered when foliage shaders compile

**Area:** Vegetation / shader configuration / runtime colour controls  
**Affected:** PineNeedles.gdshader, both active forest grass shaders, project.godot  
**Status:** Godot error reported by owner; correction source-inspected, Godot retest pending

- **Symptom:** Godot reports `Global uniform "forest_tint" does not exist` and fails to compile the pine needle shader; both grass shaders share the same dependency.
- **Confirmed finding:** The shaders require `global uniform` declarations; the running Godot instance did not have the necessary named global registered at shader compilation. The repository had a `[shader_globals]` entry, but **why the running editor did not recognize it is not established** (for example, editor state after an external Git pull needs checking).
- **Fix / prevention:** Prefer ordinary ShaderMaterial uniforms when a finite set of shared foliage materials needs one UI palette. `ForestFoliagePalette.cs` now updates two pine needle and two forest grass material resources when F1 changes. Removes the project-global compile-time dependency and doesn't touch GLTF geometry, LOD, wind or per-instance batch processing.
- **Verification:** Reviewed all three shader declarations, four material paths and the single event-driven UI helper. Pull, reopen the Godot project, check for shader compilation errors and confirm F1 changes both pine needles and grass while bark stays unchanged. Runtime result remains owner-pending.

### SCENE-001 — Editor test scene must not reuse Sky3D without compatible initialization

**Area:** Terrain3D test scenes / editor-preview environment / Sky3D
**Affected:** `WORLD/Scenes/test2.tscn`
**Status:** Owner-confirmed Godot runtime error; correction source-reviewed, runtime retest pending

- **Symptom:** Playing Test 2 stopped inside `addons/sky_3d/src/Sky3D.gd` around line 545 with an invalid `sky.environment` assignment because `sky` was nil.
- **Cause / uncertainty:** Test 2 reused the Sky3D-based `environment.tscn` without the original test world's initialization/ownership. Sky3D's `_set("environment", ...)` assumes its child sky dome already exists. The precise initialization ordering that triggered this in the owner's editor is not yet independently reproduced.
- **Fix / prevention:** Test 2 uses its own plain `WorldEnvironment`, `ProceduralSkyMaterial` and `DirectionalLight3D` for preview instead of the runtime Sky3D scene, with a separate one-shot player spawn script. Keep authoring sandboxes independent of heavy environment controllers unless their complete setup contract is included, and preserve manually authored terrain materials and region data during repairs.
- **Verification:** Source-reviewed absence of Sky3D from Test 2; owner should reopen Godot, F6 play Test 2, verify the player spawns on terrain and original Test 1 remains unchanged.

## New entry template

### <AREA-###> — Short failure title

**Area:** Owning subsystem / any other affected systems  
**Affected files/assets:** Paths or components  
**Status:** Confirmed / partially verified / hypothesis  
**Reference:** Relevant README, issue or commit if available

- **Symptom:**
- **Cause (or uncertainty):**
- **Fix / prevention:**
- **Verification performed / still needed:**
