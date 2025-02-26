using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GM;

[GlobalClass]
public partial class Sob : Node3D
{
	[Export(PropertyHint.GlobalDir)]
	public string GameDir { get; set; }
	
	[Export]
	public string ObjectName { get; set; }

	[Export] public float ImportScale { get; set; } = 64.0f;
	
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

	private string GetObjectPath()
	{
		var dir = $"{GameDir}/SOBS/";
		var options = new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive };
		var paths = Directory.GetFiles(dir, $"{ObjectName}.sob", options);
		return paths.IsEmpty() ? "" : paths[0];
	}
	
	public override void _Ready()
	{
		var path = GetObjectPath();
		var tokensList = new List<string>();
		foreach (var line in File.ReadAllLines(path))
		{
			tokensList.AddRange(line.Split(' '));
		}

		tokensList.RemoveAll(string.IsNullOrEmpty);

		GD.Print($"[{string.Join(", ", tokensList)}]");

		var tokens = new Queue<string>(tokensList);
		var vertexCount = ReadInt(tokens);
		var polyCount = ReadInt(tokens);
		var vertices = new List<Vector3>(vertexCount);
		for (var i = 0; i < vertexCount; i++)
		{
			vertices.Add(ReadVector3(tokens, ImportScale));
		}
		
		var polys = new List<Polygon>(polyCount);
		for (var i = 0; i < polyCount; i++)
		{
			var textureName = ReadString(tokens);
			ReadSkip(tokens, 1);
			var anchorMode = ReadString(tokens);
			ReadSkip(tokens, 5);
			var brightness = ReadInt(tokens);
			var vCount = ReadInt(tokens);
			var indices = ReadInt(tokens, vCount);
			var uv0 = ReadInt(tokens);
			var uv1 = ReadInt(tokens);
			var uv2 = ReadInt(tokens);
			var uv = (uv0 == -1 ? new Vector2(uv1, uv2) : new Vector2(uv1, uv0)) / 128.0f;
			var altUvMode = uv0 == -1;
			var uvec = ReadVector3(tokens, 65536.0f);
			var vvec = ReadVector3(tokens, 65536.0f);
			var lmuv = new Vector2(ReadInt(tokens), ReadInt(tokens)) / 128.0f;
			ReadSkip(tokens, 4);
			
			polys.Add(new Polygon {
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
		
		GD.Print($"Vertex count: {vertexCount}\nPoly count: {polyCount}\nVertices: [\n  {string.Join("\n  ", vertices)}\n]\nPolys: [\n  {string.Join("\n  ", polys)}\n]");

		var textures = new Dictionary<string, ImageTexture>();
		var surfaceDataMap = new Dictionary<string, MeshSurfaceData>();
		foreach (var poly in polys)
		{
			if (!textures.ContainsKey(poly.TextureName))
			{
				var dir = $"{GameDir}/Pics/";
				var options = new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive };
				var paths = Directory.GetFiles(dir, $"{poly.TextureName}.bmp", options);
				if (!paths.IsEmpty())
				{
					var image = Image.LoadFromFile(paths[0]);
					var texture = ImageTexture.CreateFromImage(image);
					textures.Add(poly.TextureName, texture);
				}
				else
				{
					GD.Print($"Failed to find texture: {poly.TextureName}");
					textures.Add(poly.TextureName, ImageTexture.CreateFromImage(Image.CreateEmpty(256, 256, false, Image.Format.Rgb8)));
				}
			}
			
			var vs = new List<Vector3>(poly.VertexCount);
			var uvs = new List<Vector2>(poly.VertexCount);
			foreach (var i in poly.Indices)
			{
				vs.Add(vertices[i]);
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

			var txSize = textures[poly.TextureName].GetSize();
			var scale = poly.AltUvMode ? 0.5f : 1.0f;
			var uvOffset = new Vector2(poly.Uv.Y, poly.Uv.X) * scale;
			
			foreach (var p in vs)
			{
				var delta = ImportScale * (p - anchor);

				var u = (delta.Dot(poly.VVec) + uvOffset.X) / txSize.X;
				var v = (delta.Dot(poly.UVec) + uvOffset.Y) / txSize.Y;
				
				uvs.Add(new Vector2(u, v));
			}
			
			if (!surfaceDataMap.ContainsKey(poly.TextureName))
			{
				surfaceDataMap.Add(poly.TextureName, new MeshSurfaceData());
			}
			surfaceDataMap[poly.TextureName].AddPolygon(vs, uvs);
		}

		
		GD.Print("Textures: [");
		foreach (var (name, texture) in textures)
		{
			GD.Print($"  Name: {name}, Size: {texture.GetSize()}");
		}
		GD.Print("]");

		var lineMeshMaterial = new StandardMaterial3D();
		lineMeshMaterial.RenderPriority = 1;
		
		// TODO: Can we just do a fan? Need to check if FSDS allows concave polys
		// TODO: Linemesh only needs one set of surface data
		var mesh = new ArrayMesh();
		var lineMesh = new ArrayMesh();
		foreach (var (textureName, surfaceData) in surfaceDataMap)
		{
			var material = new StandardMaterial3D
			{
				AlbedoTexture = textures[textureName],
				TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest,
			};
			var array = surfaceData.BuildSurfaceArray();
			var surfaceIdx = mesh.GetSurfaceCount();
			mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, array);
			mesh.SurfaceSetMaterial(surfaceIdx, material);
			
			var lineArray = surfaceData.BuildSurfaceArray(true);
			lineMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Lines, lineArray);
			lineMesh.SurfaceSetMaterial(surfaceIdx, lineMeshMaterial);
		}
		
		var meshInstance = new MeshInstance3D { Mesh = mesh, Position = Vector3.Zero };
		var lineMeshInstance = new MeshInstance3D { Mesh = lineMesh, Position = Vector3.Zero };
		AddChild(meshInstance);
		AddChild(lineMeshInstance);
	}

	private static int ReadInt(Queue<string> tokens)
	{
		return int.Parse(tokens.Dequeue());
	}
	
	private static List<int> ReadInt(Queue<string> tokens, int count)
	{
		var values = new List<int>(count);
		for (var i = 0; i < count; i++)
		{
			values.Add(ReadInt(tokens));
		}
		return values;
	}

	private static float ReadFloat(Queue<string> tokens)
	{
		return float.Parse(tokens.Dequeue());
	}
	
	private static Vector3 ReadVector3(Queue<string> tokens, float scale)
	{
		var x = ReadFloat(tokens);
		var y = ReadFloat(tokens);
		var z = ReadFloat(tokens);
		return new Vector3(y, z, x) / scale;
	}

	private static string ReadString(Queue<string> tokens)
	{
		return tokens.Dequeue();
	}

	private static void ReadSkip(Queue<string> tokens, int count)
	{
		for (var i = 0; i < count; i++)
		{
			tokens.Dequeue();
		}
	}
}
