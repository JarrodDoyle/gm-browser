using GME.Render;
using GME.UI;
using Godot;

namespace GME;

public partial class WorldModel : Node3D
{
    private EdgeRenderer _edgeRenderer;
    private ObjectSelector _objectSelector;
    private WorldRenderer _worldRenderer;

    public override void _Ready()
    {
        _worldRenderer = new WorldRenderer();
        AddChild(_worldRenderer);

        _edgeRenderer = new EdgeRenderer();
        AddChild(_edgeRenderer);

        _objectSelector = new ObjectSelector();
        AddChild(_objectSelector);

        EditorContext.Instance.LoadedWorld += Reload;
        Reload();
    }

    public override void _ExitTree()
    {
        EditorContext.Instance.LoadedWorld -= Reload;
    }

    private void Reload()
    {
        var world = EditorContext.Instance.World;
        var textureManager = EditorContext.Instance.TextureManager;
        foreach (var sob in world.Sobs)
        {
            foreach (var textureName in sob.Textures)
            {
                if (!textureManager.LoadTexture(textureName))
                {
                    GD.Print($"Failed to find texture: {textureName}");
                }
            }
        }

        textureManager.LogTextures();
        _worldRenderer.Rebuild();
        _edgeRenderer.Redraw = true;
    }
}