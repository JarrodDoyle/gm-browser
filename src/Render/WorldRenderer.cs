using Godot;

namespace GME.Render;

public partial class WorldRenderer : Node3D
{
    public void Rebuild()
    {
        foreach (var child in GetChildren())
        {
            child.QueueFree();
        }

        var world = EditorContext.Instance.World;

        var sobOffset = 0;
        for (var i = 0; i < world.Sectors.Count; i++)
        {
            var sectorNode = new Node3D();
            sectorNode.AddToGroup(NodeGroups.Sectors);
            AddChild(sectorNode);

            var objectCount = world.Sectors[i].Count;
            for (var j = 0; j < objectCount; j++)
            {
                var objectNode = new ObjectRenderer();
                objectNode.SectorId = i;
                objectNode.ObjectId = j;
                objectNode.GlobalObjectId = sobOffset;
                objectNode.Rebuild();
                sectorNode.AddChild(objectNode);
                sobOffset++;
            }
        }
    }
}