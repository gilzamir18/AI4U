using System.Collections;
using System.Collections.Generic;
using Godot;

namespace ai4u
{
	public partial class DistanceToTargetRewardFunc: RewardFunc
	{
		[Export]
		private Node3D target;
		
		[Export]
		private float maxReward = 1.0f;
		
		[Export]
		private float minDistance = 1.0f;

        [Export]
        private bool squareInverse = true;

		private bool usedInEvaluation = false;
		
		private BasicAgent agent;

		private float rewardAccumulator = 0.0f;
		
		public override void OnSetup(Agent agent)
		{
			usedInEvaluation = GetParent() is RewardFunc;
			this.agent = (BasicAgent)agent;
			this.agent.AddResetListener(this);
		}

		// Update is called once per frame
		public override void OnUpdate()
		{
			if (!usedInEvaluation)
			{
				Eval();
			}
			agent.AddReward(this.rewardAccumulator);
			if (!usedInEvaluation)
			{
				rewardAccumulator = 0.0f;
			}
		}

        public override bool Eval()
        {
			Vector3 d = (agent.GetAvatarBody() as Node3D).GlobalTransform.Origin - target.GlobalTransform.Origin;
			float dist = d.Length();
            if (squareInverse)
            {
                if (dist <= minDistance)
                {
                    this.rewardAccumulator += maxReward;
					return true;
				}
                else
                {
                    this.rewardAccumulator += maxReward/(dist*dist);
					return false;
				}
            }
            else
            {
                if (dist <= minDistance)
                {
                    this.rewardAccumulator += maxReward;
					return true;
				}
                else
                {
                    this.rewardAccumulator += Mathf.Max(dist, maxReward);
					return false;
				}
            }
        }

        public override void ResetEval()
        {
			this.rewardAccumulator = 0.0f;
        }

        public override void OnReset(Agent agent)
		{ 
			this.agent = (BasicAgent) agent;
		}
	}
}

