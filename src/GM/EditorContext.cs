using GM.IO;
using Godot;

namespace GM;

public class EditorContext
{
    public delegate void LoadedWorldEventHandler();

    public static EditorContext? Instance { get; private set; }

    public string GameDir { get; private set; }
    public PathManager PathManager { get; private set; }
    public TextureManager TextureManager { get; private set; }
    public string? WorldPath { get; private set; }
    public World World { get; private set; }
    public float ImportScale { get; private set; }
    public event LoadedWorldEventHandler LoadedWorld;

    public static void Init(string gameDir)
    {
        Instance = new EditorContext();
        Instance.GameDir = gameDir;
        Instance.ImportScale = 64.0f;
        Instance.PathManager = new PathManager();
        Instance.TextureManager = new TextureManager();
        Instance.World = new World();
    }

    public void LoadWorld(string path)
    {
        WorldPath = path;
        World = WorldParser.Read(new TokenReader(path));
        GD.Print($"Loaded world: {path}");
        LoadedWorld?.Invoke();
    }
}