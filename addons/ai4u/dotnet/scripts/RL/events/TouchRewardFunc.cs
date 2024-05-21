using Godot;
using System;
using ai4u;
namespace ai4u {
	public partial class TouchRewardFunc : RewardFunc {
		[Export]
		public float reward = 0.0f;
		[Export]
		private Node target;
		private float acmReward = 0.0f;
		private BasicAgent agent;
		private bool configured = false;
		private bool eval = false;
		
		public override void OnSetup(Agent agent) {
			if (!configured)
			{
				eval = false;
				configured = true;
				agent.AddResetListener(this);
				this.agent = (BasicAgent) agent;
				RigidBody3D body = this.agent.GetAvatarBody() as RigidBody3D;
				body.BodyEntered += OnEntered;
			}
		}
		
		public void OnEntered(Node body) {
			if (body == target) {
				acmReward += this.reward;
				eval = true;
			}
		}

		public override void OnUpdate() {
			this.agent.AddReward(acmReward, this);
			acmReward = 0.0f;
		}

        public override bool Eval()
        {
			return eval;
        }

        public override void ResetEval()
        {
			eval = false;
        }

        public override void OnReset(Agent agent) {
			acmReward = 0.0f;
		}
	}
}

