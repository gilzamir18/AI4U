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

		[Export]
		private float _collisionCheckInterval = 0.2f;
		
		[Export]
		private bool onlyOneTime = true;

		private float acmReward = 0.0f;
		private BasicAgent agent;
		private bool configured = false;
		private bool eval = false;
		private RigidBody2D rigidBody;

		private CharacterBody2D characterBody;
		private float _collisionIntervalCoolDown = 0;
		private bool isCharacterBody = false;

		private bool entered = false;
		public override void OnSetup(Agent agent)
		{
			if (!configured)
			{
				configured = true;
				agent.AddResetListener(this);
				this.agent = (BasicAgent)agent;
				this.agent.OnStepEnd += PhysicsUpdate;
				var body = this.agent.GetAvatarBody();

				if (body.GetType() == typeof(CharacterBody2D))
				{
					characterBody = body as CharacterBody2D;
					isCharacterBody = true;
				}
				else
				{
					isCharacterBody = false;
					rigidBody = body  as RigidBody2D;
					rigidBody.BodyEntered += OnEntered;
					if (target is Area2D)
					{
						((Area2D)target).BodyEntered += OnAreaEntered;
					}
				}
			}
		}


		public void OnEntered(Node body)
		{
			CheckCollision(body);
		}

		public void OnAreaEntered(Node body)
		{
			if (body == rigidBody)
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

		public void PhysicsUpdate(BasicAgent agent)
        {
			if (agent.SetupIsDone && agent.Alive() && isCharacterBody && (!onlyOneTime || !entered))
            {
				this._collisionIntervalCoolDown -= (float)GetPhysicsProcessDeltaTime();
				if (this._collisionIntervalCoolDown > 0)
				{
					return;
				}

                var nc = characterBody.GetSlideCollisionCount();
                for (int i = 0; i < nc; i++)
                {
                    var kc = characterBody.GetSlideCollision(i);
    
                    var n = (Node)kc.GetCollider();
                    if (n == target)
                    {
						entered = true;
						acmReward += this.reward;
						eval = true;
						this._collisionIntervalCoolDown = this._collisionCheckInterval; 
						break;
                    }
                }
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
			entered = false;
			_collisionIntervalCoolDown = 0;
		}
	}
}

