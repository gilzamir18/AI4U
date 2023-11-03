using Godot;
using System;

public partial class SpikeController : RigidBody2D
{

	[Export]
	private float speed = 5;

    [Export]
    private NodePath playerPath;

	[Export]
	private NodePath referencePath;

    private Transform2D reference;

    private BotController player;

	private int forward = 1;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		reference = GetNode<Node2D>(referencePath).Transform;
		player = GetNode<BotController>(playerPath);
		player.respawnEventHandler += RespawnHandler;
	}

	public void RespawnHandler()
	{
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
		if (node.GetGroups().Contains("player"))
		{
			player.Kill();
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

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		float f = speed * forward;
        ApplyCentralImpulse(new Vector2(f, 0));
    }
}
