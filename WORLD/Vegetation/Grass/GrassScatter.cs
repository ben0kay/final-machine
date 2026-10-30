// Deterministic patchy forest-floor grass, grounded on saved Terrain3D and batched into two visual LODs.
using Godot;
using System;
using System.Collections.Generic;

public partial class GrassScatter : Node3D
{
    [Export] public bool RenderGrass { get; set; } = true;
    [Export] public GrassSettings Settings { get; set; }
    [Export] public NodePath SurfacePath { get; set; }
    [Export] public NodePath ForestPath { get; set; }
    private readonly Dictionary<Vector2I, List<Vector2>> _trunks = new();
    private const float TrunkCellSize = 4f;
    private readonly List<Vector2> _growthCenters = new();

    // Ferns build when this one-shot scatter completes, without adding a polling loop.
    public IReadOnlyList<Vector2> GrowthCenters => _growthCenters;
    public int PlacedTufts { get; private set; }
    public bool IsBuilt { get; private set; }
    public event Action ScatterFinished;

    private readonly struct GrassItem
    {
        public readonly Transform3D Transform;
        public readonly Color Tint;
        public GrassItem(Transform3D transform, Color tint) { Transform = transform; Tint = tint; }
    }

    #region Placement
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
        var detailNoise = new FastNoiseLite { Seed = Settings.Seed + 903, Frequency = 0.21f };
        var centers = new Dictionary<Vector2I, List<Vector2>>();
        _growthCenters.Clear();
        PlacedTufts = 0;
        IsBuilt = false;
        CacheTrunks();
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

        for (int i = 0; i < Settings.PatchAttempts && tuftCount < Settings.MaximumTufts; i++)
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
                if (centers.TryGetValue(cell + new Vector2I(dx, dz), out var others))
                    foreach (var other in others)
                        if (center.DistanceSquaredTo(other) < spacing * spacing) { blocked = true; break; }
            if (blocked) continue;

            Vector4 centerSample = surface.Call("grass_sample", x, z).AsVector4();
            if (!Suitable(centerSample) || random.Randf() > centerSample.Z) continue;

            if (!centers.TryGetValue(cell, out var points)) centers[cell] = points = new();
            points.Add(center);
            bool isolated = random.Randf() < Settings.IsolatedPatchFraction;
            float radius = isolated ? 0.7f : random.RandfRange(minRadius, maxRadius);
            float stretch = random.RandfRange(0.55f, 1.5f);
            float patchAngle = random.RandfRange(0f, Mathf.Tau);
            float patchTint = random.RandfRange(1f - Settings.TintVariation, 1f + Settings.TintVariation);
            float size = Mathf.Clamp((radius - minRadius) / Mathf.Max(0.1f, maxRadius - minRadius), 0f, 1f);
            int count = isolated ? random.RandiRange(1, 3) : Mathf.RoundToInt(
                Mathf.Lerp(minTufts, maxTufts, size) * random.RandfRange(0.82f, 1.15f));
            count = Mathf.Min(count, maxTufts);
            // Sample the whole growth area, not a few dense cores that look like isolated shrubs.
            // Two smooth lobes break the circular outline while neighboring patches may overlap.
            float shapePhase = random.RandfRange(0f, Mathf.Tau);
            var accepted = new List<Vector2>();
            int added = 0;
            for (int j = 0; j < count * 3 && added < count && tuftCount < Settings.MaximumTufts; j++)
            {
                float angle = random.RandfRange(0f, Mathf.Tau);
                float lobe = Mathf.Clamp(0.88f + 0.17f * Mathf.Sin(angle * 3f + shapePhase)
                    + 0.10f * Mathf.Sin(angle * 5f - shapePhase * 0.7f), 0.55f, 1.15f);
                float distance = Mathf.Sqrt(random.Randf()) * radius * lobe;
                var offset = new Vector2(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance * stretch)
                    .Rotated(patchAngle);
                float gx = x + offset.X, gz = z + offset.Y;
                if (Mathf.Abs(gx) > 470f || Mathf.Abs(gz) > 470f) continue;
                var point = new Vector2(gx, gz);
                bool crowded = false;
                foreach (var other in accepted)
                    if (point.DistanceSquaredTo(other) < Settings.MinimumTuftSpacing * Settings.MinimumTuftSpacing) { crowded = true; break; }
                if (crowded || NearTrunk(point)) continue;
                float localDensity = detailNoise.GetNoise2D(gx, gz);
                if (localDensity < -0.23f || surface.Call("woodland", gx, gz).AsSingle() < Settings.MinimumWoodland) continue;
                Vector4 sample = surface.Call("grass_sample", gx, gz).AsVector4();
                if (!Suitable(sample) || random.Randf() > sample.Z) continue;
                // Slightly richer suitable banks, with exposed channel centres and occasional internal gaps.
                if (random.Randf() > Mathf.Clamp(0.8f + localDensity * 0.3f + sample.W * 0.18f, 0f, 1f)) continue;
                float height = sample.X;
                accepted.Add(point);
                added++;

                int variant = (int)random.RandiRange(0, 2);
                // Unequal short/medium/tall distribution keeps nearby tufts from reading as clones.
                float heightTier = random.Randf();
                float scaleT = heightTier < 0.32f ? random.RandfRange(0f, 0.36f) :
                    heightTier < 0.86f ? random.RandfRange(0.30f, 0.78f) : random.RandfRange(0.75f, 1f);
                float scale = Mathf.Lerp(minScale, maxScale, scaleT);
                float width = scale * random.RandfRange(Settings.MinimumWidth, Settings.MaximumWidth);
                Vector3 normal = surface.Call("surface_normal", gx, gz).AsVector3();
                Vector3 up = Vector3.Up.Lerp(normal, Settings.NormalAlignment).Normalized();
                var tilt = new Basis(new Quaternion(Vector3.Up, up));
                var basis = (tilt * new Basis(Vector3.Up, random.RandfRange(0f, Mathf.Tau)))
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
            if (added > 0) _growthCenters.Add(center);
        }

