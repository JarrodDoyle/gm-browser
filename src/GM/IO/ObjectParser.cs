using System.Collections.Generic;
using Godot;

namespace GM.IO;

public static class ObjectParser
{
    public static Sob Read(TokenReader reader, float importScale)
    {
        var unknowns = new List<string>();
        
        var vertexCount = reader.ReadInt();
        var polyCount = reader.ReadInt();
        
        var vertices = new List<Vector3>(vertexCount);
        for (var i = 0; i < vertexCount; i++)
        {
            vertices.Add(reader.ReadVector3(importScale));
        }
        
        var polygons = new List<Polygon>(polyCount);
        for (var i = 0; i < polyCount; i++)
        {
            var textureName = reader.ReadString();
            unknowns.Add(reader.ReadString());
            var anchorMode = reader.ReadString();
            unknowns.AddRange(reader.ReadString(5));
            var brightness = reader.ReadInt();
            var vCount = reader.ReadInt();
            var indices = reader.ReadInt(vCount);
            var uv0 = reader.ReadInt();
            var uv1 = reader.ReadInt();
            var uv2 = reader.ReadInt();
            var uv = (uv0 == -1 ? new Vector2(uv1, uv2) : new Vector2(uv1, uv0)) / 128.0f;
            var altUvMode = uv0 == -1;
            var uvec = reader.ReadVector3(65536.0f);
            var vvec = reader.ReadVector3(65536.0f);
            var lmuv = reader.ReadVector2(128.0f);
            unknowns.Add(reader.ReadString());
            var mode = (Polygon.RenderMode)reader.ReadInt();
            unknowns.AddRange(reader.ReadString(2));
			
            // TODO: Should polygon parsing be split out?
            polygons.Add(new Polygon {
                CenterAnchor = anchorMode == "0",
                Brightness = brightness,
                VertexCount = vCount,
                Indices = indices,
                AltUvMode = altUvMode,
                Uv = uv,
                LmUv = lmuv,
                UVec = uvec,
                VVec = vvec,
                TextureName = textureName,
                Mode = mode,
            });
        }

        return new Sob(importScale, vertices, polygons, unknowns);
    }

    public static void Write(TokenWriter writer, Sob gmObject)
    {
        var unknowns = new Queue<string>(gmObject.Unknowns);
        
        writer.Write(gmObject.Vertices.Count);
        writer.NewLine();
        writer.Write(gmObject.Polygons.Count);
        writer.NewLine();
        foreach (var vertex in gmObject.Vertices)
        {
            writer.Write(vertex, gmObject.ImportScale);
            writer.NewLine();
        }

        foreach (var poly in gmObject.Polygons)
        {
            writer.Write(poly.TextureName);
            writer.NewLine();
            writer.Write(unknowns.Dequeue());
            writer.NewLine();
            writer.Write(poly.CenterAnchor ? "0" : "none");
            writer.NewLine();
            for (var i = 0; i < 5; i++)
            {
                writer.Write(unknowns.Dequeue());
                writer.NewLine();
            }
            writer.Write(poly.Brightness);
            writer.Write(poly.VertexCount);
            foreach (var idx in poly.Indices)
            {
                writer.Write(idx);
            }

            if (poly.AltUvMode)
            {
                writer.Write(-1);
                writer.NewLine();
                writer.Write(poly.Uv.X * 128.0f);
                writer.Write(poly.Uv.Y * 128.0f);
            }
            else
            {
                writer.NewLine();
                writer.Write(poly.Uv.Y * 128.0f);
                writer.Write(poly.Uv.X * 128.0f);
                writer.Write(0);
            }
            writer.Write(poly.UVec, 65536.0f);
            writer.Write(poly.VVec, 65536.0f);
            writer.Write(poly.LmUv, 128.0f);
            writer.Write(unknowns.Dequeue(), true);
            writer.Write((int)poly.Mode);
            for (var i = 0; i < 2; i++)
            {
                writer.Write(unknowns.Dequeue(), true);
            }
        }
    }
}