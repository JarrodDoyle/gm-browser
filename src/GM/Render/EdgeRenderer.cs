using System.Collections.Generic;
using Godot;

namespace GM.Render;

public partial class EdgeRenderer : Node3D
{
    private float _redrawRate = 1.0f;
    private float _redrawTimer;
    public bool Redraw;

    public override void _Process(double delta)
    {
        _redrawTimer += (float)delta;
        if (_redrawTimer >= _redrawRate)
        {
            Redraw = true;
            _redrawTimer -= _redrawRate;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Redraw)
        {
            Draw();
            Redraw = false;
        }
    }

    private void Draw()
    {
        var cameraPos = GetViewport().GetCamera3D().GlobalPosition;
        var genericLines = new List<Vector3>();
        var selectedLines = new List<Vector3>();

        var objectNodes = GetTree().GetNodesInGroup(NodeGroups.Objects);
        foreach (var node in objectNodes)
        {
            if (node is not ObjectRenderer objectRenderer)
            {
                continue;
            }

            var sectorId = objectRenderer.SectorId;
            var objectId = objectRenderer.ObjectId;
            var globalObjectId = objectRenderer.GlobalObjectId;

            var lines = objectRenderer.IsInGroup(NodeGroups.Selected) ? selectedLines : genericLines;
            AddLines(lines, cameraPos, sectorId, objectId, globalObjectId);
        }

        DebugDraw3D.ClearAll();
        using (DebugDraw3D.NewScopedConfig().SetThickness(0).SetViewport(GetViewport()))
        {
            DebugDraw3D.DrawLines(genericLines.ToArray(), new Color("#e5e5e5"), _redrawRate);
        }

        using (DebugDraw3D.NewScopedConfig().SetThickness(0).SetNoDepthTest(true).SetViewport(GetViewport()))
        {
            DebugDraw3D.DrawLines(selectedLines.ToArray(), new Color("#ff0000"), _redrawRate);
        }
    }

    private void AddLines(
        List<Vector3> lines,
        Vector3 cameraPos,
        int sectorId,
        int objectId,
        int globalObjectId)
    {
        var world = EditorContext.Instance.World;
        var offset = world.Sectors[sectorId][objectId];
        var gmObject = world.Sobs[globalObjectId];

        foreach (var poly in gmObject.Polygons)
        {
            for (var i = 0; i < poly.VertexCount; i++)
            {
                var v0 = offset + gmObject.Vertices[poly.Indices[i]];
                var v1 = offset + gmObject.Vertices[poly.Indices[(i + 1) % poly.VertexCount]];
                lines.Add(v0 + (cameraPos - v0).Normalized() * 0.01f);
                lines.Add(v1 + (cameraPos - v1).Normalized() * 0.01f);
            }
        }
    }
}