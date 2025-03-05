using System.Collections.Generic;
using Godot;
using GArray = Godot.Collections.Array;

namespace GME.Render;

public class MeshSurfaceData
{
    public bool Empty { get; private set; } = true;

    private readonly List<Vector3> _vertices = new();
    private readonly List<Vector3> _normals = new();
    private readonly List<int> _indices = new();
    private readonly List<int> _indicesLine = new();
    private readonly List<Vector2> _uvs = new();
    
    // TODO: Guard against empty polygons being added
    public void AddPolygon(
        List<Vector3> vertices,
        List<Vector2> uvs)
    {
        Empty = false;

        var vertexCount = vertices.Count;
        var indexOffset = _vertices.Count;
        _vertices.AddRange(vertices);

        var ab = vertices[1] - vertices[0];
        var bc = vertices[2] - vertices[1];
        var normal = ab.Cross(bc).Normalized();
        for (var i = 0; i < vertexCount; i++)
        {
            _normals.Add(normal);
        }

        // Simple triangulation. Polys are always convex so we can just do a fan
        for (var j = 1; j < vertexCount - 1; j++)
        {
            _indices.Add(indexOffset);
            _indices.Add(indexOffset + j);
            _indices.Add(indexOffset + j + 1);
        }

        for (var i = 0; i < vertexCount; i++)
        {
            _indicesLine.Add(indexOffset + i);
            _indicesLine.Add(indexOffset + (i + 1) % vertexCount);
        }

        _uvs.AddRange(uvs);
    }

    public GArray BuildSurfaceArray(bool lineMesh = false)
    {
        var indices = lineMesh ? _indicesLine : _indices;
        
        var array = new GArray();
        array.Resize((int)Mesh.ArrayType.Max);
        array[(int)Mesh.ArrayType.Vertex] = _vertices.ToArray();
        array[(int)Mesh.ArrayType.Normal] = _normals.ToArray();
        array[(int)Mesh.ArrayType.Index] = indices.ToArray();
        array[(int)Mesh.ArrayType.TexUV] = _uvs.ToArray();

        return array;
    }
}