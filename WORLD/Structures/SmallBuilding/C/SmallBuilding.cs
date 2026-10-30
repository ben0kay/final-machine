// Small, reusable textured building shell; geometry and collision are generated once, never per-frame.
using Godot;

[Tool]
public partial class SmallBuilding : StaticBody3D
{
    [ExportGroup("Materials")]
    [Export] public Material FloorMaterial { get; set; }
    [Export] public Material WallMaterial { get; set; }

    [ExportGroup("Dimensions (metres)")]
    [Export(PropertyHint.Range, "4,12,0.25")] public float Width { get; set; } = 6.0f;
    [Export(PropertyHint.Range, "4,12,0.25")] public float Depth { get; set; } = 5.0f;
    [Export(PropertyHint.Range, "2.5,5,0.1")] public float WallHeight { get; set; } = 2.9f;
    [Export(PropertyHint.Range, "0.12,0.4,0.02")] public float WallThickness { get; set; } = 0.2f;
    [Export(PropertyHint.Range, "1,2.4,0.1")] public float DoorWidth { get; set; } = 1.4f;
    [Export(PropertyHint.Range, "1.8,3,0.1")] public float DoorHeight { get; set; } = 2.2f;
    [Export(PropertyHint.Range, "0.6,3,0.1")] public float RoofOpeningWidth { get; set; } = 1.8f;
    [Export(PropertyHint.Range, "0.6,3,0.1")] public float RoofOpeningDepth { get; set; } = 1.6f;
    [Export(PropertyHint.Range, "0.25,2,0.05")] public float TilesPerMetre { get; set; } = 0.75f;

    // =========================================================
    #region Construction

    // A tool script makes the shell visible when opened in the Godot editor.
    // Generated nodes are intentionally not owned/saved into the scene: no duplicate baked geometry.
    public override void _Ready()
    {
        if (GetNodeOrNull<MeshInstance3D>("ShellVisual") != null) return;
        if (FloorMaterial == null || WallMaterial == null)
        {
            GD.PushError("SmallBuilding: assign both shared floor and wall materials.");
            return;
        }

        float w = Mathf.Max(4f, Width), d = Mathf.Max(4f, Depth), h = Mathf.Max(2.5f, WallHeight);
        float t = Mathf.Clamp(WallThickness, 0.12f, 0.4f), slab = 0.2f;
        float doorW = Mathf.Clamp(DoorWidth, 1f, w - 2f * t - 0.5f);
        float doorH = Mathf.Clamp(DoorHeight, 1.8f, h - 0.2f);
        float holeW = Mathf.Clamp(RoofOpeningWidth, 0.6f, w - 2f * t - 0.5f);
        float holeD = Mathf.Clamp(RoofOpeningDepth, 0.6f, d - 2f * t - 0.5f);

        var floors = new SurfaceTool();
        var walls = new SurfaceTool();
        floors.Begin(Mesh.PrimitiveType.Triangles);
        walls.Begin(Mesh.PrimitiveType.Triangles);
        floors.SetMaterial(FloorMaterial);
        walls.SetMaterial(WallMaterial);

        // Floor top is at local Y=0, so the doorway has no raised threshold.
        AddPanel(floors, "Floor", new Vector3(0f, -slab * 0.5f, 0f), new Vector3(w, slab, d));

        // Front is +Z. Split its wall around the walk-through door and add one lintel.
        float side = (w - doorW) * 0.5f;
        float frontZ = d * 0.5f - t * 0.5f;
        AddPanel(walls, "FrontLeft", new Vector3(-(doorW + side) * 0.5f, h * 0.5f, frontZ),
            new Vector3(side, h, t));
        AddPanel(walls, "FrontRight", new Vector3((doorW + side) * 0.5f, h * 0.5f, frontZ),
            new Vector3(side, h, t));
        AddPanel(walls, "DoorLintel", new Vector3(0f, doorH + (h - doorH) * 0.5f, frontZ),
            new Vector3(doorW, h - doorH, t));

        // Other three solid walls. Side walls fit between the full-width front and rear panels.
        AddPanel(walls, "BackWall", new Vector3(0f, h * 0.5f, -frontZ), new Vector3(w, h, t));
        float sideX = w * 0.5f - t * 0.5f;
        AddPanel(walls, "LeftWall", new Vector3(-sideX, h * 0.5f, 0f), new Vector3(t, h, d - 2f * t));
        AddPanel(walls, "RightWall", new Vector3(sideX, h * 0.5f, 0f), new Vector3(t, h, d - 2f * t));

        // Four roof sections leave a real, unobstructed rectangular opening.
        float roofSide = (w - holeW) * 0.5f;
        float roofEnd = (d - holeD) * 0.5f;
        float roofY = h + slab * 0.5f;
        AddPanel(floors, "RoofLeft", new Vector3(-(holeW + roofSide) * 0.5f, roofY, 0f),
            new Vector3(roofSide, slab, d));
        AddPanel(floors, "RoofRight", new Vector3((holeW + roofSide) * 0.5f, roofY, 0f),
            new Vector3(roofSide, slab, d));
        AddPanel(floors, "RoofFront", new Vector3(0f, roofY, (holeD + roofEnd) * 0.5f),
            new Vector3(holeW, slab, roofEnd));
        AddPanel(floors, "RoofBack", new Vector3(0f, roofY, -(holeD + roofEnd) * 0.5f),
            new Vector3(holeW, slab, roofEnd));

        // One renderer with two material surfaces, rather than a draw call per box.
        var mesh = new ArrayMesh();
        floors.GenerateTangents();
        floors.Commit(mesh);
        walls.GenerateTangents();
        walls.Commit(mesh);
        AddChild(new MeshInstance3D { Name = "ShellVisual", Mesh = mesh });
    }

