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
	public partial class CBRespawnActuator : Actuator
	{	
		
			[Export]
			private NodePath respawnOptionsPath;

			[Export]
			private bool radomizeDirection = true;

			[Export]
			private bool early = true;


			private Node nodeRef;
		
			private CharacterBody3D cBody;
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
				cBody = ( (RLAgent) agent).GetAvatarBody() as CharacterBody3D;

				if (early)
				{
					((RLAgent)agent).OnResetStart += HandleReset;
				}
				else
				{
					((RLAgent)agent).OnEpisodeStart += HandleReset;
				}
			
			}
			
			public void HandleReset(RLAgent agent)
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
				var bscale = cBody.Scale;
				cBody.Velocity = Vector3.Zero;
				cBody.GlobalTransform = reference;
                cBody.GlobalPosition = reference.Origin;
				if (radomizeDirection)
				{
                	cBody.Rotate(cBody.Basis.Y, (float)GD.RandRange(0.0, 2.0 * Mathf.Pi));
				}
			    cBody.Scale = bscale;
        }
	}
}
