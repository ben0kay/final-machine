// Deterministic patchy forest-floor grass, grounded on saved Terrain3D and batched into two visual LODs.
using Godot;
using System;
using System.Collections.Generic;

public partial class GrassScatter : Node3D
{
    [Export] public GrassSettings Settings { get; set; }
    [Export] public NodePath SurfacePath { get; set; }

    private readonly struct GrassItem
    {
        public readonly Transform3D Transform;
        public readonly Color Tint;
        public GrassItem(Transform3D transform, Color tint) { Transform = transform; Tint = tint; }
    }

    public override void _Ready() => CallDeferred(nameof(Build));

    // =========================================================
    // One-time randomized patches: broad coherent noise, irregular ellipses, and separate tuft jitter.
    // Saved terrain height/normal and the woodland composition mask are queried via the existing bridge.
    private void Build()
    {
        Settings ??= new GrassSettings();
        var surface = GetNodeOrNull(SurfacePath);
        if (surface == null) { GD.PushError("GrassScatter: Terrain3D surface node not found."); return; }

        var random = new RandomNumberGenerator { Seed = (ulong)Settings.Seed };
        var patchNoise = new FastNoiseLite { Seed = Settings.Seed + 701, Frequency = 0.025f };
        var centers = new Dictionary<Vector2I, Vector2>();
        var batches = new Dictionary<Vector2I, List<GrassItem>[]>();
        float spacing = Mathf.Max(1f, Settings.MinimumPatchSpacing);
        float batchSize = Mathf.Max(8f, Settings.BatchSize);
        float minRadius = Mathf.Max(0.1f, Mathf.Min(Settings.MinimumPatchRadius, Settings.MaximumPatchRadius));
        float maxRadius = Mathf.Max(minRadius, Mathf.Max(Settings.MinimumPatchRadius, Settings.MaximumPatchRadius));
        float minScale = Mathf.Max(0.05f, Mathf.Min(Settings.MinimumScale, Settings.MaximumScale));
        float maxScale = Mathf.Max(minScale, Mathf.Max(Settings.MinimumScale, Settings.MaximumScale));
        int minTufts = Mathf.Max(1, Mathf.Min(Settings.MinimumTuftsPerPatch, Settings.MaximumTuftsPerPatch));
        int maxTufts = Mathf.Max(minTufts, Mathf.Max(Settings.MinimumTuftsPerPatch, Settings.MaximumTuftsPerPatch));
        int tuftCount = 0;

        for (int i = 0; i < Settings.PatchAttempts; i++)
        {
            float x = random.RandfRange(-470f, 470f), z = random.RandfRange(-470f, 470f);
            float woodland = surface.Call("woodland", x, z).AsSingle();
            if (woodland < Settings.MinimumWoodland) continue;

            float noise = 0.5f + 0.5f * patchNoise.GetNoise2D(x, z);
            if (noise <= Settings.NoiseThreshold) continue;
            float noiseStrength = Mathf.SmoothStep(0f, 1f,
                Mathf.Clamp((noise - Settings.NoiseThreshold) / Mathf.Max(0.01f, 1f - Settings.NoiseThreshold), 0f, 1f));
            float woodlandStrength = Mathf.SmoothStep(0f, 1f,
                Mathf.Clamp((woodland - Settings.MinimumWoodland) / Mathf.Max(0.01f, 1f - Settings.MinimumWoodland), 0f, 1f));
            if (random.Randf() > Settings.PatchDensity * noiseStrength * woodlandStrength) continue;

            var center = new Vector2(x, z);
            var cell = new Vector2I(Mathf.FloorToInt(x / spacing), Mathf.FloorToInt(z / spacing));
            bool blocked = false;
            for (int dz = -1; dz <= 1 && !blocked; dz++)
            for (int dx = -1; dx <= 1; dx++)
                if (centers.TryGetValue(cell + new Vector2I(dx, dz), out var other) &&
                    center.DistanceSquaredTo(other) < spacing * spacing) { blocked = true; break; }
            if (blocked) continue;

            float centerHeight = surface.Call("surface_height", x, z).AsSingle();
            float centerUp = surface.Call("surface_up", x, z).AsSingle();
            if (!float.IsFinite(centerHeight) || centerHeight > Settings.MaximumHeight ||
                centerUp < Settings.MinimumSurfaceUp) continue;

            centers[cell] = center;
            float radius = random.RandfRange(minRadius, maxRadius);
            float stretch = random.RandfRange(0.72f, 1.3f);
            float patchAngle = random.RandfRange(0f, Mathf.Tau);
            float patchTint = random.RandfRange(1f - Settings.TintVariation, 1f + Settings.TintVariation);
            int count = random.RandiRange(minTufts, maxTufts);

            for (int j = 0; j < count; j++)
            {
                // Uniform area distribution in an elongated patch, then small irregular displacement.
                float distance = Mathf.Sqrt(random.Randf()) * radius;
                float angle = random.RandfRange(0f, Mathf.Tau);
                var offset = new Vector2(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance * stretch).Rotated(patchAngle);
                float gx = x + offset.X, gz = z + offset.Y;
                if (Mathf.Abs(gx) > 470f || Mathf.Abs(gz) > 470f) continue;
                if (surface.Call("woodland", gx, gz).AsSingle() < Settings.MinimumWoodland) continue;

                float height = surface.Call("surface_height", gx, gz).AsSingle();
                if (!float.IsFinite(height) || height > Settings.MaximumHeight ||
                    surface.Call("surface_up", gx, gz).AsSingle() < Settings.MinimumSurfaceUp) continue;

                int variant = (int)random.RandiRange(0, 2);
                float scale = random.RandfRange(minScale, maxScale);
                float width = scale * random.RandfRange(0.9f, 1.1f);
                var basis = new Basis(Vector3.Up, random.RandfRange(0f, Mathf.Tau))
                    .Scaled(new Vector3(width, scale, width));
                var batch = new Vector2I(Mathf.FloorToInt(gx / batchSize), Mathf.FloorToInt(gz / batchSize));
                if (!batches.TryGetValue(batch, out var variants))
                    batches[batch] = variants = new[] { new List<GrassItem>(), new List<GrassItem>(), new List<GrassItem>() };

                // MultiMesh instance colors multiply imported vertex colors in the shared wind shader.
                float tone = patchTint + random.RandfRange(-Settings.TintVariation * 0.35f, Settings.TintVariation * 0.35f);
                var tint = new Color(tone * 0.99f, tone * 1.01f, tone * 0.98f, 1f);
                var local = new Vector3(gx - batch.X * batchSize, height - 0.035f, gz - batch.Y * batchSize);
                variants[variant].Add(new GrassItem(new Transform3D(basis, local), tint));
                tuftCount++;
            }
        }

        var material = GD.Load<ShaderMaterial>(
            "res://ASSETS/Vegetation/Grass/ForestGrass001/Materials/ForestGrass001Wind_DistanceFade.tres");
        if (material == null) { GD.PushError("GrassScatter: ForestGrass001 wind material unavailable."); return; }

        for (int lod = 0; lod < 2; lod++)
        for (int variant = 0; variant < 3; variant++)
        {
            char letter = (char)('a' + variant);
            string path = "res://ASSETS/Vegetation/Grass/ForestGrass001/Models/forest_grass_001_" +
                letter + "_LOD" + (lod + 1) + ".gltf";
            var packed = GD.Load<PackedScene>(path);
            if (packed == null) { GD.PushError("GrassScatter: Missing model " + path); continue; }
            var source = packed.Instantiate<Node3D>();
            foreach (var entry in batches)
                if (entry.Value[variant].Count > 0)
                    AddMeshes(source, Transform3D.Identity, entry.Key, entry.Value[variant], batchSize, lod, material);
            source.Free();
        }
        GD.Print("GrassScatter: " + centers.Count + " patches, " + tuftCount +
            " tufts across " + batches.Count + " cells; no grass process callbacks.");
    }

