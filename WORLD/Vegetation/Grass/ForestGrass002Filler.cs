// Dispersed, one-time ForestGrass002 ground cover alongside the existing ForestGrass001 patches.
using Godot;
using System;
using System.Collections.Generic;

public partial class ForestGrass002Filler : Node3D
{
    [Export] public ForestGrass002FillerSettings Settings { get; set; }
    [Export] public NodePath SurfacePath { get; set; }
    [Export] public NodePath ForestPath { get; set; }

    private const float Extent = 470f;
    private const float TrunkBucketSize = 4f;
    private readonly Dictionary<Vector2I, List<Vector2>> _trunks = new();

    private readonly struct Item
    {
        public readonly Transform3D Transform;
        public readonly Color Tint;
        public Item(Transform3D transform, Color tint) { Transform = transform; Tint = tint; }
    }

    #region Placement
    public override void _Ready() => CallDeferred(nameof(Build));

    // =========================================================
    // Strided visit order keeps density distributed over the map even when the cap is reached.
    // Each jittered grid cell can contribute one tuft; noise creates light local density variation.
    private void Build()
    {
        Settings ??= new ForestGrass002FillerSettings();
        if (!Settings.Enabled) return;
        var surface = GetNodeOrNull(SurfacePath);
        if (surface == null) { GD.PushError("ForestGrass002Filler: Terrain3D surface missing."); return; }
        var material = GD.Load<ShaderMaterial>("res://WORLD/Vegetation/Grass/Materials/ForestGrass002Filler.tres");
        if (material == null) { GD.PushError("ForestGrass002Filler: grass material missing."); return; }
        var meshes = new Mesh[2, 3];
        for (int lod = 0; lod < 2; lod++)
        for (int variant = 0; variant < 3; variant++)
        {
            char letter = (char)('a' + variant);
            string path = "res://ASSETS/Vegetation/Grass/ForestGrass002/Models/forest_grass_002_" +
                letter + "_LOD" + (lod + 1) + ".gltf";
            var packed = GD.Load<PackedScene>(path);
            if (packed == null) { GD.PushError("ForestGrass002Filler: Missing model " + path); return; }
            var source = packed.Instantiate<Node3D>();
            meshes[lod, variant] = FindMesh(source);
            source.Free();
            if (meshes[lod, variant] == null)
            {
                GD.PushError("ForestGrass002Filler: Mesh missing in " + path); return;
            }
        }

        CacheTrunks();
        var random = new RandomNumberGenerator { Seed = (ulong)Mathf.Max(0, Settings.Seed) };
        var noise = new FastNoiseLite { Seed = Settings.Seed + 319, Frequency = 0.065f };
        float spacing = Mathf.Max(0.75f, Settings.GridSpacing);
        float batchSize = Mathf.Max(8f, Settings.BatchSize);
        int side = Mathf.CeilToInt(Extent * 2f / spacing);
        int total = side * side;
        int start = random.RandiRange(0, total - 1);
        int stride = random.RandiRange(side + 1, Mathf.Max(side + 1, total / 2));
        while (Gcd(stride, total) != 1) stride++;
        var batches = new Dictionary<Vector2I, List<Item>[]>();
        int placed = 0;

        for (int i = 0; i < total && placed < Settings.MaximumTufts; i++)
        {
            int index = (int)((start + (long)i * stride) % total);
            int column = index % side, row = index / side;
            float x = -Extent + (column + 0.5f + random.RandfRange(-Settings.Jitter, Settings.Jitter)) * spacing;
            float z = -Extent + (row + 0.5f + random.RandfRange(-Settings.Jitter, Settings.Jitter)) * spacing;
            if (Mathf.Abs(x) >= Extent || Mathf.Abs(z) >= Extent) continue;
            float pocket = 0.5f + 0.5f * noise.GetNoise2D(x, z);
            if (random.Randf() > Mathf.Clamp(Settings.Coverage * (0.78f + 0.35f * pocket), 0f, 1f)) continue;
            if (surface.Call("woodland", x, z).AsSingle() < Settings.MinimumWoodland) continue;
            if (NearTrunk(new Vector2(x, z))) continue;
            Vector4 sample = surface.Call("grass_sample", x, z).AsVector4();
            if (!float.IsFinite(sample.X) || sample.X > Settings.MaximumHeight ||
                sample.Y < Settings.MinimumSurfaceUp || sample.Z <= 0f ||
                random.Randf() > sample.Z) continue;

            float choice = random.Randf();
            int variant = choice < 0.44f ? 0 : choice < 0.88f ? 1 : 2;
            float low = Mathf.Min(Settings.MinimumScale, Settings.MaximumScale);
            float high = Mathf.Max(Settings.MinimumScale, Settings.MaximumScale);
            float scale = Mathf.Lerp(low, high, Mathf.Pow(random.Randf(), 1.45f));
            float width = scale * random.RandfRange(Settings.MinimumWidth, Settings.MaximumWidth);
            Vector3 normal = surface.Call("surface_normal", x, z).AsVector3();
            Vector3 up = Vector3.Up.Lerp(normal, Settings.NormalAlignment).Normalized();
            var basis = (new Basis(new Quaternion(Vector3.Up, up)) *
                new Basis(Vector3.Up, random.RandfRange(0f, Mathf.Tau)))
                .Scaled(new Vector3(width, scale, width));
            float tone = random.RandfRange(1f - Settings.TintVariation, 1f + Settings.TintVariation);
            var tint = new Color(tone * 0.97f, tone * 1.01f, tone * 0.96f, 1f);
            var cell = new Vector2I(Mathf.FloorToInt(x / batchSize), Mathf.FloorToInt(z / batchSize));
            if (!batches.TryGetValue(cell, out var variants))
                batches[cell] = variants = new[] { new List<Item>(), new List<Item>(), new List<Item>() };
            var position = new Vector3(x - cell.X * batchSize, sample.X - 0.025f, z - cell.Y * batchSize);
            variants[variant].Add(new Item(new Transform3D(basis, position), tint));
            placed++;
        }

        _trunks.Clear();
        foreach (var entry in batches)
        for (int variant = 0; variant < 3; variant++)
            if (entry.Value[variant].Count > 0)
                for (int lod = 0; lod < 2; lod++)
                    AddBatch(meshes[lod, variant], entry.Key, entry.Value[variant], batchSize, lod, material);
        GD.Print("ForestGrass002Filler: " + placed + " filler tufts across " + batches.Count +
            " cells (cap " + Settings.MaximumTufts + "); no per-frame callbacks.");
    }

