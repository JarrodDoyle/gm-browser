using GM.IO;
using GM.Render;
using GM.UI;
using Godot;

namespace GM;

public partial class WorldModel : Node3D
{
    private EdgeRenderer _edgeRenderer;
    private ObjectSelector _objectSelector;
    private WorldRenderer _worldRenderer;

    public override void _Ready()
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

        _worldRenderer = new WorldRenderer();
        _worldRenderer.Rebuild();
        AddChild(_worldRenderer);

        _edgeRenderer = new EdgeRenderer();
        _edgeRenderer.World = world;
        _edgeRenderer.Redraw = true;
        AddChild(_edgeRenderer);

        _objectSelector = new ObjectSelector();
        _objectSelector.SelectedObject += ModifySelectedPoly;
        _objectSelector.SelectedObject += _ => _edgeRenderer.Redraw = true;
        AddChild(_objectSelector);
    }

    public override void _Input(InputEvent @event)
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
        }
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