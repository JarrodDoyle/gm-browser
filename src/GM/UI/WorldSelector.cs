using System.IO;
using System.Linq;
using Godot;

namespace GM.UI;

public partial class WorldSelector: Control
{
    private Node3D _WorldManager;
    
    private FileDialog _FolderSelect;
    private LineEdit _FolderPath;
    private Button _BrowseButton;
    private ItemList _Objects;

    private string _GameDir;
    
    public override void _Ready()
    {
        _WorldManager = GetNode<Node3D>("%WorldManager");
        
        // TODO: Load initial folderpath from config and prefil everything
        _FolderSelect = GetNode<FileDialog>("%FolderSelect");
        _FolderPath = GetNode<LineEdit>("%FolderPath");
        _BrowseButton = GetNode<Button>("%BrowseButton");
        _Objects = GetNode<ItemList>("%ObjectList");

        _BrowseButton.Pressed += () => _FolderSelect.Visible = true;
        _FolderSelect.DirSelected += SetGameDir;
        _FolderPath.TextSubmitted += SetGameDir;
        _Objects.ItemSelected += InitObject;
    }

    private void SetGameDir(string path)
    {
        _FolderPath.Text = path;
        _FolderSelect.CurrentDir = path;
        _GameDir = path;
        
        _Objects.Clear();
        var dir = $"{_GameDir}/";
        var options = new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive };
        var paths = Directory.GetFiles(dir, $"*.sow", options).ToList();
        paths.Sort();
        foreach (var objectPath in paths)
        {
            var name = Path.GetFileNameWithoutExtension(objectPath);
            _Objects.AddItem(name);
        }
    }

    private void InitObject(long idx)
    {
        _Objects.ReleaseFocus();
        
        var objectName = _Objects.GetItemText((int)idx);
        foreach (var child in _WorldManager.GetChildren())
        {
            child.QueueFree();
        }

        var world = new WorldModel();
        world.GameDir = _GameDir;
        world.ObjectName = objectName;
        
        _WorldManager.AddChild(world);
    }
}