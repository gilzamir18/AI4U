using System.Collections;
using System.Collections.Generic;
using Godot;

namespace ai4u
{
	public partial class MinDistReward : RewardFunc
	{
		
		[Export]
		private Node3D target;
		
		
		[Export]
		private float successReward = 0.01f;
		
		[Export]
		private float stepReward = -0.001f;
		
		private RLAgent agent;
		private float minDistance;
		private PhysicsBody3D rBody;

		public Node3D Target
		{
			get { return target; }
			set { target = value; }
		}

		public override void OnSetup(Agent agent)
		{

			if (target == null)
			{
				GD.Print("MinDistReward error: Mandatory field (targetPath) not provided!");
			}

			this.agent = (RLAgent)agent;

			rBody = (PhysicsBody3D)this.agent.GetAvatarBody();

			this.agent.AddResetListener(this);
			//minDistance = (rBody.GlobalTransform.origin - targetNode.GlobalTransform.origin).Length();
			minDistance = System.Single.PositiveInfinity;
		}

		public void SetTarget(Node3D t)
		{
			this.target = t;
		}
		
		public override void OnUpdate()
		{
			if (target == null || rBody == null)
			{
				this.agent.AddReward(stepReward, this);
				return;
			}
			Vector3 d = rBody.GlobalTransform.Origin - target.GlobalTransform.Origin;
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

