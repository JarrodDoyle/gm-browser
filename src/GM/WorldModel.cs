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
        
        var textures = new Dictionary<string, ImageTexture>();
        foreach (var sob in _world.Sobs)
        {
            foreach (var textureName in sob.Textures)
            {
                if (textures.ContainsKey(textureName))
                {
                    continue;
                }
                
                var dir = $"{GameDir}/Pics/";
                var options = new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive };
                var paths = Directory.GetFiles(dir, $"{textureName}.bmp", options);
                if (!paths.IsEmpty())
                {
                    var image = Image.LoadFromFile(paths[0]);
                    var texture = ImageTexture.CreateFromImage(image);
                    textures.Add(textureName, texture);
                }
                else
                {
                    GD.Print($"Failed to find texture: {textureName}");
                    textures.Add(textureName, ImageTexture.CreateFromImage(Image.CreateEmpty(256, 256, false, Image.Format.Rgb8)));
                }
            }
        }
        
        GD.Print("Textures: [");
        foreach (var (name, texture) in textures)
        {
            GD.Print($"  Name: {name}, Size: {texture.GetSize()}");
        }
        GD.Print("]");
        
        var surfaceDataMap = new Dictionary<string, MeshSurfaceData>();
        _world.AddToMesh(textures, surfaceDataMap);
        
        var mesh = new ArrayMesh();
        foreach (var (textureName, surfaceData) in surfaceDataMap)
        {
            var material = new StandardMaterial3D
            {
                AlbedoTexture = textures[textureName],
                TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest,
            };
            var array = surfaceData.BuildSurfaceArray();
            var surfaceIdx = mesh.GetSurfaceCount();
            mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, array);
            mesh.SurfaceSetMaterial(surfaceIdx, material);
        }
        
        var meshInstance = new MeshInstance3D { Mesh = mesh, Position = Vector3.Zero };
        AddChild(meshInstance);
    }
}