using Godot;
using System;
using ai4u;

namespace ai4u.ext
{
	public class RigidBodyTouchRewardFunc : RewardFunc
	{
		[Export]
		public float reward = 0.0f;
		[Export]
		public NodePath targetPath;
		private Node target;

		public override void OnCreate()
		{
			target = GetNode(targetPath);
			RigidBody body = agent.GetBody() as RigidBody;
			body.Connect("body_entered", this, nameof(body_entered));
		}
		
		public void body_entered(Node node)
		{
			if (node == target)
			{
				NotifyAll(reward);
				this.agent.AddReward(this.reward, this, endEpisodeInSuccess);	
			} 
		}

		public override void OnReset(Agent agent)
		{
		}
	}
}

