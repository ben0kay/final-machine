# Explicit offline authoring command; refuses to replace existing terrain regions.
extends SceneTree
const Landforms = preload("res://WORLD/Terrain/Landscape/landforms.gd")
const DIRECTORY = "res://WORLD/Terrain/Landscape/Regions"
func _initialize() -> void:
    call_deferred("bake")
func bake() -> void:
    for file in DirAccess.get_files_at(DIRECTORY):
        if file.ends_with(".res"):
            push_error("Saved terrain exists. Bake cancelled to protect authored edits. Use a separate output directory for a new landscape.")
            quit(1)
            return
    var terrain := Terrain3D.new()
    terrain.region_size = 256
    terrain.collision_mode = 0
    root.add_child(terrain)
    var land = Landforms.new()
    var height_bytes := PackedByteArray()
    var control_bytes := PackedByteArray()
    height_bytes.resize(1024*1024*4)
    control_bytes.resize(1024*1024*4)
    for z in range(1024):
        for x in range(1024):
            var wx := float(x-512)
            var wz := float(z-512)
            var h: float = land.sample(wx,wz)
            var dx: float = (land.sample(wx+1,wz)-land.sample(wx-1,wz))*0.5
            var dz: float = (land.sample(wx,wz+1)-land.sample(wx,wz-1))*0.5
            var slope := sqrt(dx*dx+dz*dz)
            var rock := smoothstep(0.42,0.95,slope)
            var forest: float = land.woodland(wx,wz)
            var base := 1 if forest > 0.45 else 0
            var overlay := 2 if rock > 0.02 else 1-base
            var weight := rock if rock > 0.02 else (forest if base == 0 else 1.0-forest)
            var bits := (base<<27)|(overlay<<22)|(int(clampf(weight,0,1)*255)<<14)
            var offset := (z*1024+x)*4
            height_bytes.encode_float(offset,h)
            control_bytes.encode_u32(offset,bits)
    var heights := Image.create_from_data(1024,1024,false,Image.FORMAT_RF,height_bytes)
    var controls := Image.create_from_data(1024,1024,false,Image.FORMAT_RF,control_bytes)
    var images: Array[Image] = [heights,controls,null]
    terrain.data.import_images(images,Vector3(-512,0,-512),0,1)
    terrain.data.save_directory(DIRECTORY)
    print("FM0: saved 1024m landscape as native editable regions")
    quit()
