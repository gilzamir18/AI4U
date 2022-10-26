using Godot;
using System;
using ai4u;
namespace ai4u {
	public class TouchRewardFunc : RewardFunc {
		[Export]
		public float reward = 0.0f;
		[Export]
		public NodePath targetPath;
		private Node target;
		private float acmReward = 0.0f;
		private BasicAgent agent;
		
		public override void OnSetup(Agent agent) {
			agent.AddResetListener(this);
			this.agent = (BasicAgent) agent;
			target = GetNode(targetPath);
			RigidBody body = this.agent.GetAvatarBody() as RigidBody;
			body.Connect("body_shape_entered", this, nameof(body_shape_entered));
		}
		
		public void body_shape_entered(RID body_rid, Node body,
										int body_shape_index,
										int local_shape_index ) {
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

