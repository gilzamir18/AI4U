using Godot;
using System;
using ai4u;

namespace ai4u.ext
{
	public class KinematicBodyTouchRewardFunc : RewardFunc
	{
		
		private KinematicBody body;
		[Export]
		public float reward = 0.0f;
		[Export]
		public NodePath targetPath;
		private Node target;
		
		public override void OnCreate()
		{
			target = GetNode(targetPath);
			body = agent.GetBody() as KinematicBody;
		}
	
		public override void _PhysicsProcess(float delta)
		{
			for (int i = 0; i < body.GetSlideCount(); i++)
			{
				var collision = body.GetSlideCollision(i);
				Node node = (Spatial)collision.Collider;
				if (node.Name == target.Name)
				{
					NotifyAll(reward);
					agent.AddReward(reward, this, endEpisodeInSuccess);
				}
			}
		}
	}
}
