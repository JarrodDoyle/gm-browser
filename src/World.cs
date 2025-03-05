using System.Collections.Generic;
using Godot;

namespace GME;

public struct Sector
{
    // object offsets
}

public class World
{
    public World()
    {
        Sectors = new List<List<Vector3>>();
        Sobs = new List<Sob>();
        Unknowns = new List<string>();
    }

    public World(List<List<Vector3>> sectors, List<Sob> sobs, List<string> unknowns)
    {
        Sectors = sectors;
        Sobs = sobs;
        Unknowns = unknowns;
    }

    public List<List<Vector3>> Sectors { get; }

    public List<Sob> Sobs { get; }
    public List<string> Unknowns { get; }

    public void AddToMesh(Dictionary<string, MeshSurfaceData> surfaceDataMap)
    {
        var sobOffset = 0;
        foreach (var sector in Sectors)
        {
            for (var i = 0; i < sector.Count; i++)
            {
                var offset = sector[i];
                Sobs[sobOffset].AddToMesh(surfaceDataMap, offset, i == 0);
                sobOffset++;
            }
        }
    }
}