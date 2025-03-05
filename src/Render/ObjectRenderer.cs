using System.Collections.Generic;
using Godot;

namespace GME.Render;

public partial class ObjectRenderer : Node3D
{
    public readonly List<int> TriPolyMap = new();
    public bool Flipped;
    public int GlobalObjectId;
    public int ObjectId;
    public int SectorId;

    public override void _Ready()
    {
        EditorContext.Instance.ObjectUpdated += () =>
        {
            var selection = EditorContext.Instance.CurrentSelection;
            if (selection.GlobalObjectId == GlobalObjectId)
            {
                Rebuild();
            }
        };
    }

    public void Rebuild()
    {
        foreach (var child in GetChildren())
        {
            child.QueueFree();
        }

        var world = EditorContext.Instance.World;
        var textureManager = EditorContext.Instance.TextureManager;

        var offset = world.Sectors[SectorId][ObjectId];
        var gmObject = world.Sobs[GlobalObjectId];

        TriPolyMap.Clear();
        Flipped = ObjectId == 0;
        Position = offset;
        AddToGroup(NodeGroups.Objects);

        var surfaceMap = new Dictionary<int, MeshSurfaceData>();
        for (var k = 0; k < gmObject.Polygons.Count; k++)
        {
            var data = new MeshSurfaceData();
            if (gmObject.AddPolyToMesh(k, data, Vector3.Zero, Flipped))
            {
                surfaceMap.Add(k, data);
                for (var l = 0; l < gmObject.Polygons[k].VertexCount - 2; l++)
                {
                    TriPolyMap.Add(k);
                }
            }
        }

        var mesh = new ArrayMesh();
        foreach (var (polyIdx, data) in surfaceMap)
        {
            var textureName = gmObject.Polygons[polyIdx].TextureName;
            var material = textureManager.Materials[textureName].Duplicate() as StandardMaterial3D;

            var array = data.BuildSurfaceArray();
            var surfaceIdx = mesh.GetSurfaceCount();
            mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, array);
            mesh.SurfaceSetMaterial(surfaceIdx, material);
            mesh.SurfaceSetName(surfaceIdx, $"{SectorId} {ObjectId} {polyIdx}");
        }

        if (mesh.GetSurfaceCount() != 0)
        {
            var meshInstance = new MeshInstance3D { Mesh = mesh };
            meshInstance.CreateTrimeshCollision();
            AddChild(meshInstance);
        }
    }
}