    // =========================================================
    private static int Gcd(int a, int b)
    {
        while (b != 0) { int remainder = a % b; a = b; b = remainder; }
        return a;
    }

    // =========================================================
    private static Mesh FindMesh(Node node)
    {
        if (node is MeshInstance3D source && source.Mesh != null) return source.Mesh;
        foreach (var child in node.GetChildren())
            if (FindMesh(child) is Mesh mesh) return mesh;
        return null;
    }

    // =========================================================
    // Read the existing shared pine trunk proxies; do not add physics bodies per tuft.
    private void CacheTrunks()
    {
        var forest = GetNodeOrNull<Node3D>(ForestPath);
        if (forest == null) { GD.PushWarning("ForestGrass002Filler: trunk clearance unavailable."); return; }
        foreach (var child in forest.GetChildren())
            if (child is StaticBody3D body)
                foreach (var proxy in body.GetChildren())
                    if (proxy is CollisionShape3D shape)
                    {
                        Vector3 local = ToLocal(shape.GlobalPosition);
                        var position = new Vector2(local.X, local.Z);
                        var cell = new Vector2I(Mathf.FloorToInt(local.X / TrunkBucketSize),
                            Mathf.FloorToInt(local.Z / TrunkBucketSize));
                        if (!_trunks.TryGetValue(cell, out var list)) _trunks[cell] = list = new();
                        list.Add(position);
                    }
    }

    // =========================================================
    private bool NearTrunk(Vector2 point)
    {
        var cell = new Vector2I(Mathf.FloorToInt(point.X / TrunkBucketSize),
            Mathf.FloorToInt(point.Y / TrunkBucketSize));
        int reach = Mathf.CeilToInt(Settings.TrunkClearance / TrunkBucketSize);
        float minDistanceSquared = Settings.TrunkClearance * Settings.TrunkClearance;
        for (int dz = -reach; dz <= reach; dz++)
        for (int dx = -reach; dx <= reach; dx++)
            if (_trunks.TryGetValue(cell + new Vector2I(dx, dz), out var list))
                foreach (var other in list)
                    if (point.DistanceSquaredTo(other) < minDistanceSquared) return true;
        return false;
    }
    #endregion

    #region Rendering
    // =========================================================
    // Native 2-LOD MultiMesh batching: no collision, shadows, runtime C# loops or new manager.
    private void AddBatch(Mesh mesh, Vector2I cell, List<Item> items, float batchSize, int lod, ShaderMaterial material)
    {
        var multi = new MultiMesh {
            TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
            UseColors = true,
            Mesh = mesh,
            InstanceCount = items.Count
        };
        float minY = float.PositiveInfinity, maxY = float.NegativeInfinity;
        for (int i = 0; i < items.Count; i++)
        {
            multi.SetInstanceTransform(i, items[i].Transform);
            multi.SetInstanceColor(i, items[i].Tint);
            minY = Mathf.Min(minY, items[i].Transform.Origin.Y);
            maxY = Mathf.Max(maxY, items[i].Transform.Origin.Y);
        }
        float pad = Mathf.Max(2f, Settings.MaximumScale * Settings.MaximumWidth * 0.8f + 0.5f);
        multi.CustomAabb = new Aabb(
            new Vector3(-pad, minY - 1.5f, -pad),
            new Vector3(batchSize + 2f * pad, maxY - minY + 3.5f, batchSize + 2f * pad));
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
    #endregion
}
