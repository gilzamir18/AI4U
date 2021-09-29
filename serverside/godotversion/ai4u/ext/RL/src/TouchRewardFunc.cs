using Godot;
using System;
using ai4u;

namespace ai4u.ext
{
	public class TouchRewardFunc : RewardFunc
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
				this.agent.AddReward(this.reward, this);	
			}
		}

		public override void OnReset(Agent agent)
		{
			
		}
	}
}

