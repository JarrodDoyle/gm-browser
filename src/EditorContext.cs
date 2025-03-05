using GME.GM;
using GME.IO;
using GME.Render;
using GME.UI;
using Godot;

namespace GME;

public class EditorContext
{
    public delegate void LoadedWorldEventHandler();
    public delegate void SelectionChangedEventHandler();
    
    public event LoadedWorldEventHandler LoadedWorld;
    public event SelectionChangedEventHandler SelectionChanged;

    public static EditorContext? Instance { get; private set; }
    
    public string GameDir { get; private set; }
    public PathManager PathManager { get; private set; }
    public TextureManager TextureManager { get; private set; }
    public string? WorldPath { get; private set; }
    public World World { get; private set; }
    public float ImportScale { get; private set; }

    private Selection _currentSelection = Selection.None;
    public Selection CurrentSelection
    {
        get => _currentSelection;
        set
        {
            _currentSelection = value;
            SelectionChanged?.Invoke();
        }
    }

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
        CurrentSelection = Selection.None;
        LoadedWorld?.Invoke();
    }
}