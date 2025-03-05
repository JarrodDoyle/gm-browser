using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace GME.UI;

public partial class InstallManager : Control
{
    public delegate void SelectedInstallEventHandler(string installPath);
    public event SelectedInstallEventHandler SelectedInstall;
    
    private Button _addButton;
    private ConfigFile _configFile;
    private Button _exitButton;
    private FileDialog _folderSelect;
    private ItemList _installPaths;
    private Button _loadButton;
    private Button _removeButton;

    [Export(PropertyHint.File)] public string ConfigFilePath;

    public override void _Ready()
    {
        _folderSelect = GetNode<FileDialog>("%FolderSelect");
        _installPaths = GetNode<ItemList>("%InstallPaths");
        _addButton = GetNode<Button>("%AddButton");
        _removeButton = GetNode<Button>("%RemoveButton");
        _loadButton = GetNode<Button>("%LoadButton");
        _exitButton = GetNode<Button>("%ExitButton");

        _addButton.Pressed += () => _folderSelect.Visible = true;
        _removeButton.Pressed += RemoveDir;
        _loadButton.Pressed += LoadDir;
        _exitButton.Pressed += () => GetTree().Quit();
        _folderSelect.DirSelected += AddDir;
        _installPaths.ItemSelected += _ =>
        {
            _removeButton.Disabled = false;
            _loadButton.Disabled = false;
        };

        _configFile = new ConfigFile();
        if (_configFile.Load(ConfigFilePath) == Error.Ok)
        {
            var paths = _configFile.GetValue("general", "install_paths", Array.Empty<string>()).AsStringArray();
            foreach (var path in paths)
            {
                _installPaths.AddItem(path);
            }

            if (paths.Length > 0)
            {
                _installPaths.Select(0);
                _removeButton.Disabled = false;
                _loadButton.Disabled = false;
            }
        }
    }

    private void AddDir(string path)
    {
        _installPaths.AddItem(path);
        _installPaths.SortItemsByText();
        UpdateConfig();
    }

    private void RemoveDir()
    {
        var idx = _installPaths.GetSelectedItems().FirstOrDefault(0);
        _installPaths.RemoveItem(idx);
        UpdateConfig();

        _removeButton.Disabled = true;
        _loadButton.Disabled = true;
    }

    private void UpdateConfig()
    {
        var count = _installPaths.ItemCount;
        var paths = new List<string>();
        for (var i = 0; i < count; i++)
        {
            paths.Add(_installPaths.GetItemText(i));
        }

        _configFile.SetValue("general", "install_paths", paths.ToArray());
        _configFile.Save(ConfigFilePath);
    }

    private void LoadDir()
    {
        var idx = _installPaths.GetSelectedItems().FirstOrDefault(0);
        var path = _installPaths.GetItemText(idx);
        SelectedInstall?.Invoke(path);
        // EditorContext.Init(path);
        // GetTree().ChangeSceneToFile("uid://dmfu6i4ms4ojs");
    }
}