using System.Collections.Generic;
using Godot;

namespace GM.Render;

public partial class ObjectRenderer: Node3D
{
    public int SectorId;
    public int ObjectId;
    public int GlobalObjectId;
    public List<int> TriPolyMap = new ();
    
    public bool Flipped;
    
    public void Rebuild(TextureManager textureManager, World world)
    {
        foreach (var child in GetChildren())
        {
            child.QueueFree();
        }
        
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
            if (gmObject.AddPolyToMesh(k, textureManager, data, Vector3.Zero, Flipped))
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