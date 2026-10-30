// One-shot scatter grounded on saved terrain. Small spatial MultiMeshes use native visibility ranges.
using Godot;
using System;
using System.Collections.Generic;
public partial class PineForest : Node3D
{
    [Export] public ForestSettings Settings { get; set; }
    [Export] public NodePath SurfacePath { get; set; }
    public enum PineVisualStyle { PineTree002 = 0, PineTree001 = 1 }
    [Export] public PineVisualStyle StartingVisualStyle { get; set; } = PineVisualStyle.PineTree002;
    public PineVisualStyle ActiveVisualStyle { get; private set; } = PineVisualStyle.PineTree002;

    private readonly Dictionary<Vector2I, Vector3> _occupied = new();
    private readonly List<(MultiMesh Multimesh, int Lod)> _visualBatches = new();
    // Three prepared meshes per family; only the selected startup family loads initially.
    private readonly Mesh[][] _styleMeshes = new Mesh[2][];

    public override void _Ready()
    {
        ActiveVisualStyle = StartingVisualStyle;
        CallDeferred(nameof(Build));
    }
    // =========================================================
    // Saved terrain queries stay behind the integration bridge, including slope and woodland mask.
    private void Build()
    {
        Settings ??= new ForestSettings();
        var surface = GetNode(SurfacePath);
        var random = new RandomNumberGenerator { Seed = (ulong)Settings.Seed };
        var thinning = new RandomNumberGenerator { Seed = (ulong)(Settings.Seed + 91007) };
        var keptPositions = new List<Vector3>();
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
            // Reserve every original location for spacing, but keep only a configurable fraction.
            // The placement RNG still draws scale/rotation for every accepted tree, preserving the
            // original positions and transforms of trees that survive the thinning pass.
            _occupied[cell] = position;
            float scale = random.RandfRange(0.88f, 1.12f);
            var basis = new Basis(Vector3.Up, random.RandfRange(0, Mathf.Tau)).Scaled(Vector3.One * scale);
            if (thinning.Randf() > Mathf.Clamp(Settings.TreeKeepFraction, 0f, 1f)) continue;
            keptPositions.Add(position);
            var batch = new Vector2I(Mathf.FloorToInt(x / Settings.BatchSize), Mathf.FloorToInt(z / Settings.BatchSize));
            if (!batches.TryGetValue(batch, out var transforms)) batches[batch] = transforms = new();
            transforms.Add(new Transform3D(basis, position - new Vector3(batch.X * Settings.BatchSize, 0, batch.Y * Settings.BatchSize)));
        }
        // Load the chosen startup tree family directly; the other one stays lazy until F1 selects it.
        var selectedMeshes = GetPreparedMeshes(ActiveVisualStyle);
        if (selectedMeshes == null)
        {
            GD.PushWarning("PineForest: Startup tree unavailable; falling back to PineTree002.");
            ActiveVisualStyle = PineVisualStyle.PineTree002;
            selectedMeshes = GetPreparedMeshes(ActiveVisualStyle);
            if (selectedMeshes == null) { GD.PushError("PineForest: No usable tree models."); return; }
        }
        for (int lod = 0; lod < 3; lod++)
        {
            var visual = new MeshInstance3D { Mesh = selectedMeshes[lod] };
            foreach (var batch in batches) AddMeshes(visual, Transform3D.Identity, batch.Key, batch.Value, lod);
            visual.Free();
        }
        // Simple static trunk proxies are generated once, independent of visual LOD.
        var body = new StaticBody3D { CollisionLayer = 1, CollisionMask = 0 };
        AddChild(body);
        var trunk = new CapsuleShape3D { Radius = 0.32f, Height = 16f };
        foreach (var position in keptPositions)
            body.AddChild(new CollisionShape3D { Shape = trunk, Position = position + Vector3.Up * 8 });
        GD.Print($"FM0: {keptPositions.Count} {ActiveVisualStyle} pines in {batches.Count} spatial batches; no per-tree processing.");
        _occupied.Clear();
    }

    #region Model comparison
    // =========================================================
    // Swap only each existing MultiMesh's shared mesh. Instance transforms, collision proxies,
    // spatial batches, visibility distances, and shadow settings are never reconstructed.
    public bool TrySetVisualStyle(PineVisualStyle style)
    {
        if (style != PineVisualStyle.PineTree002 && style != PineVisualStyle.PineTree001) return false;
        if (style == ActiveVisualStyle) return true;
        var meshes = GetPreparedMeshes(style);
        if (meshes == null) return false;
        ActiveVisualStyle = style;
        if (_visualBatches.Count > 0) ApplyPreparedMeshes(meshes);
        GD.Print("PineForest: visual family switched to " + style);
        return true;
    }

    // =========================================================
    // Load one tree family on demand. The source GLTFs stay untouched; only prepared shared
    // meshes are cached for existing MultiMeshes and F1 swaps (no extra forest nodes).
    private Mesh[] GetPreparedMeshes(PineVisualStyle style)
    {
        if (style != PineVisualStyle.PineTree002 && style != PineVisualStyle.PineTree001) return null;
        int key = (int)style;
        if (_styleMeshes[key] != null) return _styleMeshes[key];

        bool stylized = style == PineVisualStyle.PineTree001;
        var foliage = GD.Load<ShaderMaterial>(stylized
            ? "res://ASSETS/Vegetation/Trees/PineTree001/Materials/PineTree001Foliage.tres"
            : "res://ASSETS/Vegetation/Trees/PineTree002/Materials/PineNeedlesNear.tres");
        var bark = stylized ? GD.Load<ShaderMaterial>(
            "res://ASSETS/Vegetation/Trees/PineTree001/Materials/PineTree001Bark.tres") : null;
        var distantFoliage = stylized ? foliage : GD.Load<ShaderMaterial>(
            "res://ASSETS/Vegetation/Trees/PineTree002/Materials/PineNeedlesLod3.tres");
        if (foliage == null || distantFoliage == null || (stylized && bark == null))
        {
            GD.PushError("PineForest: Missing materials for " + style);
            return null;
        }
        var prepared = new Mesh[3];
        for (int lod = 0; lod < 3; lod++)
        {
            // PineTree001 uses source LOD0/1/2; PineTree002 uses source LOD1/2/3.
            string path = stylized
                ? $"res://ASSETS/Vegetation/Trees/PineTree001/Models/pine_tree_001_a_LOD{lod}.gltf"
                : $"res://ASSETS/Vegetation/Trees/PineTree002/Models/pine_tree_002_a_LOD{lod + 1}.gltf";
            var packed = GD.Load<PackedScene>(path);
            if (packed == null) { GD.PushError("PineForest: Missing tree model " + path); return null; }
            var source = packed.Instantiate<Node3D>();
            PrepareNeedleMaterial(source, lod == 2 ? distantFoliage : foliage, bark);
            prepared[lod] = FindFirstMesh(source)?.Mesh;
            source.Free();
            if (prepared[lod] == null)
            {
                GD.PushError("PineForest: Tree model has no mesh " + path);
                return null;
            }
        }
        _styleMeshes[key] = prepared;
        return prepared;
    }

    // =========================================================
    private void ApplyPreparedMeshes(Mesh[] meshes)
    {
        if (meshes == null || meshes.Length != 3) return;
        foreach (var (multi, lod) in _visualBatches)
            multi.Mesh = meshes[lod];
    }

    // =========================================================
    private static MeshInstance3D FindFirstMesh(Node3D node)
    {
        if (node is MeshInstance3D mesh && mesh.Mesh != null) return mesh;
        foreach (var child in node.GetChildren())
            if (child is Node3D child3D && FindFirstMesh(child3D) is MeshInstance3D found) return found;
        return null;
    }
    #endregion

    #region Materials and batches
    // =========================================================
    // Duplicate each source mesh once per LOD, not once per tree or batch.
    // PineTree002 retains its PBR bark. PineTree001 binds both vertex-colour shader surfaces.
    private void PrepareNeedleMaterial(Node3D node, ShaderMaterial needles, ShaderMaterial bark = null)
    {
        if (node is MeshInstance3D visual)
        {
            if (needles == null) GD.PushWarning("PineForest: needle shader missing; using original imported material.");
            else if (visual.Mesh is ArrayMesh imported && imported.GetSurfaceCount() >= 2)
            {
                var prepared = (ArrayMesh)imported.Duplicate(false);
                if (bark != null) prepared.SurfaceSetMaterial(0, bark);
                prepared.SurfaceSetMaterial(1, needles);
                visual.Mesh = prepared;
            }
            else GD.PushWarning("PineForest: expected two-surface ArrayMesh; keeping the imported material.");
        }
        foreach (var child in node.GetChildren())
            if (child is Node3D child3D) PrepareNeedleMaterial(child3D, needles, bark);
    }

    // =========================================================
    // Each batch/LOD shares prepared meshes and unchanged native visibility/collision settings.
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
            _visualBatches.Add((multi, lod));
        }
        foreach (var child in node.GetChildren()) if (child is Node3D child3D) AddMeshes(child3D, local, cell, transforms, lod);
    }
    #endregion
}
