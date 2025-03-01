using Godot;
using Godot.Collections;

namespace GM.Render;

public partial class WorldRenderer: Node3D
{
    public World World;

    public void Rebuild(TextureManager textureManager)
    {
        foreach (var child in GetChildren())
        {
            child.QueueFree();
        }
        
        var sobOffset = 0;
        for (var i = 0; i < World.Sectors.Count; i++)
        {
            var sectorNode = new Node3D();
            sectorNode.AddToGroup(NodeGroups.Sectors);
            AddChild(sectorNode);
            
            var offsets = World.Sectors[i];
            for (var j = 0; j < offsets.Count; j++)
            {
                var offset = offsets[j];
                var gmObject = World.Sobs[sobOffset];
                
                var objectNode = new Node3D();
                objectNode.Position = offset;
                objectNode.AddToGroup(NodeGroups.Objects);
                objectNode.SetMeta("sectorId", i);
                objectNode.SetMeta("objectId", j);
                objectNode.SetMeta("globalObjectId", sobOffset);
                
                var metaData = new Array();
                var surfaceMap = new System.Collections.Generic.Dictionary<int, MeshSurfaceData>();
                for (var k = 0; k < gmObject.Polygons.Count; k++)
                {
                    var data = new MeshSurfaceData();
                    if (gmObject.AddPolyToMesh(k, textureManager, data, Vector3.Zero, j == 0))
                    {
                        surfaceMap.Add(k, data);
                        for (var l = 0; l < gmObject.Polygons[k].VertexCount - 2; l++)
                        {
                            metaData.Add(k);
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
                    mesh.SurfaceSetName(surfaceIdx, $"{i} {j} {polyIdx}");
                }

                if (mesh.GetSurfaceCount() != 0)
                {
                    var meshInstance = new MeshInstance3D { Mesh = mesh };
                    meshInstance.CreateTrimeshCollision();
                    
                    objectNode.SetMeta("triPolyMap", Variant.CreateFrom(metaData));
                    objectNode.AddChild(meshInstance);
                }
                
                sectorNode.AddChild(objectNode);
                sobOffset++;
            }
        }
    }
}