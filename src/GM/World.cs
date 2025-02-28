using System.Collections.Generic;
using Godot;

namespace GM;

public struct Sector
{
    // object offsets
}

public class World
{
    public List<List<Vector3>> Sectors;
    public List<Sob> Sobs { get; }
    
    public World(TokenReader reader, float importScale)
    {
        var sectorCount = reader.ReadInt();

        var sobCount = 0;
        Sectors = new List<List<Vector3>>(sectorCount);
        for (var i = 0; i < sectorCount; i++)
        {
            // There seems to be some flags in the first half
            var objectCount = reader.ReadInt() & 0xFF;
            var objectPositions = new List<Vector3>(objectCount);
            for (var j = 0; j < objectCount; j++)
            {
                reader.ReadFloat(); // idk :) Thought it was flags, but apparently it can be a float lol
                objectPositions.Add(reader.ReadVector3(importScale));
                reader.ReadVector3(importScale, 3); // Axis?
            }
            Sectors.Add(objectPositions);
            
            sobCount += objectCount;
        }

        Sobs = new List<Sob>(sobCount);
        for (var i = 0; i < sobCount; i++)
        {
            Sobs.Add(new Sob(reader, importScale));
        }
    }

    public void AddToMesh(
        TextureManager textureManager,
        Dictionary<string, MeshSurfaceData> surfaceDataMap)
    {
        var sobOffset = 0;
        foreach (var sector in Sectors)
        {
            for (var i = 0; i < sector.Count; i++)
            {
                var offset = sector[i];
                Sobs[sobOffset].AddToMesh(textureManager, surfaceDataMap, offset, i == 0);
                sobOffset++;
            }
        }
    }
}