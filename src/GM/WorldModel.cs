using System.Collections.Generic;
using System.IO;
using GM.IO;
using Godot;
using Godot.Collections;

namespace GM;

[GlobalClass]
public partial class WorldModel : Node3D
{
    [Export(PropertyHint.GlobalDir)]
    public string GameDir { get; set; }
	
    [Export]
    public string ObjectName { get; set; }

    [Export] public float ImportScale { get; set; } = 64.0f;

    private World _world;
    
    private string GetWorldPath()
    {
        var dir = $"{GameDir}/";
        var options = new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive };
        var paths = Directory.GetFiles(dir, $"{ObjectName}.sow", options);
        return paths.IsEmpty() ? "" : paths[0];
    }

    public override void _Ready()
    {
        var path = GetWorldPath();
        var reader = new TokenReader(path);
        _world = new World(reader, ImportScale);

        var textureManager = new TextureManager(GameDir);
        foreach (var sob in _world.Sobs)
        {
            foreach (var textureName in sob.Textures)
            {
                if (!textureManager.LoadTexture(textureName))
                {
                    GD.Print($"Failed to find texture: {textureName}");
                }
            }
        }
        
        textureManager.LogTextures();
        
        // Each object as it's own mesh, and each poly as it's own surface (hmm yuck?)
        var faceSectorMap = new Array();
        var faceObjectMap = new Array();
        var facePolyMap = new Array();
        
        var sobOffset = 0;
        for (var i = 0; i < _world.Sectors.Count; i++)
        {
            var offsets = _world.Sectors[i];
            for (var j = 0; j < offsets.Count; j++)
            {
                var offset = offsets[j];
                var sob = _world.Sobs[sobOffset];
                sobOffset++;
                
                var metaData = new Array();
                var surfaceMap = new System.Collections.Generic.Dictionary<int, MeshSurfaceData>();
                for (var k = 0; k < sob.Polygons.Count; k++)
                {
                    var data = new MeshSurfaceData();
                    if (sob.AddPolyToMesh(k, textureManager, data, Vector3.Zero, j == 0))
                    {
                        surfaceMap.Add(k, data);
                        for (var l = 0; l < sob.Polygons[k].VertexCount - 2; l++)
                        {
                            faceSectorMap.Add(i);
                            faceObjectMap.Add(j);
                            facePolyMap.Add(k);
                        }
                    }
                }

                var mesh = new ArrayMesh();
                foreach (var (polyIdx, data) in surfaceMap)
                {
                    var textureName = sob.Polygons[polyIdx].TextureName;
                    var material = textureManager.Materials[textureName].Duplicate() as StandardMaterial3D;
                    
                    var array = data.BuildSurfaceArray();
                    var surfaceIdx = mesh.GetSurfaceCount();
                    mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, array);
                    mesh.SurfaceSetMaterial(surfaceIdx, material);
                    mesh.SurfaceSetName(surfaceIdx, $"{i} {j} {polyIdx}");
                }

                if (mesh.GetSurfaceCount() == 0)
                {
                    continue;
                }
                
                var meshInstance = new MeshInstance3D { Mesh = mesh, Position = offset };
                meshInstance.CreateTrimeshCollision();
                meshInstance.SetMeta("sector", i);
                meshInstance.SetMeta("object", j);
                meshInstance.SetMeta("face_poly_map", Variant.CreateFrom(metaData));

                AddChild(meshInstance);
            }
        }

        SetMeta("face_sector_map", faceSectorMap);
        SetMeta("face_object_map", faceObjectMap);
        SetMeta("face_poly_map", facePolyMap);
    }
    
    // !Temp:
    private bool _selectObject;
    private Vector2 _selectPosition;
    
    private const float RayLength = 1000.0f;
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } eventMouseButton)
        {
            _selectObject = true;
            _selectPosition = eventMouseButton.Position;
            var camera3D = GetViewport().GetCamera3D();
            var from = camera3D.ProjectRayOrigin(eventMouseButton.Position);
            var to = from + camera3D.ProjectRayNormal(eventMouseButton.Position) * RayLength;
        }
    }
    
    public override void _PhysicsProcess(double delta)
    {
        if (!_selectObject)
        {
            return;
        }
    
        _selectObject = false;
        
        var spaceState = GetWorld3D().DirectSpaceState;
        var camera3D = GetViewport().GetCamera3D();
        var from = camera3D.ProjectRayOrigin(_selectPosition);
        var to = from + camera3D.ProjectRayNormal(_selectPosition) * RayLength;
    
        var query = PhysicsRayQueryParameters3D.Create(from, to);
        var result = spaceState.IntersectRay(query);
        if (result.Count == 0)
        {
            return;
        }
        
        var collider = (Node)result["collider"] as StaticBody3D;
        var faceIdx = result["face_index"].AsInt32();

        if (collider == null)
        {
            return;
        }

        var sectorIdx = GetMeta("face_sector_map").AsInt32Array()[faceIdx];
        var objectIdx = GetMeta("face_object_map").AsInt32Array()[faceIdx];
        var polyIdx = GetMeta("face_poly_map").AsInt32Array()[faceIdx];
        
        GD.Print($"Sector: {sectorIdx}, Object: {objectIdx}, Poly: {polyIdx}");
    }
}