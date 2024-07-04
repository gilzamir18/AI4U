using Godot;
using System;
using ai4u;
namespace ai4u {
	public partial class TouchRewardFunc : RewardFunc {

		[Signal] public delegate void OnTouchEventHandler(TouchRewardFunc source);
			
		[Export]
		public float reward = 0.0f;
		[Export]
		private Node target;


		[ExportCategory("CharacterBody3D Options")]
		[Export]
		private float _collisionCheckInterval = 5;
		[Export]
		private bool onlyOneTime = true;

		private float _collisionIntervalCoolDown = 0;

		private float acmReward = 0.0f;
		private RLAgent agent;
		private bool configured = false;
		private bool eval = false;

		private bool entered = false;


		private bool isCharacterBody = false;

		private CharacterBody3D chBody;
		
		public override void OnSetup(Agent agent) {
			if (!configured)
			{
				eval = false;
				configured = true;
				agent.AddResetListener(this);
				this.agent = (RLAgent) agent;
				this.agent.OnStepEnd += PhysicsUpdate;
				var body = this.agent.GetAvatarBody();
				if (body.GetType() == typeof(RigidBody3D))
                {
					((RigidBody3D)body).BodyEntered += OnEntered;
				} else if (body.GetType() == typeof(CharacterBody3D))
                {
                    isCharacterBody = true;
                    chBody = (CharacterBody3D) body;
                }
                else
                {
                    throw new Exception("The type of the agent avatar is invalid!");
                }
			}
		}
		
		public void OnEntered(Node body) {
			if (body == target) {
				acmReward += this.reward;
				eval = true;
				EmitSignal(SignalName.OnTouch, this);
			}
		}

        public void PhysicsUpdate(RLAgent agent)
        {


			if (agent.SetupIsDone && agent.Alive() && isCharacterBody && (!onlyOneTime || !entered) && !agent.Done)
            {
				this._collisionIntervalCoolDown -= (float)this.agent.GetPhysicsProcessDeltaTime();
				if (this._collisionIntervalCoolDown > 0)
				{
					return;
				}

                var nc = chBody.GetSlideCollisionCount();
                for (int i = 0; i < nc; i++)
                {
                    var kc = chBody.GetSlideCollision(i);
    
                    var n = (Node)kc.GetCollider();
                    if (n == target)
                    {
						entered = true;
                        acmReward += this.reward;
						eval = true;
						this._collisionIntervalCoolDown = this._collisionCheckInterval;
                        EmitSignal(SignalName.OnTouch, this);
                        break;
                    }
                }
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
			this._collisionIntervalCoolDown = 0;
			this.entered = false;
			eval = false;
		}
	}
}

