using Godot;

namespace GME.UI;

public partial class MenuBar : Node
{
	private PopupMenu _fileMenu;
	private PopupMenu _editMenu;
	private PopupMenu _viewMenu;
	private PopupMenu _helpMenu;
	
	public override void _Ready()
	{
		SetupFileMenu();
		_editMenu = GetNode<PopupMenu>("%Edit");
		_viewMenu = GetNode<PopupMenu>("%View");
		_helpMenu = GetNode<PopupMenu>("%Help");
	}

	private void SetupFileMenu()
	{
		_fileMenu = GetNode<PopupMenu>("%File");

		var items = new[] { "_World", "New", "Open...", "Save", "Save As...", "_Object", "Import...", "Export...", "_", "Quit" };
		foreach (var item in items)
		{
			if (item.StartsWith('_'))
			{
				var separatorLabel = item.Trim('_');
				_fileMenu.AddSeparator(separatorLabel);
			}
			else
			{
				_fileMenu.AddItem(item);
			}
		}
	}
}