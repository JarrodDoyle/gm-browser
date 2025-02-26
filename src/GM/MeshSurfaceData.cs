using System.Collections.Generic;
using Godot;
using GArray = Godot.Collections.Array;

namespace GM;

public class MeshSurfaceData
{
    public bool Empty { get; private set; } = true;

    private readonly List<Vector3> _vertices = new();
    private readonly List<Vector3> _normals = new();
    private readonly List<int> _indices = new();
    private readonly List<Vector2> _uvs = new();
    
    // TODO: Guard against empty polygons being added
    public void AddPolygon(
        List<Vector3> vertices,
        Vector3 normal,
        List<Vector2> uvs)
    {
        Empty = false;

        var vertexCount = vertices.Count;
        var indexOffset = _vertices.Count;
        _vertices.AddRange(vertices);

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

        _uvs.AddRange(uvs);
    }

    public GArray BuildSurfaceArray()
    {
        var array = new GArray();
        array.Resize((int)Mesh.ArrayType.Max);
        array[(int)Mesh.ArrayType.Vertex] = _vertices.ToArray();
        array[(int)Mesh.ArrayType.Normal] = _normals.ToArray();
        array[(int)Mesh.ArrayType.Index] = _indices.ToArray();
        array[(int)Mesh.ArrayType.TexUV] = _uvs.ToArray();

        return array;
    }
}