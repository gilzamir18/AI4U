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
	public partial class CBRespawnActuator2D : Actuator
	{	
		
			[Export]
			private NodePath respawnOptionsPath;

			[Export]
			private bool radomizeDirection = true;

			[Export]
			private bool early = true;

			[Export]
			private bool flipWhenTurn = true;


			private Node nodeRef;
		
			private CharacterBody2D cBody;
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
				cBody = ( (BasicAgent) agent).GetAvatarBody() as CharacterBody2D;

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
				Transform2D reference;
				if (children.Count > 0)
				{
					int idx = (int)GD.RandRange(0, children.Count-1);
					reference = ((Node2D)children[idx]).GlobalTransform;
					lastSelected = idx;
				}
				else
				{
					reference = ((Node2D) nodeRef).GlobalTransform;
					lastSelected = -1;
				}
                cBody.Velocity = Vector2.Zero;
				cBody.Transform = reference;
                cBody.Position = reference.Origin;
				if (flipWhenTurn)
				{
					if (GD.RandRange(0, 1) > 0 && cBody.Transform.Scale.Y < 0)
					{
						cBody.Scale *= new Vector2(-1, 1);
					}
					else if (cBody.Transform.Scale.Y > 0)
					{
						cBody.Scale *= new Vector2(-1, 1);
					}
				}
				else
				{
					cBody.Rotate(Mathf.DegToRad(-GD.RandRange(0, 360)));
				}
			}
	}
}