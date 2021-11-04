using Godot;
using System;
using ai4u;

public class SwapPositions : Spatial, IAgentResetListener
{
	
	[Export]
	public NodePath otherObject;
	[Export]
	public NodePath agent;
	
	[Export]
	public bool hide = false;
	
	private Random random;
	public override void _Ready()
	{
		random = new Random();
		(GetNode(agent) as Agent).AddResetListener(this);
		OnReset(null);
	}
	
	public void OnReset(Agent agent)
	{
			Spatial node = (GetNode(otherObject) as Spatial);
			if (random.Next(0,2) == 1)
			{
				Vector3 pos = (GetNode(otherObject) as Spatial).Translation;
				Vector3 tmp = Translation;
				Translation = pos;
				node.Translation = tmp;
				if (hide)
				{
					this.Hide();
					node.Show();
				}
			}
			else
			{
				if (hide)
				{
					this.Show();
					node.Hide();
				}
			}
	}
}
