using GME.IO;
using Godot;

namespace GME.UI;

public partial class EditorView : Control
{
	private FileDialog _worldSelector;
	
	public override void _Ready()
	{
		_worldSelector = GetNode<FileDialog>("%WorldSelector");
		_worldSelector.CurrentDir = EditorContext.Instance.GameDir;
		_worldSelector.Visible = true;
		_worldSelector.FileSelected += EditorContext.Instance.LoadWorld;
	}
	
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventKey { Pressed: true } keyEvent)
		{
			if (keyEvent.Keycode == Key.T)
			{
				var writer = new TokenWriter();
				WorldParser.Write(writer, EditorContext.Instance.World);

				var path = EditorContext.Instance.WorldPath;
				GD.Print($"Saving: {path}");
				writer.Save(path);
			}

			if (keyEvent.Keycode == Key.O)
			{
				_worldSelector.Visible = true;
			}
		}
	}
}