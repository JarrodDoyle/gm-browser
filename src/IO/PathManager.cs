using System.IO;
using System.Linq;

namespace GME.IO;

public class PathManager
{
    private readonly string _animTextureFolder;
    private readonly string _textureFolder;

    public PathManager()
    {
        var gameDir = EditorContext.Instance.GameDir;
        var enumerationOptions = new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive };
        var directoryInfo = new DirectoryInfo(gameDir);
        _textureFolder = directoryInfo.GetDirectories("pics", enumerationOptions).FirstOrDefault()?.Name;
        _animTextureFolder = directoryInfo.GetDirectories("anims", enumerationOptions).FirstOrDefault()?.Name;
    }

    public bool TryGetTexturePath(string name, out string path)
    {
        return TryGetPath(_textureFolder, $"{name}.bmp", out path);
    }

    public bool TryGetAnimTexturePath(string name, out string path)
    {
        return TryGetPath(_animTextureFolder, $"{name}.soa", out path);
    }

    private static bool TryGetPath(string subFolder, string pattern, out string path)
    {
        var gameDir = EditorContext.Instance.GameDir;
        var dir = Path.Join(gameDir, subFolder);
        var enumerationOptions = new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive };
        var paths = Directory.GetFiles(dir, pattern, enumerationOptions);
        if (paths.Length == 0)
        {
            path = "";
            return false;
        }

        path = paths[0];
        return true;
    }
}