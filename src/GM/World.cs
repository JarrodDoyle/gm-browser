using System.Collections.Generic;
using Godot;

namespace GM;

public struct Sector
{
    // object offsets
}

public class World
{
    public float ImportScale { get; }
    public List<List<Vector3>> Sectors;
    public List<Sob> Sobs { get; }
    public List<string> Unknowns { get; }

    public World(float importScale, List<List<Vector3>> sectors, List<Sob> sobs, List<string> unknowns)
    {
        ImportScale = importScale;
        Sectors = sectors;
        Sobs = sobs;
        Unknowns = unknowns;
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