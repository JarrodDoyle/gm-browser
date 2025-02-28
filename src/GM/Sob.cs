using System.Collections.Generic;
using GM.IO;
using Godot;

namespace GM;

public struct Polygon
{
	public enum RenderMode
	{
		Normal,
		Sector,
		Hole,
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
            reader.ReadString(1);
            var mode = (Polygon.RenderMode)reader.ReadInt();
            reader.ReadString(2);
			
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
                Mode = mode,
            });
        }

        Textures = new HashSet<string>();
        foreach (var poly in Polygons)
        {
            Textures.Add(poly.TextureName);
        }
    }

    public void AddToMesh(
	    TextureManager textureManager,
	    Dictionary<string, MeshSurfaceData> surfaceDataMap,
	    Vector3 offset,
	    bool flip = false)
    {
	    for (var i = 0; i < Polygons.Count; i++)
	    {
		    var poly = Polygons[i];
		    if (surfaceDataMap.TryGetValue(poly.TextureName, out var meshData))
		    {
			    AddPolyToMesh(i, textureManager, meshData, offset, flip);
		    }
		    else
		    {
			    meshData = new MeshSurfaceData();
			    if (AddPolyToMesh(i, textureManager, meshData, offset, flip))
			    {
				    surfaceDataMap.Add(poly.TextureName, meshData);
			    }
		    }
	    }
    }

    public bool AddPolyToMesh(
	    int polyIdx,
	    TextureManager textureManager,
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
		    var idx = flip ? (poly.VertexCount - 1 - i) : i;
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

	    var txSize = textureManager.Textures[poly.TextureName].GetSize();
	    var scale = poly.AltUvMode ? 0.5f : 1.0f;
	    var uvOffset = new Vector2(poly.Uv.Y, poly.Uv.X) * scale;
        	
	    foreach (var p in vs)
	    {
		    var delta = ImportScale * (p - anchor);

		    var u = (delta.Dot(poly.VVec) + uvOffset.X) / txSize.X;
		    var v = (delta.Dot(poly.UVec) + uvOffset.Y) / txSize.Y;
        		
		    uvs.Add(new Vector2(u, v));
	    }
	    
	    meshData.AddPolygon(vs, uvs);
	    return true;
    }
}