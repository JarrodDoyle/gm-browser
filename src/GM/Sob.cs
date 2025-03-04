using System.Collections.Generic;
using Godot;

namespace GM;

public struct Polygon
{
    public enum RenderMode
    {
        Normal,
        Sector,
        Hole
    }

    public bool CenterAnchor;
    public int Brightness;
    public int VertexCount;
    public List<int> Indices;
    public bool AltUvMode;
    public Vector2 Uv;
    public Vector2 LmUv;
    public Vector3 UVec;
    public Vector3 VVec;
    public string TextureName;
    public RenderMode Mode;

    public override string ToString()
    {
        return
            $"Poly(CenterAnchor: {CenterAnchor}, Flags: {Brightness}, VertexCount: {VertexCount}, Indices: [{string.Join(", ", Indices)}], UV: {Uv}, Lightmap UV: {LmUv}, TexU Axis: {UVec}, TexV Axis: {VVec}, Texture: {TextureName})";
    }
}

public class Sob
{
    public Sob(List<Vector3> vertices, List<Polygon> polygons, List<string> unknowns)
    {
        Vertices = vertices;
        Polygons = polygons;

        Textures = new HashSet<string>();
        foreach (var poly in polygons)
        {
            Textures.Add(poly.TextureName);
        }

        Unknowns = unknowns;
    }

    public List<Vector3> Vertices { get; }
    public List<Polygon> Polygons { get; }
    public HashSet<string> Textures { get; }
    public List<string> Unknowns { get; }

    public void AddToMesh(
        Dictionary<string, MeshSurfaceData> surfaceDataMap,
        Vector3 offset,
        bool flip = false)
    {
        for (var i = 0; i < Polygons.Count; i++)
        {
            var poly = Polygons[i];
            if (surfaceDataMap.TryGetValue(poly.TextureName, out var meshData))
            {
                AddPolyToMesh(i, meshData, offset, flip);
            }
            else
            {
                meshData = new MeshSurfaceData();
                if (AddPolyToMesh(i, meshData, offset, flip))
                {
                    surfaceDataMap.Add(poly.TextureName, meshData);
                }
            }
        }
    }

    public bool AddPolyToMesh(
        int polyIdx,
        MeshSurfaceData meshData,
        Vector3 offset,
        bool flip = false)
    {
        var poly = Polygons[polyIdx];
        if (poly.Mode != Polygon.RenderMode.Normal || poly.TextureName == "none")
        {
            return false;
        }

        var vs = new List<Vector3>(poly.VertexCount);
        var uvs = new List<Vector2>(poly.VertexCount);
        for (var i = 0; i < poly.VertexCount; i++)
        {
            var idx = flip ? poly.VertexCount - 1 - i : i;
            vs.Add(Vertices[poly.Indices[idx]] + offset);
        }

        var anchor = vs[0];
        if (poly.CenterAnchor)
        {
            var min = vs[0];
            var max = vs[0];
            foreach (var v in vs)
            {
                min = min.Min(v);
                max = max.Max(v);
            }

            anchor = (min + max) / 2.0f;
        }

        var txSize = EditorContext.Instance.TextureManager.Textures[poly.TextureName].GetSize();
        var scale = poly.AltUvMode ? 0.5f : 1.0f;
        var uvOffset = new Vector2(poly.Uv.Y, poly.Uv.X) * scale;
        if (flip)
        {
            uvOffset.X = 1.0f - uvOffset.X;
        }

        var importScale = EditorContext.Instance.ImportScale;
        foreach (var p in vs)
        {
            var delta = importScale * (p - anchor);

            var u = (delta.Dot(poly.VVec) + uvOffset.X) / txSize.X;
            var v = (delta.Dot(poly.UVec) + uvOffset.Y) / txSize.Y;

            uvs.Add(new Vector2(u, v));
        }

        meshData.AddPolygon(vs, uvs);
        return true;
    }
}