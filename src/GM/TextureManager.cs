using System.Collections.Generic;
using System.IO;
using Godot;

namespace GM;

public class TextureManager
{
    public string GameDir { get; }
    public Dictionary<string, ImageTexture> Textures;
    public Dictionary<string, StandardMaterial3D> Materials;

    public TextureManager(string gameDir)
    {
        GameDir = gameDir;
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
        
        var options = new EnumerationOptions
        {
            MatchCasing = MatchCasing.CaseInsensitive,
            RecurseSubdirectories = true,
        };
        
        // TODO: Would be nice not to have to do a recursive search. But the folder case is meh
        // We search for an animation file first. For now we're just going to use the first frame
        var realTextureName = textureName;
        var animPaths = Directory.GetFiles(GameDir, $"{textureName}.soa", options);
        if (!animPaths.IsEmpty())
        {
            var reader = new TokenReader(animPaths[0]);
            reader.ReadString(4);
            realTextureName = reader.ReadString();
            GD.Print($"Loaded texture {realTextureName} for animation {textureName}.");
        }
        
        var foundTexture = false;
        var texPaths = Directory.GetFiles(GameDir, $"{realTextureName}.bmp", options);
        if (!texPaths.IsEmpty())
        {
            var image = Image.LoadFromFile(texPaths[0]);
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