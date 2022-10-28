using System.Collections;
using System.Collections.Generic;
using Godot;

namespace ai4u
{
	public class MinDistReward : RewardFunc
	{
		
		[Export]
		private NodePath target;
		private Spatial targetNode;
		
		[Export]
		private float successReward = 0.01f;
		
		[Export]
		private float stepReward = -0.001f;
		
		private BasicAgent agent;
		private float minDistance;
		private RigidBody rBody;
		
		public override void OnSetup(Agent agent)
		{
			targetNode = GetNode(target) as Spatial;
			this.agent = (BasicAgent)agent;
			
			rBody = (RigidBody) this.agent.GetAvatarBody();
			
			this.agent.AddResetListener(this);
			//minDistance = (rBody.GlobalTransform.origin - targetNode.GlobalTransform.origin).Length();
			minDistance = System.Single.PositiveInfinity;
		}

		// Update is called once per frame
		public override void OnUpdate()
		{
			Vector3 d = rBody.GlobalTransform.origin - targetNode.GlobalTransform.origin;
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
			this.agent = (BasicAgent) agent;
			//minDistance = (rBody.GlobalTransform.origin - targetNode.GlobalTransform.origin).Length();
			minDistance = System.Single.PositiveInfinity;
		}
	}
}

