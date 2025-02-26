using System.Collections.Generic;
using Godot;

namespace GM;

public struct Sector
{
    // object offsets
}

public class World
{
    private List<List<Vector3>> _sectors;
    public List<Sob> Sobs { get; }
    
    public World(TokenReader reader, float importScale)
    {
        var sectorCount = reader.ReadInt();

        var sobCount = 0;
        _sectors = new List<List<Vector3>>(sectorCount);
        for (var i = 0; i < sectorCount; i++)
        {
            var objectCount = reader.ReadInt();
            sobCount += objectCount;
            
            var objectPositions = new List<Vector3>(objectCount);
            for (var j = 0; j < objectCount; j++)
            {
                reader.ReadInt(); // Flags
                objectPositions.Add(reader.ReadVector3(importScale));
                reader.ReadVector3(importScale, 3); // Axis?
            }
            _sectors.Add(objectPositions);
            
        }

        Sobs = new List<Sob>(sobCount);
        for (var i = 0; i < sobCount; i++)
        {
            Sobs.Add(new Sob(reader, importScale));
        }
    }

    public void AddToMesh(
        Dictionary<string, ImageTexture> textures,
        Dictionary<string, MeshSurfaceData> surfaceDataMap)
    {
        var sobOffset = 0;
        foreach (var sector in _sectors)
        {
            for (var i = 0; i < sector.Count; i++)
            {
                var offset = sector[i];
                Sobs[sobOffset].AddToMesh(textures, surfaceDataMap, offset, i == 0);
                sobOffset++;
            }
        }
    }
}