using Godot;
using System;

public partial class SpikeController : RigidBody2D
{

	[Export]
	private float speed = 5;

	[Export]
	private int energyGain = 100;

	[Export]
	private int superPowerDecay = 500;

    [Export]
    private NodePath playerPath;

	[Export]
	private NodePath referencePath;

    private Transform2D reference;

    private BotController player;

	private int forward = 1;

	private bool killed = false;
	[Export]
	private int timeToRespawn = 1000;
	private int timeToRespawnCounter = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		reference = GetNode<Node2D>(referencePath).Transform;
		player = GetNode<BotController>(playerPath);
		player.respawnEventHandler += RespawnHandler;
	}

	public void RespawnHandler()
	{
		timeToRespawnCounter = 0;
		killed = false;
		Visible = true;
		CollisionLayer = 1;
		PhysicsServer2D.BodySetState(
			GetRid(),
			PhysicsServer2D.BodyState.Transform,
			reference
		);
				
		PhysicsServer2D.BodySetState(
			GetRid(),
			PhysicsServer2D.BodyState.AngularVelocity,
			new Vector3(0, 0, 0)
		);	
				
		PhysicsServer2D.BodySetState(
			GetRid(),
			PhysicsServer2D.BodyState.LinearVelocity,
			new Vector3(0, 0, 0)
		);
    }

	private void _on_body_entered(Node node)
	{
		if (killed)
		{
			return;
		}
		else if (player.State == StateEnum.live)
		{
			if (node.GetGroups().Contains("player"))
			{
				if (player.SuperPower > 0)
				{
					player.AddEnergy(energyGain);
					Visible = false;
					killed = true;
					CollisionLayer = 2;
					player.AddSuperPower(-superPowerDecay);
				}
				else
				{
					player.Kill();
				}
			}
			else if (node.GetGroups().Contains("left_wall"))
			{
				forward = 1;
			}
			else if (node.GetGroups().Contains("right_wall"))
			{
				forward = -1;
			}
		}
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (player.State == StateEnum.live)
		{
			if (!killed)
			{
				float f = speed * forward;
				ApplyCentralImpulse(new Vector2(f, 0));
			}
			else
			{
				timeToRespawnCounter++;
				//GD.Print(timeToRespawnCounter);
				if (timeToRespawnCounter >= timeToRespawn)
				{
					RespawnHandler();
				}
			}
		}
    }
}
