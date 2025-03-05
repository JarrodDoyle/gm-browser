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

    private void ModifySelectedPoly(Selection selection)
    {
        var world = EditorContext.Instance.World;
        var poly = world.Sobs[selection.GlobalObjectId].Polygons[selection.PolyId];
        poly.Uv += new Vector2(8, 8);
        world.Sobs[selection.GlobalObjectId].Polygons[selection.PolyId] = poly;

        var objectNodes = GetTree().GetNodesInGroup(NodeGroups.Objects);
        foreach (var node in objectNodes)
        {
            if (node is not ObjectRenderer objectRenderer)
            {
                continue;
            }

            if (objectRenderer.SectorId == selection.SectorId
                && objectRenderer.ObjectId == selection.ObjectId)
            {
                objectRenderer.Rebuild();
            }
        }
    }
}