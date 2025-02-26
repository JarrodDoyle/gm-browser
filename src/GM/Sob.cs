using System.Collections.Generic;
using Godot;

namespace GM;

public struct Polygon
{
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

    public override string ToString()
    {
        return $"Poly(CenterAnchor: {CenterAnchor}, Flags: {Brightness}, VertexCount: {VertexCount}, Indices: [{string.Join(", ", Indices)}], UV: {Uv}, Lightmap UV: {LmUv}, TexU Axis: {UVec}, TexV Axis: {VVec}, Texture: {TextureName})";
    }
}

public class Sob
{
    public float ImportScale { get; }
    public List<Vector3> Vertices { get; }
    public List<Polygon> Polygons { get; }
    public HashSet<string> Textures { get; }

    public Sob(TokenReader reader, float importScale)
    {
        ImportScale = importScale;
        
        var vertexCount = reader.ReadInt();
        var polyCount = reader.ReadInt();
        
        Vertices = new List<Vector3>(vertexCount);
        for (var i = 0; i < vertexCount; i++)
        {
            Vertices.Add(reader.ReadVector3(ImportScale));
        }
        
        Polygons = new List<Polygon>(polyCount);
        for (var i = 0; i < polyCount; i++)
        {
            var textureName = reader.ReadString();
            reader.ReadString();
            var anchorMode = reader.ReadString();
            reader.ReadString(5);
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
            reader.ReadString(4);
			
            Polygons.Add(new Polygon {
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
            });
        }

        Textures = new HashSet<string>();
        foreach (var poly in Polygons)
        {
            Textures.Add(poly.TextureName);
        }
    }
}