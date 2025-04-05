using Gizmo3DPlugin;
using GME.Render;
using GME.UI;
using Godot;

namespace GME;

public partial class WorldModel : Node3D
{
    private EdgeRenderer _edgeRenderer;
    private ObjectSelector _objectSelector;
    private WorldRenderer _worldRenderer;
    private Gizmo3D _gizmo;

    public override void _Ready()
    {
        _worldRenderer = new WorldRenderer();
        AddChild(_worldRenderer);

        _edgeRenderer = new EdgeRenderer();
        AddChild(_edgeRenderer);

        _objectSelector = new ObjectSelector();
        AddChild(_objectSelector);

        _gizmo = new Gizmo3D();
        _gizmo.Mode = Gizmo3D.ToolMode.Move;
        _gizmo.TransformEnd += UpdateObjectTransform;
        AddChild(_gizmo);

        EditorContext.Instance.LoadedWorld += Reload;
        EditorContext.Instance.SelectionChanged += OnSelectionChanged;
        Reload();
    }

    public override void _ExitTree()
    {
        EditorContext.Instance.LoadedWorld -= Reload;
        EditorContext.Instance.SelectionChanged -= OnSelectionChanged;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        var gizmoActive = _gizmo.Hovering || _gizmo.Editing;
        _objectSelector.CanSelect = !gizmoActive;
    }

    private void OnSelectionChanged()
    {
        _gizmo.ClearSelection();

        var selection = EditorContext.Instance.CurrentSelection;
        var objectNodes = GetTree().GetNodesInGroup(NodeGroups.Objects);
        foreach (var node in objectNodes)
        {
            if (node is not ObjectRenderer objectRenderer)
            {
                continue;
            }

            if (objectRenderer.GlobalObjectId != selection.GlobalObjectId)
            {
                continue;
            }

            _gizmo.Select(objectRenderer);
        }
    }

    private void UpdateObjectTransform(int mode)
    {
        var context = EditorContext.Instance!;

        var selection = context.CurrentSelection;
        var objectNodes = GetTree().GetNodesInGroup(NodeGroups.Objects);
        foreach (var node in objectNodes)
        {
            if (node is not ObjectRenderer objectRenderer)
            {
                continue;
            }

            if (objectRenderer.GlobalObjectId != selection.GlobalObjectId)
            {
                continue;
            }

            context.World.Sectors[selection.SectorId][selection.ObjectId] = objectRenderer.Position;
        }

        context.TriggerObjectUpdated();
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