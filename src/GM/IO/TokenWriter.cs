using System.Collections.Generic;
using System.IO;
using Godot;

namespace GM.IO;

public class TokenWriter
{
    private readonly List<string> _lines = new() {""};

    public void Save(string path)
    {
        File.WriteAllLines(path, _lines);
    }
    
    public void Write(int val)
    {
        if (val >= 0)
        {
            _lines[^1] += " " + val + " ";
        }
        else
        {
            _lines[^1] += val + " ";
        }
    }

    public void Write(float val)
    {
        if (val >= 0)
        {
            _lines[^1] += " " + val.ToString() + " ";
        }
        else
        {
            _lines[^1] += val.ToString() + " ";
        }
    }
    
    public void Write(Vector2 val, float scale)
    {
        Write(val.X * scale);
        Write(val.Y * scale);
    }

    public void Write(Vector3 val, float scale)
    {
        Write(val.Z * scale);
        Write(val.X * scale);
        Write(val.Y * scale);
    }

    public void Write(string val, bool addSpace = false)
    {
        if (addSpace)
        {
            _lines[^1] += " " + val + " ";
        }
        else
        {
            _lines[^1] += val;
        }
    }

    public void NewLine()
    {
        _lines[^1] += "\r";
        _lines.Add("");
    }
}