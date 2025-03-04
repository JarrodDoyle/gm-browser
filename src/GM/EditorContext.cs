using GM.IO;

namespace GM;

public class EditorContext
{
    public static EditorContext? Instance { get; private set; }

    public string GameDir { get; private set; }
    public PathManager PathManager { get; private set; }
    public TextureManager TextureManager { get; private set; }
    public string WorldPath { get; private set; }
    public World World { get; private set; }
    public float ImportScale { get; private set; }

    public static void Init(string gameDir, string worldPath)
    {
        Instance = new EditorContext();
        Instance.GameDir = gameDir;
        Instance.WorldPath = worldPath;
        Instance.ImportScale = 64.0f;
        Instance.PathManager = new PathManager();
        Instance.TextureManager = new TextureManager();
        Instance.World = WorldParser.Read(new TokenReader(worldPath));
    }
}