using Godot;
using System;
using ai4u;
using System.Collections.Generic;

public partial class RespawnTarget : RigidBody3D, IAgentResetListener
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	[Export]
	private NodePath positions;

	[Export]
	private NodePath agentPath;
	private BasicAgent agent;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		agent = GetNode(agentPath) as BasicAgent;
		agent.AddResetListener(this);
	}

	public void OnReset(Agent agent)
	{
				var children = GetNode(positions).GetChildren();
				int idx = (int)GD.RandRange(0, children.Count-1);
				
				/*var mode = rBody.Mode;
				rBody.Mode = RigidBody3D.ModeEnum.Kinematic;
				rBody.Position = children[idx].position;
				rBody.Mode = mode;*/

				PhysicsServer3D.BodySetState(
					GetRid(),
					PhysicsServer3D.BodyState.Transform,
					((Node3D)children[idx]).GlobalTransform
				);
				
				PhysicsServer3D.BodySetState(
					GetRid(),
					PhysicsServer3D.BodyState.AngularVelocity,
					new Vector3(0, 0, 0)
				);	
				
				PhysicsServer3D.BodySetState(
					GetRid(),
					PhysicsServer3D.BodyState.LinearVelocity,
					new Vector3(0, 0, 0)
				);
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
