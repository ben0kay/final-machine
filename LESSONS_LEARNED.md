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
