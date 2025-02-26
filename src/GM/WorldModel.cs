using System.Collections.Generic;
using System.IO;
using Godot;

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
        
        var surfaceDataMap = new Dictionary<string, MeshSurfaceData>();
        _world.AddToMesh(textureManager, surfaceDataMap);
        
        var mesh = new ArrayMesh();
        foreach (var (textureName, surfaceData) in surfaceDataMap)
        {
            var material = textureManager.Materials[textureName];
            var array = surfaceData.BuildSurfaceArray();
            var surfaceIdx = mesh.GetSurfaceCount();
            mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, array);
            mesh.SurfaceSetMaterial(surfaceIdx, material);
        }
        
        var meshInstance = new MeshInstance3D { Mesh = mesh, Position = Vector3.Zero };
        AddChild(meshInstance);
    }
}