using System.IO;
using System.Linq;
using Godot;

namespace GM.UI;

public partial class WorldBrowser : Control
{
    private Button _browseButton;
    private LineEdit _folderPath;
    private FileDialog _folderSelect;
    private string _gameDir;
    private Button _loadButton;
    private Node3D _worldManager;
    private ItemList _worldNames;

    public override void _Ready()
    {
        _worldManager = GetNode<Node3D>("%WorldManager");

        // TODO: Load initial folderpath from config and prefil everything
        _folderSelect = GetNode<FileDialog>("%FolderSelect");
        _folderPath = GetNode<LineEdit>("%FolderPath");
        _browseButton = GetNode<Button>("%BrowseButton");
        _worldNames = GetNode<ItemList>("%WorldList");
        _loadButton = GetNode<Button>("%LoadButton");

        _browseButton.Pressed += () => _folderSelect.Visible = true;
        _folderSelect.DirSelected += SetGameDir;
        _folderPath.TextSubmitted += SetGameDir;
        _worldNames.ItemSelected += _ => _loadButton.Disabled = false;
        _loadButton.Pressed += LoadWorld;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true } keyEvent)
        {
            if (keyEvent.Keycode == Key.Escape)
            {
                Visible = true;
            }
        }
    }

    private void SetGameDir(string path)
    {
        _folderPath.Text = path;
        _folderSelect.CurrentDir = path;
        _gameDir = path;

        _worldNames.Clear();
        var dir = $"{_gameDir}/";
        var options = new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive };
        var paths = Directory.GetFiles(dir, "*.sow", options).ToList();
        paths.Sort();
        foreach (var objectPath in paths)
        {
            var name = Path.GetFileNameWithoutExtension(objectPath);
            _worldNames.AddItem(name);
        }
    }

    private void LoadWorld()
    {
        foreach (var child in _worldManager.GetChildren())
        {
            child.QueueFree();
        }

        var worldIdx = _worldNames.GetSelectedItems().FirstOrDefault(0);
        var world = _worldNames.GetItemText(worldIdx);
        var worldPath = $"{_gameDir}/{world}.sow";

        EditorContext.Init(_gameDir, worldPath);
        _worldManager.AddChild(new WorldModel());
        Visible = false;
    }
}