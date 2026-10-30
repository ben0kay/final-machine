# War Machine terrain ground — preserved Terrain3D materials

This is a **copy**, not a move, of the terrain textures used by `ben0kay/war-machine`, branch `implement-td0-td1`, in `WORLD/Terrain/TerrainAssets.tres` and `WORLD/Terrain/TerrainMaterial.tres` as inspected on 2026-10-30.

It is a **standalone, inactive art reference**. Nothing in Final Machine's active `WORLD/Terrain/Configuration/TerrainAssets.tres`, `WORLD/Terrain/Integration/Terrain3D/terrain.tscn`, native painted regions, grass systems or forest has been changed.

## What is included

- `Textures/ground037_alb_ht.png` and `ground037_nrm_rgh.png`: Ground037 packed Terrain3D grass/earth maps (albedo+height and normal+roughness).
- `Textures/rock023_alb_ht.png` and `rock023_nrm_rgh.png`: Rock023 packed cliff/rock maps.
- `WarMachineTerrainAssets.tres`: matching standalone two-texture Terrain3D assets. Slot 0 = Cliff, slot 1 = Grass, as in War Machine. The **disabled legacy grass mesh** from the War Machine asset file is intentionally omitted; it is not part of the terrain ground look.
- `WarMachineTerrainMaterial.tres`: copied Terrain3D autoshader settings, including macro variation, noise, projection and dual scaling. Resource UID declarations have been removed so the copy receives its own Godot identity.
- `SOURCE_LICENSE.txt`: upstream ambientCG CC0 notice.

**Visual configuration behind the original appearance:** Grass slot 1 uses `albedo_color=Color(0.67451, 0.74902, 0.686275, 1)`, `uv_scale=0.2`, `detiling_rotation=0.161`, AO 2.0 and normal depth 1.0. Slope-based cliff/grass blending uses auto shader base texture 0 (Rock023), overlay texture 1 (Ground037), `auto_slope=0.45`; material uses macro colour variation, positive mipmap bias and dual scaling. The gentle, muted look comes from **the maps plus configuration**, not from a custom procedural ground shader.

## Using it later in Final Machine

Do **not** overwrite the active `TerrainAssets.tres` with this file: current Final Machine slot IDs are 0 Meadow / 1 Woodland / 2 Rock, while this reference set uses 0 Cliff / 1 Grass. Existing painted texture indices and woodland/rock habitat tests would no longer mean the same thing if swapped wholesale.

For a visual test, create a separate Terrain3D test scene/material or import the copied **Ground037 packed maps** into a new terrain texture slot and carefully adapt the active texture-ID/habitat references. Consider copying the tint/UV/macro settings rather than replacing the whole terrain material. Keep the existing source native region files intact.

Source art is unchanged byte-for-byte, preserved via original Git blob SHA. Godot rendering, import and FPS have **not** been locally verified; pulling the commit should trigger the image imports.

## Upstream / license

- Original source: `ben0kay/war-machine@implement-td0-td1/demo/assets/textures/`
- Terrain3D reference settings: `WORLD/Terrain/TerrainAssets.tres` and `TerrainMaterial.tres`
- Ground037: https://ambientcg.com/view?id=Ground037
- Rock023: https://ambientcg.com/view?id=Rock023
- CC0 1.0 Universal; see `SOURCE_LICENSE.txt`.