    // =========================================================
    // Each visual panel is an actual closed box, with one simple physics box matching it.
    private void AddPanel(SurfaceTool tool, string name, Vector3 center, Vector3 size)
    {
        AddBox(tool, center, size);
        AddChild(new CollisionShape3D {
            Name = name + "Collision",
            Position = center,
            Shape = new BoxShape3D { Size = size }
        });
    }

    #endregion

    // =========================================================
    #region Textured box geometry

    // Physical-length UVs prevent a 6m wall from stretching one texture over its entire face.
    private void AddBox(SurfaceTool tool, Vector3 c, Vector3 size)
    {
        float x = size.X * 0.5f, y = size.Y * 0.5f, z = size.Z * 0.5f;
        Vector3 P(float px, float py, float pz) => c + new Vector3(px, py, pz);
        AddQuad(tool, P(-x,-y,z), P(x,-y,z), P(x,y,z), P(-x,y,z));       // +Z
        AddQuad(tool, P(x,-y,-z), P(-x,-y,-z), P(-x,y,-z), P(x,y,-z));   // -Z
        AddQuad(tool, P(x,-y,z), P(x,-y,-z), P(x,y,-z), P(x,y,z));       // +X
        AddQuad(tool, P(-x,-y,-z), P(-x,-y,z), P(-x,y,z), P(-x,y,-z));   // -X
        AddQuad(tool, P(-x,y,z), P(x,y,z), P(x,y,-z), P(-x,y,-z));       // +Y
        AddQuad(tool, P(-x,-y,-z), P(x,-y,-z), P(x,-y,z), P(-x,-y,z));   // -Y
    }

    private void AddQuad(SurfaceTool tool, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        Vector3 normal = (b - a).Cross(d - a).Normalized();
        float u = (b - a).Length() * Mathf.Max(0.1f, TilesPerMetre);
        float v = (d - a).Length() * Mathf.Max(0.1f, TilesPerMetre);
        var uvA = Vector2.Zero; var uvB = new Vector2(u, 0f);
        var uvC = new Vector2(u, v); var uvD = new Vector2(0f, v);
        AddVertex(tool, a, normal, uvA);
        AddVertex(tool, b, normal, uvB);
        AddVertex(tool, c, normal, uvC);
        AddVertex(tool, a, normal, uvA);
        AddVertex(tool, c, normal, uvC);
        AddVertex(tool, d, normal, uvD);
    }

    private static void AddVertex(SurfaceTool tool, Vector3 position, Vector3 normal, Vector2 uv)
    {
        tool.SetNormal(normal);
        tool.SetUV(uv);
        tool.AddVertex(position);
    }

    #endregion
}
