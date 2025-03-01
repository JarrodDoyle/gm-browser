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

            var objectCount = World.Sectors[i].Count;
            for (var j = 0; j < objectCount; j++)
            {
                var objectNode = new ObjectRenderer();
                objectNode.SectorId = i;
                objectNode.ObjectId = j;
                objectNode.GlobalObjectId = sobOffset;
                objectNode.Rebuild(textureManager, World);
                sectorNode.AddChild(objectNode);
                sobOffset++;
            }
        }
    }
}