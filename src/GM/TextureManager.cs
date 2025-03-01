using System.Collections.Generic;
using System.IO;
using GM.IO;
using Godot;

namespace GM;

public class TextureManager
{
    public string GameDir { get; }
    public Dictionary<string, ImageTexture> Textures;
    public Dictionary<string, StandardMaterial3D> Materials;

    private readonly PathManager _pathManager;

    public TextureManager(string gameDir)
    {
        GameDir = gameDir;
        _pathManager = new PathManager(gameDir);
        Textures = new Dictionary<string, ImageTexture>();
        Materials = new Dictionary<string, StandardMaterial3D>();
    }

    public void LogTextures()
    {
        GD.Print("Textures: [");
        foreach (var (name, texture) in Textures)
        {
            GD.Print($"  Name: {name}, Size: {texture.GetSize()}");
        }
        GD.Print("]");
    }

    public bool LoadTexture(string textureName)
    {
        if (Textures.ContainsKey(textureName))
        {
            return true;
        }

        var realTextureName = textureName;
        if (_pathManager.TryGetAnimTexturePath(textureName, out var animPath))
        {
            var reader = new TokenReader(animPath);
            reader.ReadString(4);
            realTextureName = reader.ReadString();
            GD.Print($"Loaded texture {realTextureName} for animation {textureName}.");
        }

        var foundTexture = false;
        if (_pathManager.TryGetTexturePath(realTextureName, out var texPath))
        {
            var image = Image.LoadFromFile(texPath);
            var texture = ImageTexture.CreateFromImage(image);
            Textures.Add(textureName, texture);
            
            foundTexture = true;
        }
        else
        {
            // Dummy blank texture
            Textures.Add(textureName, ImageTexture.CreateFromImage(Image.CreateEmpty(256, 256, false, Image.Format.Rgb8)));
        }
        
        Materials.Add(textureName, new StandardMaterial3D
        {
            AlbedoTexture = Textures[textureName],
            TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest,
        });

        return foundTexture;
    }
}