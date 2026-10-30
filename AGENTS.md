# AI Agent Instructions — Final Machine

**Applies to:** `ben0kay/final-machine` (the current working branch, normally `main`). These instructions are a **short entry point**, not a replacement for existing project documentation.

## Before making changes

1. **Read `CodePreferences.md` in full.** Its code style, `#region` conventions, world-root structure, performance rules, task scope, Git workflow and **minimal/proportional validation preference** all remain authoritative. Do not abbreviate, silently reinterpret or replace them.
2. **Read the root `README.md`** to understand the current game direction, technical foundation and implemented-vs-planned state.
3. **Read the owning feature's documentation before modifying it** (for example `WORLD/Terrain/README.md`, `WORLD/Environment/README.md`, `WORLD/Vegetation/README.md`, `PLAYER/README.md`, or `DEBUG/Overlay/README.md`), plus any relevant asset README and authoring guide. Inspect the actual scene/scripts/resources too; documentation can be outdated.
4. **For any tree, grass, plant, rock or other natural asset creation/modification, first read `ASSETS/Vegetation/VISUAL_AUTHORING_GUIDE.md`.** Apply its geometry, triangle/LOD, material, winding/normals/lighting, botanical plausibility, export and acceptance checks. Read the specific asset's README as well.
5. Use `terra-machine` and `war-machine` **only as references** when useful or requested. This repository is **Terrain3D-first**; don't port Voxel Tools code or inherit another repository's branch policy.

## While implementing

- Work **only on the authorized task**. Preserve functioning systems, user-authored assets, validated optimizations, stable paths and unrelated changes.
- Follow existing folder ownership, root scene containers, reusable scenes, configuration resources and performance-conscious patterns. Don't introduce unnecessary managers, processing loops or whole-system rewrites.
- **Fix causes, not cosmetic symptoms.** Before retrying a problem previously solved elsewhere, consult the documented lessons and checks. When a guide and current code differ, inspect both and report a material conflict rather than blindly overriding either.

## Validation, commits and handoff

- **Save Codex time:** do brief, focused source checks and a basic build/essential check when practical. **Do not spend ages on repeated builds, extensive test suites, automated visual validation or performance profiling unless explicitly asked.** The owner performs most in-Godot visual and FPS acceptance testing. State clearly what was checked and what remains unverified.
- Follow `CodePreferences.md` for Git authorization and branch rules. Prefer **one logical commit per authorized job**, and push when authorized; do not make extra unrelated commits.
- Update the owning README/authoring guide **when the task introduces a lasting design rule or resolves a significant recurring mistake**. Record the symptom, verified root cause, prevention rule, and short verification step; separate confirmed results from assumptions. Do not turn an unverified guess into a permanent requirement.
- In your handoff, briefly report changes, commit SHA if applicable, checks performed, and the specific visual/runtime checks left to the owner.

**Principle:** Read the lessons first, make focused changes, check just enough, and preserve what already works.
