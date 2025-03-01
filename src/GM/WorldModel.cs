using System.IO;
using GM.IO;
using GM.Render;
using GM.UI;
using Godot;

namespace GM;

 public partial class WorldModel : Node3D
{
    [Export(PropertyHint.GlobalDir)]
    public string GameDir { get; set; }
	
    [Export]
    public string ObjectName { get; set; }

    [Export] public float ImportScale { get; set; } = 64.0f;

    private World _world;
    private TextureManager _textureManager;
    private WorldRenderer _worldRenderer;
    private EdgeRenderer _edgeRenderer;
    private ObjectSelector _objectSelector;
    
    private string GetWorldPath()
    {
        var dir = $"{GameDir}/";
        var options = new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive };
        var paths = Directory.GetFiles(dir, $"{ObjectName}.sow", options);
        return paths.IsEmpty() ? "" : paths[0];
    }

    public override void _Ready()
    {
        var path = GetWorldPath();
        var reader = new TokenReader(path);
        _world = WorldParser.Read(reader, ImportScale);

        _textureManager = new TextureManager(GameDir);
        foreach (var sob in _world.Sobs)
        {
            foreach (var textureName in sob.Textures)
            {
                if (!_textureManager.LoadTexture(textureName))
                {
                    GD.Print($"Failed to find texture: {textureName}");
                }
            }
        }
        
        _textureManager.LogTextures();
        
        _worldRenderer = new WorldRenderer();
        _worldRenderer.World = _world;
        _worldRenderer.Rebuild(_textureManager);
        AddChild(_worldRenderer);

        _edgeRenderer = new EdgeRenderer();
        _edgeRenderer.World = _world;
        _edgeRenderer.Redraw = true;
        AddChild(_edgeRenderer);

        _objectSelector = new ObjectSelector();
        _objectSelector.SelectedObject += ModifySelectedPoly;
        _objectSelector.SelectedObject += (_) => _edgeRenderer.Redraw = true;
        AddChild(_objectSelector);
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true } keyEvent)
        {
            if (keyEvent.Keycode == Key.T)
            {
                var writer = new TokenWriter();
                WorldParser.Write(writer, _world);
                
                var path = GetWorldPath();
                GD.Print($"Saving: {path}");
                writer.Save(GetWorldPath());
            }
        }
    }

    private void ModifySelectedPoly(Selection selection)
    {
        var poly = _world.Sobs[selection.GlobalObjectId].Polygons[selection.PolyId];
        poly.Uv += new Vector2(8, 8);
        _world.Sobs[selection.GlobalObjectId].Polygons[selection.PolyId] = poly;
        
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
                objectRenderer.Rebuild(_textureManager, _world);
            }
        }
    }
}