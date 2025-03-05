using GME.UI;
using Godot;

namespace GME;

public partial class Main : Node
{
	private InstallManager _installManager;
	
	public override void _Ready()
	{
		_installManager = GetNode<InstallManager>("%InstallManager");
		_installManager.SelectedInstall += LoadEditor;
	}

	private void LoadEditor(string installPath)
	{
		EditorContext.Init(installPath);
		GetTree().ChangeSceneToFile("uid://dmfu6i4ms4ojs");
	}
}