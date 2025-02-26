using System.Collections.Generic;
using System.IO;
using Godot;

namespace GM;

public class TokenReader
{
    private readonly Queue<string> _tokens;

    public TokenReader(string path)
    {
        var tokensList = new List<string>();
        foreach (var line in File.ReadAllLines(path))
        {
            tokensList.AddRange(line.Split(' '));
        }

        tokensList.RemoveAll(string.IsNullOrEmpty);
        _tokens = new Queue<string>(tokensList);
    }

    public int ReadInt()
    {
        return int.Parse(_tokens.Dequeue());
    }
	
    public List<int> ReadInt(int count)
    {
        var values = new List<int>(count);
        for (var i = 0; i < count; i++)
        {
            values.Add(ReadInt());
        }
        return values;
    }

    public float ReadFloat()
    {
        return float.Parse(_tokens.Dequeue());
    }
    
    public List<float> ReadFloat(int count)
    {
        var values = new List<float>(count);
        for (var i = 0; i < count; i++)
        {
            values.Add(ReadFloat());
        }
        return values;
    }
    
    public Vector2 ReadVector2(float scale)
    {
        var x = ReadFloat();
        var y = ReadFloat();
        return new Vector2(x, y) / scale;
    }
	
    public Vector3 ReadVector3(float scale)
    {
        var x = ReadFloat();
        var y = ReadFloat();
        var z = ReadFloat();
        return new Vector3(y, z, x) / scale;
    }
    
    public List<Vector3> ReadVector3(float scale, int count)
    {
        var values = new List<Vector3>(count);
        for (var i = 0; i < count; i++)
        {
            values.Add(ReadVector3(scale));
        }
        return values;
    }

    public string ReadString()
    {
        return _tokens.Dequeue();
    }
    
    public List<string> ReadString(int count)
    {
        var values = new List<string>(count);
        for (var i = 0; i < count; i++)
        {
            values.Add(_tokens.Dequeue());
        }
        return values;
    }
}