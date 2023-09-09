using Godot;
using System;
using ai4u;
namespace ai4u {
	public partial class TouchRewardFunc : RewardFunc {
		[Export]
		public float reward = 0.0f;
		[Export]
		public NodePath targetPath;
		private Node target;
		private float acmReward = 0.0f;
		private BasicAgent agent;
		private bool configured = false;
		
		public override void OnSetup(Agent agent) {
			if (!configured)
			{
				configured = true;
				agent.AddResetListener(this);
				this.agent = (BasicAgent) agent;
				target = GetNode(targetPath);
				RigidBody3D body = this.agent.GetAvatarBody() as RigidBody3D;
				body.BodyEntered += OnEntered;
			}
		}
		
		public void OnEntered(Node body) {
			if (body == target) {
				acmReward += this.reward;
			}
		}

		public override void OnUpdate() {
			this.agent.AddReward(acmReward, this);
			acmReward = 0.0f;
		}

		public override void OnReset(Agent agent) {
			acmReward = 0.0f;
		}
	}
}

