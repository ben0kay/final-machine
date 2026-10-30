# Blender 4.x source-project creator for ForestGrass002.
# Run in Blender Scripting, or: blender --background --python blender_import_and_save.py
# It imports all six glTF meshes, preserving each card's edited geometry/UV and PBR atlas.
import bpy
from pathlib import Path
base = Path(__file__).resolve().parents[1]
bpy.ops.wm.read_factory_settings(use_empty=True)
for letter in "abc":
    for lod in (1,2):
        path = base / "Models" / f"forest_grass_002_{letter}_LOD{lod}.gltf"
        bpy.ops.import_scene.gltf(filepath=str(path))
        for obj in bpy.context.selected_objects:
            if obj.type == 'MESH':
                obj.name = f"ForestGrass002_{letter.upper()}_LOD{lod}"
                # The importer's texture node is already linked to the atlas.
                obj.hide_render = False
out = base / "Source" / "forest_grass_002_source.blend"
bpy.ops.wm.save_as_mainfile(filepath=str(out))
print("Saved editable Blender file:", out)
