using System.Collections.Generic;
using Godot;

namespace GM.IO;

public static class WorldParser
{
    public static World Read(TokenReader reader)
    {
        var importScale = EditorContext.Instance.ImportScale;

        var sectorCount = reader.ReadInt();

        var sobCount = 0;
        var sectors = new List<List<Vector3>>(sectorCount);
        var unknowns = new List<string>();
        for (var i = 0; i < sectorCount; i++)
        {
            // There seems to be some flags in the first half
            var objectCount = reader.ReadInt() & 0xFF;
            var objectPositions = new List<Vector3>(objectCount);
            for (var j = 0; j < objectCount; j++)
            {
                // idk :) Thought it was flags, but apparently it can be a float lol
                unknowns.Add(reader.ReadString());
                objectPositions.Add(reader.ReadVector3(importScale));
                // Axis of some sort. Perhaps trans matrix columns
                unknowns.AddRange(reader.ReadString(9));
            }

            sectors.Add(objectPositions);
            sobCount += objectCount;
        }

        var sobs = new List<Sob>(sobCount);
        for (var i = 0; i < sobCount; i++)
        {
            sobs.Add(ObjectParser.Read(reader));
        }

        return new World(sectors, sobs, unknowns);
    }

    public static void Write(TokenWriter writer, World world)
    {
        var unknowns = new Queue<string>(world.Unknowns);

        writer.Write(world.Sectors.Count);
        writer.NewLine();

        foreach (var offsets in world.Sectors)
        {
            // TODO: Sector flags are dropped :(
            writer.Write(offsets.Count);
            writer.NewLine();

            foreach (var offset in offsets)
            {
                writer.Write(unknowns.Dequeue(), true);
                writer.Write(offset, EditorContext.Instance.ImportScale);
                for (var i = 0; i < 9; i++)
                {
                    writer.Write(unknowns.Dequeue(), true);
                }

                writer.NewLine();
            }
        }

        foreach (var gmObject in world.Sobs)
        {
            ObjectParser.Write(writer, gmObject);
        }
    }
}