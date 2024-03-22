using Godot;
using System;
using System.Collections.Generic;

public partial class Floor : StaticBody3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		/*
		var mymesh = GetChild<MeshInstance3D>(1);
		Vector3[] vertices = mymesh.Mesh.GetFaces();
		List<Vector3> centroides = new List<Vector3>();
		for (int i = 0; i < vertices.Length; i += 3)
		{
			Vector3 c = (vertices[i] + vertices[i+1] + vertices[i+2])/3.0f;
			centroides.Add( new Vector3(c.X * mymesh.Scale.X, 1.0f, c.Z * mymesh.Scale.Z) );
		}

		for (int i = 0; i < 10; i++)
		{
			var pos = (int)(GD.Randf() * centroides.Count);
			Vector3 ct = centroides[pos];
			var cube = new BoxMesh();
			var instance = new MeshInstance3D();
			instance.Mesh = cube;
			instance.Position =  ct + new Vector3(0, 2.0f, 0) + mymesh.Position;
			instance.Scale = new Vector3(1, 4, 1);
			AddChild(instance);
		}*/
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
