using Godot;
using System;
using ai4u;
namespace ai4u
{
	public partial class TouchRewardFunc2D : RewardFunc
	{
		[Export]
		public float reward = 0.0f;
		[Export]
		private Node2D target;
		private float acmReward = 0.0f;
		private BasicAgent agent;
		private bool configured = false;
		private bool eval = false;
		private RigidBody2D agentBody;

		public override void OnSetup(Agent agent)
		{
			if (!configured)
			{
				configured = true;
				agent.AddResetListener(this);
				this.agent = (BasicAgent)agent;
				agentBody = this.agent.GetAvatarBody() as RigidBody2D;
				agentBody.BodyEntered += OnEntered;
				if (target is Area2D)
				{
					((Area2D)target).BodyEntered += OnAreaEntered;
				}
			}
		}


		public void OnEntered(Node body)
		{
			CheckCollision(body);
		}

		public void OnAreaEntered(Node body)
		{
			if (body == agentBody)
			{
				if (target.Visible)
				{
					CheckCollision(target);
				}
			}
		}

		private void CheckCollision(Node body)
		{
            if (body == target)
            {
                eval = true;
                acmReward += this.reward;
            }
        }

		public override bool Eval()
		{
			return eval;
		}

		public override void OnUpdate()
		{
			this.agent.AddReward(acmReward, this);
			acmReward = 0.0f;
		}

		public override void ResetEval()
		{
			eval = false;
		}

		public override void OnReset(Agent agent)
		{
			eval = false;
			acmReward = 0.0f;
		}
	}
}

