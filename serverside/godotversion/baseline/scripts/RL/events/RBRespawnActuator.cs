using Godot;
using System;
using Godot.Collections;

namespace ai4u
{
	public class RBRespawnActuator : Actuator
	{	
		
			[Export]
			private NodePath respawnOptionsPath;
			
			private Node nodeRef;
		
			private RigidBody rBody;
			private Godot.Collections.Array children;
			
			public override void OnSetup(Agent agent)
			{
				nodeRef = GetNode(respawnOptionsPath);
				children = nodeRef.GetChildren();
				rBody = ( (BasicAgent) agent).GetAvatarBody() as RigidBody;
				((BasicAgent)agent).beforeTheResetEvent += HandleReset;
			}
			
			public void HandleReset(BasicAgent agent)
			{
				int idx = (int)GD.RandRange(0, children.Count);
				
				/*var mode = rBody.Mode;
				rBody.Mode = RigidBody.ModeEnum.Kinematic;
				rBody.Position = children[idx].position;
				rBody.Mode = mode;*/
				
				PhysicsServer.BodySetState(
					rBody.GetRid(),
					PhysicsServer.BodyState.Transform,
					((Spatial)children[idx]).GlobalTransform		
				);
				
				PhysicsServer.BodySetState(
					rBody.GetRid(),
					PhysicsServer.BodyState.AngularVelocity,
					new Vector3(0, 0, 0)
				);	
				
				PhysicsServer.BodySetState(
					rBody.GetRid(),
					PhysicsServer.BodyState.LinearVelocity,
					new Vector3(0, 0, 0)
				);	
			}
	}
}
