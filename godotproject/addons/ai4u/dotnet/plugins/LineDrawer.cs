using System.Collections.Generic;
using Godot;

partial class LineDrawer: MeshInstance3D
{
	private List<Vector3> linesA;

	public Camera3D Camera_Node;

	private ImmediateMesh imesh;

	public LineDrawer()
	{
		linesA = new ();	
	}

	public void StartMeshes()
	{
		linesA = new ();
		imesh = new ImmediateMesh();
		Mesh = imesh;
	}

	public void SetColor(Color c)
	{
		var material = new StandardMaterial3D();
        material.AlbedoColor = c;
		this.MaterialOverride = material;
	}

	public void AddLine(Vector3 a, Vector3 b)
	{
		linesA.Add(a);
		linesA.Add(b);
	}

	public void Clear()
	{
		if (linesA == null)
		{
			linesA = new();
		}
		linesA.Clear();

		if (imesh == null)
		{
			imesh = new ImmediateMesh();
		}
		imesh.ClearSurfaces();
	}

	public void DrawLines()
	{
		imesh.SurfaceBegin(Mesh.PrimitiveType.Lines);
		for (int i  = 0; i < linesA.Count; i++)
		{
			var a = linesA[i];
			imesh.SurfaceSetColor(new Color(1, 0, 0, 1));
			imesh.SurfaceAddVertex(a);
		}
		imesh.SurfaceEnd();
		Mesh = imesh;
	}
}
