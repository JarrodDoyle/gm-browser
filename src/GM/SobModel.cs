using Godot;
using System.Collections.Generic;
using System.IO;

namespace GM;

[GlobalClass]
public partial class SobModel : Node3D
{
	[Export(PropertyHint.GlobalDir)]
	public string GameDir { get; set; }
	
	[Export]
	public string ObjectName { get; set; }

	[Export] public float ImportScale { get; set; } = 64.0f;

	private Sob _sob;

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
		var reader = new TokenReader(path);
		_sob = new Sob(reader, ImportScale);

		// TODO: Add string override to Sob?
		// GD.Print($"Vertex count: {vertexCount}\nPoly count: {polyCount}\nVertices: [\n  {string.Join("\n  ", vertices)}\n]\nPolys: [\n  {string.Join("\n  ", polys)}\n]");

		var textures = new Dictionary<string, ImageTexture>();
		foreach (var textureName in _sob.Textures)
		{
			var dir = $"{GameDir}/Pics/";
			var options = new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive };
			var paths = Directory.GetFiles(dir, $"{textureName}.bmp", options);
			if (!paths.IsEmpty())
			{
				var image = Image.LoadFromFile(paths[0]);
				var texture = ImageTexture.CreateFromImage(image);
				textures.Add(textureName, texture);
			}
			else
			{
				GD.Print($"Failed to find texture: {textureName}");
				textures.Add(textureName, ImageTexture.CreateFromImage(Image.CreateEmpty(256, 256, false, Image.Format.Rgb8)));
			}
		}
		
		GD.Print("Textures: [");
		foreach (var (name, texture) in textures)
		{
			GD.Print($"  Name: {name}, Size: {texture.GetSize()}");
		}
		GD.Print("]");
		
		var surfaceDataMap = new Dictionary<string, MeshSurfaceData>();
		_sob.AddToMesh(textures, surfaceDataMap);

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
}
