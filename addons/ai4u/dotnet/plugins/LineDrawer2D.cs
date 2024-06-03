using System;
using System.Collections.Generic;
using Godot;

class Line
{
	public int id;
	public Color color;
	public float thickness;
	public Vector2 a;
	public Vector2 b;
	public float originSize;
	public Color originColor;
	public Line(int id, Vector2 a, Vector2 b, Color c, Color oc, float th, float originSize = 10)
	{
		this.id = id;
		this.a = a;
		this.b = b;
		this.color = c;
		this.originColor = oc;
		this.thickness = th;
		this.originSize = originSize;
	}
}


partial class LineDrawer2D: Node2D
{
	public List<Line> Lines;

	public override void _Ready()
	{
		Lines = new List<Line>();
	}

    private void StartLines()
    {
		if (Lines == null)
		{
       		Lines = new List<Line>();
       		SetProcess(true);
		}
    }

	public override void _Draw()
	{
		StartLines();
		foreach(Line line in Lines)
		{

			if (line.originSize > 0)
				DrawCircle(line.b, line.originSize*0.5f, line.originColor);
			    DrawLine(line.a, line.b, line.color, line.thickness);
			if (line.originSize > 0)
				DrawCircle(line.a, line.originSize, line.originColor);

		}
	}

	public void DrawLine(int id, Vector2 vector_a, Vector2 vector_b, Color color, Color originColor, float thickness, float originSize)
	{
		StartLines();
		foreach(Line line in Lines)
		{
			if (line.id == id)
			{
				line.color = color;
				line.a = vector_a;
				line.b = vector_b;
				line.thickness = thickness;
				line.originColor = originColor;
				line.originSize = originSize;
				return;
			}
		}

		var new_line = new Line(id, new Vector2(), new Vector2(), color, originColor, thickness, originSize);
		new_line.id = id;
		new_line.color = color;
		new_line.a = vector_a;
		new_line.b = vector_b;
		new_line.thickness = thickness;
		Lines.Add(new_line);
	}
	

	public void Remove_Line(int id)
	{
			int i = 0;
			bool found = false;
			Line removed = null;
			foreach(Line line in Lines)
			{
				if (line.id == id)
				{
					found = true;
					removed = line;
					break;
				}
				i += 1;
			}	
			if (found)
				Lines.Remove(removed);
	}
}