        _trunks.Clear();
        // Keep existing habitat/count metadata for FernScatter while disabling all legacy grass draws.
        if (RenderGrass)
        {
            var material = GD.Load<ShaderMaterial>(
                "res://WORLD/Vegetation/Grass/Materials/ForestFloorGrass.tres");
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
        }
        PlacedTufts = tuftCount;
        IsBuilt = true;
        GD.Print("GrassScatter: " + _growthCenters.Count + " populated patches, " + tuftCount +
            " tufts across " + batches.Count + " cells; render=" + RenderGrass + "; no grass process callbacks.");
        ScatterFinished?.Invoke();
    }

    // =========================================================
    private bool Suitable(Vector4 sample) => float.IsFinite(sample.X) && sample.X <= Settings.MaximumHeight &&
        sample.Y >= Settings.MinimumSurfaceUp && sample.Z > 0;

    // =========================================================
    // Forest builds first in this scene. Read its actual kept trunk proxies once, without modifying pine RNG.
    private void CacheTrunks()
    {
        var forest = GetNodeOrNull<Node3D>(ForestPath);
        if (forest == null) { GD.PushWarning("GrassScatter: no forest bound; trunk exclusion is unavailable."); return; }
        foreach (var child in forest.GetChildren())
            if (child is StaticBody3D body)
                foreach (var proxy in body.GetChildren())
                    if (proxy is CollisionShape3D shape)
                    {
                        Vector3 local = ToLocal(shape.GlobalPosition);
                        var position = new Vector2(local.X, local.Z);
                        var cell = new Vector2I(Mathf.FloorToInt(local.X / TrunkCellSize), Mathf.FloorToInt(local.Z / TrunkCellSize));
                        if (!_trunks.TryGetValue(cell, out var trunks)) _trunks[cell] = trunks = new();
                        trunks.Add(position);
                    }
    }

    // =========================================================
    private bool NearTrunk(Vector2 position)
    {
        var cell = new Vector2I(Mathf.FloorToInt(position.X / TrunkCellSize), Mathf.FloorToInt(position.Y / TrunkCellSize));
        int reach = Mathf.CeilToInt(Settings.TrunkClearance / TrunkCellSize);
        for (int dz = -reach; dz <= reach; dz++)
        for (int dx = -reach; dx <= reach; dx++)
            if (_trunks.TryGetValue(cell + new Vector2I(dx, dz), out var trunks))
                foreach (var trunk in trunks)
                    if (position.DistanceSquaredTo(trunk) < Settings.TrunkClearance * Settings.TrunkClearance) return true;
        return false;
    }
    #endregion

    #region Rendering
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
    #endregion
}
