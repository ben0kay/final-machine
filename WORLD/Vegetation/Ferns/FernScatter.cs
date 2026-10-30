// Ferns share populated grass patches but have independent one-shot distribution and GPU batching.
using Godot;
using System;
using System.Collections.Generic;

public partial class FernScatter : Node3D
{
    [Export] public FernSettings Settings { get; set; }
    [Export] public NodePath SurfacePath { get; set; }
    [Export] public NodePath GrassPath { get; set; }
    [Export] public NodePath ForestPath { get; set; }

    private GrassScatter _grass;
    private bool _built;
    private const float TrunkCellSize = 4f;
    private readonly Dictionary<Vector2I, List<Vector2>> _trunks = new();

    private readonly struct FernItem
    {
        public readonly Transform3D Transform;
        public readonly Color Tint;
        public FernItem(Transform3D transform, Color tint) { Transform = transform; Tint = tint; }
    }

    #region Placement
    public override void _Ready()
    {
        _grass = GetNodeOrNull<GrassScatter>(GrassPath);
        if (_grass == null) { GD.PushWarning("FernScatter: GrassScatter was not found."); return; }
        if (_grass.IsBuilt) CallDeferred(nameof(Build));
        else _grass.ScatterFinished += OnGrassReady;
    }

    public override void _ExitTree()
    {
        if (_grass != null) _grass.ScatterFinished -= OnGrassReady;
    }

    // =========================================================
    // The grass owner's completion event avoids per-frame polling or build-order assumptions.
    private void OnGrassReady() => CallDeferred(nameof(Build));

    private void Build()
    {
        if (_built) return;
        _built = true;
        _grass.ScatterFinished -= OnGrassReady;
        Settings ??= new FernSettings();

        var surface = GetNodeOrNull(SurfacePath);
        if (surface == null) { GD.PushError("FernScatter: Terrain3D surface unavailable."); return; }
        if (_grass.GrowthCenters.Count == 0 || _grass.PlacedTufts <= 0)
        {
            GD.Print("FernScatter: No populated grass patches; no ferns placed."); return;
        }

        int target = Mathf.Min(Settings.MaximumFerns,
            Mathf.FloorToInt(_grass.PlacedTufts / Mathf.Max(1f, Settings.GrassToFernRatio)));
        var random = new RandomNumberGenerator { Seed = (ulong)Settings.Seed };
        var potential = new List<Vector2>();
        foreach (var center in _grass.GrowthCenters)
            if (random.Randf() < Settings.FernPatchFraction) potential.Add(center);
        if (potential.Count == 0) { GD.Print("FernScatter: No suitable fern growth patches."); return; }

        // The imported GLTF contains four distinct mesh nodes; their 1m sheet-layout
        // translations are intentionally not applied to scattered ground positions.
        var packed = GD.Load<PackedScene>("res://ASSETS/Vegetation/Plants/Ferns001/Models/ferns00`.gltf");
        var material = GD.Load<ShaderMaterial>("res://ASSETS/Vegetation/Plants/Ferns001/Materials/FernWind.tres");
        if (packed == null || material == null)
        {
            GD.PushError("FernScatter: Ferns001 model or FernWind material could not load."); return;
        }
        var source = packed.Instantiate<Node3D>();
        var meshes = new Mesh[4];
        for (int i = 0; i < meshes.Length; i++)
        {
            char variant = (char)('a' + i);
            if (source.FindChild("fern_02_" + variant, true, false) is MeshInstance3D node) meshes[i] = node.Mesh;
            else GD.PushWarning("FernScatter: Missing fern variant " + variant);
        }
        if (Array.TrueForAll(meshes, mesh => mesh == null))
        {
            GD.PushError("FernScatter: No usable fern mesh found.");
            source.Free();
            return;
        }

        CacheTrunks();
        var buckets = new Dictionary<Vector2I, List<Vector2>>();
        var batches = new Dictionary<Vector2I, List<FernItem>[]>();
        float spacing = Mathf.Max(0.5f, Settings.MinimumSpacing);
        float batchSize = Mathf.Max(8f, Settings.BatchSize);
        float minScale = Mathf.Max(0.05f, Mathf.Min(Settings.MinimumScale, Settings.MaximumScale));
        float maxScale = Mathf.Max(minScale, Mathf.Max(Settings.MinimumScale, Settings.MaximumScale));
        int placed = 0;

        // Some grass patches contain clusters of ferns; others remain fern-free.
        for (int attempt = 0; attempt < target * Settings.AttemptsPerFern && placed < target; attempt++)
        {
            Vector2 anchor = potential[random.RandiRange(0, potential.Count - 1)];
            float angle = random.RandfRange(0f, Mathf.Tau);
            float distance = Mathf.Sqrt(random.Randf()) * Settings.ScatterRadius;
            Vector2 point = anchor + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
            if (Mathf.Abs(point.X) > 470f || Mathf.Abs(point.Y) > 470f || NearTrunk(point)) continue;

            float woodland = surface.Call("woodland", point.X, point.Y).AsSingle();
            if (woodland < Settings.MinimumWoodland) continue;
            Vector4 sample = surface.Call("grass_sample", point.X, point.Y).AsVector4();
            if (!float.IsFinite(sample.X) || sample.X > Settings.MaximumHeight ||
                sample.Y < Settings.MinimumSurfaceUp || sample.Z <= 0.12f) continue;
            if (random.Randf() > Mathf.Clamp(sample.Z * (0.78f + sample.W * 0.18f), 0f, 1f)) continue;

            // Spacing buckets retain all accepted points (see VEG-002), not just the last.
            var cell = new Vector2I(Mathf.FloorToInt(point.X / spacing), Mathf.FloorToInt(point.Y / spacing));
            bool crowded = false;
            for (int dz = -1; dz <= 1 && !crowded; dz++)
            for (int dx = -1; dx <= 1 && !crowded; dx++)
                if (buckets.TryGetValue(cell + new Vector2I(dx, dz), out var neighbors))
                    foreach (var other in neighbors)
                        if (point.DistanceSquaredTo(other) < spacing * spacing) { crowded = true; break; }
            if (crowded) continue;

            // Low-cost A/D (~800 tris) dominate; fuller B/C (~2.3k tris) appear occasionally.
            int variant = random.Randf() < Settings.DetailedVariantFraction
                ? random.RandiRange(1, 2) : (random.Randf() < 0.5f ? 0 : 3);
            if (meshes[variant] == null) continue;
            if (!buckets.TryGetValue(cell, out var bucket)) buckets[cell] = bucket = new();
            bucket.Add(point);

            float scale = random.RandfRange(minScale, maxScale);
            float width = scale * random.RandfRange(Settings.MinimumWidth, Settings.MaximumWidth);
            var basis = new Basis(Vector3.Up, random.RandfRange(0f, Mathf.Tau))
                .Scaled(new Vector3(width, scale, width));
            float tone = random.RandfRange(1f - Settings.TintVariation, 1f + Settings.TintVariation);
            var tint = new Color(tone * 0.98f, tone * 1.01f, tone * 0.98f, 1f);
            var batch = new Vector2I(Mathf.FloorToInt(point.X / batchSize),
                Mathf.FloorToInt(point.Y / batchSize));
            if (!batches.TryGetValue(batch, out var variants))
                batches[batch] = variants = new[] {
                    new List<FernItem>(), new List<FernItem>(), new List<FernItem>(), new List<FernItem>() };
            var position = new Vector3(point.X - batch.X * batchSize, sample.X - 0.025f,
                point.Y - batch.Y * batchSize);
            variants[variant].Add(new FernItem(new Transform3D(basis, position), tint));
            placed++;
        }

        _trunks.Clear();
        for (int variant = 0; variant < meshes.Length; variant++)
        {
            if (meshes[variant] == null) continue;
            foreach (var entry in batches)
                if (entry.Value[variant].Count > 0)
                    AddBatch(meshes[variant], entry.Key, entry.Value[variant], batchSize, material);
        }
        source.Free();
        GD.Print("FernScatter: " + placed + " ferns across " + batches.Count +
            " cells (target " + target + "; grass:fern " +
            (_grass.PlacedTufts / (float)Mathf.Max(1, placed)).ToString("0.0") +
            ":1); no per-fern processing.");
    }

