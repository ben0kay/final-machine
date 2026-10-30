# AI Agent Instructions — Final Machine

**Repo:** `ben0kay/final-machine` · branch `main` unless the owner specifies otherwise. This is the **entry point**; it does not override or replace the referenced documents.

## Before implementation — including long ChatGPT conversations

At the start of each **new substantial task**, or whenever asked to “read AGENTS.md,” **refresh the current repository files**, even if this chat read them earlier. Do this quickly; don't launch broad repository analysis or tests just to refresh instructions:

1. **`CodePreferences.md` — read fully.** Authoritative C# style (`#region`, comments, compact layout), scene-root ownership, architecture, performance, Git and **minimal-testing** rules.
2. **Root `README.md`** — project vision, architecture and implemented vs. planned features.
3. **Root `LESSONS_LEARNED.md`** — known mistakes, fixes and prevention rules; single central lessons file.
4. **Owning feature README/plan, authoring guide, asset README and actual code/scene/resources** relevant to the task. For trees, grass, plants and natural rocks, **always read `ASSETS/Vegetation/VISUAL_AUTHORING_GUIDE.md`** (LODs, budgets, anatomy, winding/normals, lighting, materials, exports).

Use `terra-machine` and `war-machine` only as references when useful; Final Machine is **Terrain3D-first**. Never inherit their branch policies or old Voxel Tools architecture.

## Working rules

- Implement **only the authorized scope**; preserve user-authored assets, working behaviour, optimizations, unrelated changes and stable scene paths. Follow existing folder ownership, root scene containers, resources and reusable scenes. Avoid gratuitous managers, loops and rewrites.
- Consult past lessons; **fix root causes rather than masking symptoms**. If docs conflict with current code, inspect and flag material discrepancies.
- **Keep validation brief:** focused source review and one basic build/essential check when practical. **No extensive/repeated tests, visual automation or FPS profiling unless requested.** The owner handles most in-Godot visual/performance checks. Clearly distinguish verified from unverified.
- Follow CodePreferences for Git scope/authorization. Prefer **one logical commit** and push when authorized; avoid unrelated commits. Report changes and commit SHA.
- **Maintain only root `LESSONS_LEARNED.md`:** after resolving a significant/reusable mistake, document symptom, confirmed cause (or uncertainty), fix, prevention and a short verification step **in the same task**. Skip trivial typos, routine tweaks, guesses and duplicates; do not create folder-level lessons files without permission.
- Update a feature README/authoring guide only when enduring design, usage or configuration guidance changes; link to central lessons rather than duplicating lengthy reports.
- Keep handoffs short: changes, commit, checks actually run and any visual/runtime checks left to the owner.

**Reminder:** Reading `AGENTS.md` in an ongoing chat means reread these linked instructions from the repository **before** implementing—not merely acknowledge having read them earlier.
