using Godot;
using System;

public partial class FruitController : Node2D
{


    [Export]
    private NodePath playerPath;

	[Export] 
	private NodePath positionsPath;

	[Export]
	private int timeToRespawn = 1000;
	private int countTimeToRespawn = 0;

	[Export]
	private int energyGain = 100;

	[Export]
	private bool superPower = false;

	[Export]
	private int superPowerGain = 1000;

	private bool eaten = false;
	private Node positions;

    
    private BotController player;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		positions = GetNode<Node>(positionsPath);
		player = GetNode<BotController>(playerPath);
		player.respawnEventHandler += Respwan;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (eaten)
		{
			countTimeToRespawn++;
			if (countTimeToRespawn > timeToRespawn)
			{
				Respwan();
			}
		}
	}

	public void Respwan()
	{
		Visible = true;
		eaten = false;
		countTimeToRespawn = 0;
		var children = positions.GetChildren();
		int n = children.Count;
		if (n > 0)
		{
			int idx = (new RandomNumberGenerator()).RandiRange(0, children.Count - 1);
            Position = ((Node2D)children[idx]).Position;
			
		}
		else
		{
			Position = ((Node2D)positions).Position;
		}
	}

    private void _on_body_shape_entered(Rid rid, Node2D other, int bodyShapeIndex, int localShapeIndex)
	{
		if (!eaten)
		{
			if (other.GetGroups().Contains("player"))
			{
				Visible = false;
				countTimeToRespawn = 0;
				eaten = true;
				player.AddEnergy(energyGain);
				if (superPower)
				{
					player.AddSuperPower(superPowerGain);
				}
			}
		}
	}
}