    // =========================================================
    // Approximate pine trunk clearances are checked from a one-time spatial lookup.
    private void CacheTrunks()
    {
        var forest = GetNodeOrNull<Node3D>(ForestPath);
        if (forest == null) return;
        foreach (var child in forest.GetChildren())
            if (child is StaticBody3D body)
                foreach (var proxy in body.GetChildren())
                    if (proxy is CollisionShape3D shape)
                    {
                        Vector3 local = ToLocal(shape.GlobalPosition);
                        var point = new Vector2(local.X, local.Z);
                        var cell = new Vector2I(Mathf.FloorToInt(point.X / TrunkCellSize),
                            Mathf.FloorToInt(point.Y / TrunkCellSize));
                        if (!_trunks.TryGetValue(cell, out var bucket)) _trunks[cell] = bucket = new();
                        bucket.Add(point);
                    }
    }

    private bool NearTrunk(Vector2 point)
    {
        var cell = new Vector2I(Mathf.FloorToInt(point.X / TrunkCellSize),
            Mathf.FloorToInt(point.Y / TrunkCellSize));
        int reach = Mathf.CeilToInt(Settings.TrunkClearance / TrunkCellSize) + 1;
        float clearanceSquared = Settings.TrunkClearance * Settings.TrunkClearance;
        for (int dz = -reach; dz <= reach; dz++)
        for (int dx = -reach; dx <= reach; dx++)
            if (_trunks.TryGetValue(cell + new Vector2I(dx, dz), out var bucket))
                foreach (var trunk in bucket)
                    if (point.DistanceSquaredTo(trunk) < clearanceSquared) return true;
        return false;
    }
    #endregion

    #region Rendering
    // =========================================================
    // One shared mesh per fern variant/cell, short culling, no collisions or shadows.
    private void AddBatch(Mesh mesh, Vector2I cell, List<FernItem> items, float batchSize, ShaderMaterial material)
    {
        var multi = new MultiMesh {
            TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
            UseColors = true, Mesh = mesh, InstanceCount = items.Count
        };
        float minY = float.PositiveInfinity, maxY = float.NegativeInfinity;
        for (int i = 0; i < items.Count; i++)
        {
            multi.SetInstanceTransform(i, items[i].Transform);
            multi.SetInstanceColor(i, items[i].Tint);
            minY = Mathf.Min(minY, items[i].Transform.Origin.Y);
            maxY = Mathf.Max(maxY, items[i].Transform.Origin.Y);
        }
        multi.CustomAabb = new Aabb(new Vector3(-3f, minY - 2f, -3f),
            new Vector3(batchSize + 6f, maxY - minY + 7f, batchSize + 6f));
        AddChild(new MultiMeshInstance3D {
            Multimesh = multi,
            MaterialOverride = material,
            Position = new Vector3(cell.X * batchSize, 0f, cell.Y * batchSize),
            VisibilityRangeEnd = Settings.CullDistance,
            VisibilityRangeEndMargin = 2f,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
        });
    }
    #endregion
}
