using System;
using GME.Render;
using Godot;

namespace GME.UI;

public readonly struct Selection : IEquatable<Selection>
{
    public static readonly Selection None = new(-1, -1, -1, -1);
    
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

    public bool Equals(Selection other)
    {
        return SectorId == other.SectorId && ObjectId == other.ObjectId && GlobalObjectId == other.GlobalObjectId && PolyId == other.PolyId;
    }

    public override bool Equals(object obj)
    {
        return obj is Selection other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(SectorId, ObjectId, GlobalObjectId, PolyId);
    }

    public static bool operator ==(Selection left, Selection right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Selection left, Selection right)
    {
        return !(left == right);
    }
}

public partial class ObjectSelector: Node3D
{
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

        // This sucks, but for now it's fine...
        if (collider?.GetParent().GetParent() is not ObjectRenderer objectRenderer)
        {
            return;
        }
        
        var sectorIdx = objectRenderer.SectorId;
        var objectIdx = objectRenderer.ObjectId;
        var globalObjectIdx = objectRenderer.GlobalObjectId;
        var polyIdx = objectRenderer.TriPolyMap[faceIdx];
        
        GD.Print($"Sector: {sectorIdx}, Object: {objectIdx}, Poly: {polyIdx}");
        
        var selection = new Selection(sectorIdx, objectIdx, globalObjectIdx, polyIdx);
        EditorContext.Instance.CurrentSelection = selection;
    }
}