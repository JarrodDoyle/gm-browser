using Godot;

namespace GM.UI;

public partial class WorldSelector : FileDialog
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        RootSubfolder = EditorContext.Instance.GameDir;
        DirSelected += EditorContext.Instance.LoadWorld;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true } keyEvent)
        {
            if (keyEvent.Keycode == Key.O)
            {
                GD.Print("BRUH");
                Visible = true;
            }
        }
    }
}