    // =========================================================
    // One MultiMesh per source mesh / variant / LOD / spatial cell, with subtle per-instance tints.
    private void AddMeshes(Node3D node, Transform3D parent, Vector2I cell, List<GrassItem> grass,
        float batchSize, int lod, ShaderMaterial material)
    {
        Transform3D local = parent * node.Transform;
        if (node is MeshInstance3D source && source.Mesh != null)
        {
            var multi = new MultiMesh {
                TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
                UseColors = true,
                Mesh = source.Mesh,
                InstanceCount = grass.Count
            };
            float minY = float.PositiveInfinity, maxY = float.NegativeInfinity;
            for (int i = 0; i < grass.Count; i++)
            {
                multi.SetInstanceTransform(i, grass[i].Transform * local);
                multi.SetInstanceColor(i, grass[i].Tint);
                minY = Mathf.Min(minY, grass[i].Transform.Origin.Y);
                maxY = Mathf.Max(maxY, grass[i].Transform.Origin.Y);
            }
            multi.CustomAabb = new Aabb(
                new Vector3(-2f, minY - 2f, -2f),
                new Vector3(batchSize + 4f, maxY - minY + 5f, batchSize + 4f));
            float split = Mathf.Min(Settings.NearDistance, Settings.CullDistance - 1f);
            var instance = new MultiMeshInstance3D {
                Multimesh = multi,
                MaterialOverride = material,
                Position = new Vector3(cell.X * batchSize, 0f, cell.Y * batchSize),
                VisibilityRangeBegin = lod == 0 ? 0f : split,
                VisibilityRangeEnd = lod == 0 ? split : Settings.CullDistance,
                VisibilityRangeBeginMargin = 2f,
                VisibilityRangeEndMargin = 2f,
                CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
            };
            AddChild(instance);
        }
        foreach (var child in node.GetChildren())
            if (child is Node3D child3D)
                AddMeshes(child3D, local, cell, grass, batchSize, lod, material);
    }
}
