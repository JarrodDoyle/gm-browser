using System.Collections.Generic;
using GME.IO;
using Godot;

namespace GME.Render;

public class TextureManager
{
    public readonly Dictionary<string, StandardMaterial3D> Materials = new();
    public readonly Dictionary<string, ImageTexture> Textures = new();

    public void LogTextures()
    {
        GD.Print("Textures: [");
        foreach (var (name, texture) in Textures) GD.Print($"  Name: {name}, Size: {texture.GetSize()}");
        GD.Print("]");
    }

    public bool LoadTexture(string textureName)
    {
        if (Textures.ContainsKey(textureName)) return true;

        var pathManager = EditorContext.Instance.PathManager;
        var realTextureName = textureName;
        if (pathManager.TryGetAnimTexturePath(textureName, out var animPath))
        {
            var reader = new TokenReader(animPath);
            reader.ReadString(4);
            realTextureName = reader.ReadString();
            GD.Print($"Loaded texture {realTextureName} for animation {textureName}.");
        }

        var foundTexture = false;
        if (pathManager.TryGetTexturePath(realTextureName, out var texPath))
        {
            var image = Image.LoadFromFile(texPath);
            var texture = ImageTexture.CreateFromImage(image);
            Textures.Add(textureName, texture);

            foundTexture = true;
        }
        else
        {
            // Dummy blank texture
            var image = Image.CreateEmpty(256, 256, false, Image.Format.Rgb8);
            Textures.Add(textureName, ImageTexture.CreateFromImage(image));
        }

        Materials.Add(textureName, new StandardMaterial3D
        {
            AlbedoTexture = Textures[textureName],
            TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest
        });

        return foundTexture;
    }
}