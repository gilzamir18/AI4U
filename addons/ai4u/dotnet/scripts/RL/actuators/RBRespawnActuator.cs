using Godot;
using System;
using Godot.Collections;

namespace ai4u
{
	/// <summary>
	/// This class represents an agent respawn operation.
	/// Repositioning in this class can be performed either **before** any other object is reset 
	/// (when the "early" property is enabled) or **after** (when the "early" property is not enabled).
	/// </summary>
	public partial class RBRespawnActuator : Actuator
	{	
		
			[Export]
			private NodePath respawnOptionsPath;
			

			[Export]
			private bool early = true;

			private Node nodeRef;
		
			private RigidBody3D rBody;
			private Godot.Collections.Array<Node> children;


			private int lastSelected = 0;


			public int LastSelected {
				get {
					return lastSelected;
				}
			}
		

			public override void OnSetup(Agent agent)
			{
				nodeRef = GetNode(respawnOptionsPath);
				children = nodeRef.GetChildren();
				rBody = ( (BasicAgent) agent).GetAvatarBody() as RigidBody3D;

				if (early)
				{
					((BasicAgent)agent).OnResetStart += HandleReset;
				}
				else
				{
					((BasicAgent)agent).OnEpisodeStart += HandleReset;
				}
			
			}
			
			public void HandleReset(BasicAgent agent)
			{
				Transform3D reference;
				if (children.Count > 0)
				{
					int idx = (int)GD.RandRange(0, children.Count-1);
					reference = ((Node3D)children[idx]).GlobalTransform;
					lastSelected = idx;
				}
				else
				{
					reference = ((Node3D) nodeRef).GlobalTransform;
					lastSelected = -1;
				}
				/*var mode = rBody.Mode;
				rBody.Mode = RigidBody3D.ModeEnum.Kinematic;
				rBody.Position = children[idx].position;
				rBody.Mode = mode;*/
				
				PhysicsServer3D.BodySetState(
					rBody.GetRid(),
					PhysicsServer3D.BodyState.Transform,
					reference
				);
				
				PhysicsServer3D.BodySetState(
					rBody.GetRid(),
					PhysicsServer3D.BodyState.AngularVelocity,
					new Vector3(0, 0, 0)
				);	
				
				PhysicsServer3D.BodySetState(
					rBody.GetRid(),
					PhysicsServer3D.BodyState.LinearVelocity,
					new Vector3(0, 0, 0)
				);	
			}
	}
}
