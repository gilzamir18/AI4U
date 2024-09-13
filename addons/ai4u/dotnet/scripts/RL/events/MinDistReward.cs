using System.Collections;
using System.Collections.Generic;
using Godot;

namespace ai4u
{
	public partial class MinDistReward : RewardFunc
	{
		
		[Export]
		private NodePath target;
		private Node3D targetNode;
		
		[Export]
		private float successReward = 0.01f;
		
		[Export]
		private float stepReward = -0.001f;
		
		private RLAgent agent;
		private float minDistance;
		private PhysicsBody3D rBody;
		
		public override void OnSetup(Agent agent)
		{
			if (target != null)
			{
				targetNode = GetNode(target) as Node3D;
			}
            if (target == null)
            {
                GD.Print("MinDistReward error: Mandatory field (targetPath) not provided!");
            }

            this.agent = (RLAgent)agent;
			
			rBody = (PhysicsBody3D) this.agent.GetAvatarBody();
			
			this.agent.AddResetListener(this);
			//minDistance = (rBody.GlobalTransform.origin - targetNode.GlobalTransform.origin).Length();
			minDistance = System.Single.PositiveInfinity;
		}

		public void SetTarget(Node3D t)
		{
			this.targetNode = t;
		}
		
		public override void OnUpdate()
		{
			Vector3 d = rBody.GlobalTransform.Origin - targetNode.GlobalTransform.Origin;
			float dist = d.Length();
			if (dist < minDistance)
			{
				if (minDistance != System.Single.PositiveInfinity)
					this.agent.AddReward(successReward, this);
				minDistance = dist;
			} else
			{
				this.agent.AddReward(stepReward, this);
			}
		}

		public override void OnReset(Agent agent)
		{ 
			this.agent = (RLAgent) agent;
			//minDistance = (rBody.GlobalTransform.origin - targetNode.GlobalTransform.origin).Length();
			minDistance = System.Single.PositiveInfinity;
		}
	}
}

