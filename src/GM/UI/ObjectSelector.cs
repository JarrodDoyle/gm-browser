using Godot;

namespace GM.UI;

public readonly struct Selection
{
    public readonly int SectorId;
    public readonly int ObjectId;
    public readonly int GlobalObjectId;
    public readonly int PolyId;

    public Selection(int sectorId, int objectId, int globalObjectId, int polyId)
    {
        SectorId = sectorId;
        ObjectId = objectId;
        GlobalObjectId = globalObjectId;
        PolyId = polyId;
    }
}

public partial class ObjectSelector: Node3D
{
    public delegate void SelectedObjectEventHandler(Selection selection);
    public event SelectedObjectEventHandler SelectedObject;
    
    private bool _selectObject;
    private Vector2 _selectPosition;
    
    private const float RayLength = 1000.0f;
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } eventMouseButton)
        {
            _selectObject = true;
            _selectPosition = eventMouseButton.Position;
        }
    }
    
    public override void _PhysicsProcess(double delta)
    {
        if (!_selectObject)
        {
            return;
        }
    
        _selectObject = false;
        
        var spaceState = GetWorld3D().DirectSpaceState;
        var camera3D = GetViewport().GetCamera3D();
        var from = camera3D.ProjectRayOrigin(_selectPosition);
        var to = from + camera3D.ProjectRayNormal(_selectPosition) * RayLength;
    
        var query = PhysicsRayQueryParameters3D.Create(from, to);
        var result = spaceState.IntersectRay(query);
        if (result.Count == 0)
        {
            return;
        }
        
        var collider = (Node)result["collider"] as StaticBody3D;
        var faceIdx = result["face_index"].AsInt32();
        if (collider == null)
        {
            return;
        }

        // This sucks, but for now it's fine...
        var gmObject = collider.GetParent().GetParent();
        var sectorIdx = gmObject.GetMeta("sectorId").AsInt32();
        var objectIdx = gmObject.GetMeta("objectId").AsInt32();
        var globalObjectIdx = gmObject.GetMeta("globalObjectId").AsInt32();
        var polyIdx = gmObject.GetMeta("triPolyMap").AsInt32Array()[faceIdx];
        
        GD.Print($"Sector: {sectorIdx}, Object: {objectIdx}, Poly: {polyIdx}");
        
        foreach (var node in GetTree().GetNodesInGroup(NodeGroups.Selected))
        {
            node.RemoveFromGroup(NodeGroups.Selected);
        }
        gmObject.AddToGroup(NodeGroups.Selected);

        SelectedObject?.Invoke(new Selection(sectorIdx, objectIdx, globalObjectIdx, polyIdx));
    }
}