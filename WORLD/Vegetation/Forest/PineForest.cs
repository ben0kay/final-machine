// One-shot scatter grounded on saved terrain. Small spatial MultiMeshes use native visibility ranges.
using Godot;
using System;
using System.Collections.Generic;
public partial class PineForest : Node3D
{
    [Export] public ForestSettings Settings { get; set; }
    [Export] public NodePath SurfacePath { get; set; }
    private readonly Dictionary<Vector2I, Vector3> _occupied = new();
    public override void _Ready() => CallDeferred(nameof(Build));
    // =========================================================
    // Saved terrain queries stay behind the integration bridge, including slope and woodland mask.
    private void Build()
    {
        Settings ??= new ForestSettings();
        var surface = GetNode(SurfacePath);
        var random = new RandomNumberGenerator { Seed = (ulong)Settings.Seed };
        var batches = new Dictionary<Vector2I, List<Transform3D>>();
        for (int i = 0; i < Settings.Attempts; i++)
        {
            float x = random.RandfRange(-470, 470), z = random.RandfRange(-470, 470);
            float density = surface.Call("woodland", x, z).AsSingle();
            if (random.Randf() > density) continue;
            float height = surface.Call("surface_height", x, z).AsSingle();
            if (!float.IsFinite(height) || height > 115 || surface.Call("surface_up", x, z).AsSingle() < 0.86f) continue;
            var position = new Vector3(x, height - 0.12f, z);
            var cell = new Vector2I(Mathf.FloorToInt(x / Settings.MinimumSpacing), Mathf.FloorToInt(z / Settings.MinimumSpacing));
            bool blocked = false;
            for (int dz = -1; dz <= 1; dz++)
            for (int dx = -1; dx <= 1; dx++)
                if (_occupied.TryGetValue(cell + new Vector2I(dx, dz), out var other) && new Vector2(x - other.X, z - other.Z).LengthSquared() < Settings.MinimumSpacing * Settings.MinimumSpacing) blocked = true;
            if (blocked) continue;
            _occupied[cell] = position;
            var batch = new Vector2I(Mathf.FloorToInt(x / Settings.BatchSize), Mathf.FloorToInt(z / Settings.BatchSize));
            if (!batches.TryGetValue(batch, out var transforms)) batches[batch] = transforms = new();
            float scale = random.RandfRange(0.88f, 1.12f);
            var basis = new Basis(Vector3.Up, random.RandfRange(0, Mathf.Tau)).Scaled(Vector3.One * scale);
            transforms.Add(new Transform3D(basis, position - new Vector3(batch.X * Settings.BatchSize, 0, batch.Y * Settings.BatchSize)));
        }
        for (int lod = 0; lod < 3; lod++)
        {
            string path = $"res://ASSETS/Vegetation/Trees/PineTree002/Models/pine_tree_002_a_LOD{lod + 1}.gltf";
            var source = GD.Load<PackedScene>(path).Instantiate<Node3D>();
            foreach (var batch in batches) AddMeshes(source, Transform3D.Identity, batch.Key, batch.Value, lod);
            source.Free();
        }
        // Simple static trunk proxies are generated once, independent of visual LOD.
        var body = new StaticBody3D { CollisionLayer = 1, CollisionMask = 0 };
        AddChild(body);
        var trunk = new CapsuleShape3D { Radius = 0.32f, Height = 16f };
        foreach (var position in _occupied.Values)
            body.AddChild(new CollisionShape3D { Shape = trunk, Position = position + Vector3.Up * 8 });
        GD.Print($"FM0: {_occupied.Count} pines in {batches.Count} spatial batches; no per-tree processing.");
        _occupied.Clear();
    }
    // =========================================================
    // Each batch/LOD shares meshes and imported materials; all tiers use identical bounds centers.
    private void AddMeshes(Node3D node, Transform3D parent, Vector2I cell, List<Transform3D> transforms, int lod)
    {
        Transform3D local = parent * node.Transform;
        if (node is MeshInstance3D mesh && mesh.Mesh != null)
        {
            var multi = new MultiMesh { TransformFormat = MultiMesh.TransformFormatEnum.Transform3D, Mesh = mesh.Mesh, InstanceCount = transforms.Count };
            for (int i = 0; i < transforms.Count; i++) multi.SetInstanceTransform(i, transforms[i] * local);
            // Shared conservative bounds avoid differing mesh origins opening holes at LOD switches.
            float minY = float.PositiveInfinity, maxY = float.NegativeInfinity;
            foreach (var t in transforms) { minY = Mathf.Min(minY, t.Origin.Y); maxY = Mathf.Max(maxY, t.Origin.Y); }
            multi.CustomAabb = new Aabb(new Vector3(-10, minY - 2, -10), new Vector3(Settings.BatchSize + 20, maxY - minY + 32, Settings.BatchSize + 20));
            var instance = new MultiMeshInstance3D {
                Multimesh = multi, Position = new Vector3(cell.X * Settings.BatchSize, 0, cell.Y * Settings.BatchSize),
                VisibilityRangeBegin = lod == 0 ? 0 : lod == 1 ? Settings.NearDistance : Settings.MiddleDistance,
                VisibilityRangeEnd = lod == 0 ? Settings.NearDistance : lod == 1 ? Settings.MiddleDistance : Settings.CullDistance,
                VisibilityRangeBeginMargin = 3, VisibilityRangeEndMargin = 3,
                CastShadow = lod == 2 ? GeometryInstance3D.ShadowCastingSetting.Off : GeometryInstance3D.ShadowCastingSetting.On
            };
            AddChild(instance);
        }
        foreach (var child in node.GetChildren()) if (child is Node3D child3D) AddMeshes(child3D, local, cell, transforms, lod);
    }
}
