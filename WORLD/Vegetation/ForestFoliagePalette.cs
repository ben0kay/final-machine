// Apply one editable forest palette to shared pine and grass materials, without shader globals.
using Godot;

public static class ForestFoliagePalette
{
    public static readonly Color DefaultTint = new(0.43f, 0.55f, 0.37f, 1f);
    public const float DefaultStrength = 0.25f;

    private static readonly string[] MaterialPaths = {
        "res://ASSETS/Vegetation/Trees/PineTree002/Materials/PineNeedlesNear.tres",
        "res://ASSETS/Vegetation/Trees/PineTree002/Materials/PineNeedlesLod3.tres",
        "res://ASSETS/Vegetation/Trees/PineTree001/Materials/PineTree001Foliage.tres",
        "res://WORLD/Vegetation/Grass/Materials/ForestFloorGrass.tres",
        "res://WORLD/Vegetation/Grass/Materials/ForestGrass002Filler.tres"
    };
    private static ShaderMaterial[] _materials;

    // =========================================================
    // Load shared resources once. All MultiMesh instances reuse these same materials.
    // This does not traverse plants, touch model geometry or update every frame.
    public static void Apply(Color tint, float strength)
    {
        if (_materials == null)
        {
            _materials = new ShaderMaterial[MaterialPaths.Length];
            for (int i = 0; i < MaterialPaths.Length; i++)
            {
                _materials[i] = GD.Load<ShaderMaterial>(MaterialPaths[i]);
                if (_materials[i] == null)
                    GD.PushWarning("ForestFoliagePalette: material unavailable: " + MaterialPaths[i]);
            }
        }

        foreach (var material in _materials)
        {
            if (material == null) continue;
            material.SetShaderParameter("forest_tint", tint);
            material.SetShaderParameter("forest_tint_strength", Mathf.Clamp(strength, 0f, 1f));
        }
    }
}
