using Godot;
using System;
using ai4u;

namespace ai4u
{
	public class TouchRewardFunc : RewardFunc
	{
		
		[Export]
		public float reward = 0.0f;
		[Export]
		public NodePath targetPath;
		private Node target;
		private float acmReward = 0.0f;
		private BasicAgent agent;
		
		public override void OnSetup(Agent agent)
		{
			this.agent = (BasicAgent) agent;
			target = GetNode(targetPath);
			RigidBody body = this.agent.GetAvatarBody() as RigidBody;
			body.Connect("body_entered", this, nameof(body_entered));
		}
		
		public void body_entered(Node node)
		{
			if (node == target)
			{
				acmReward += this.reward;
			}
		}

		public override void OnUpdate()
		{
			this.agent.AddReward(acmReward, this);
			acmReward = 0.0f;
		}

		public override void OnReset(Agent agent)
		{
			acmReward = 0.0f;
		}
	}
}

