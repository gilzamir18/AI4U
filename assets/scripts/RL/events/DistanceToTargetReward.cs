using System.Collections;
using System.Collections.Generic;
using Godot;

namespace ai4u
{
	public partial class DistanceToTargetReward : RewardFunc
	{
		[Export]
		private Node3D target;
		
		[Export]
		private float maxReward = 1.0f;
		
		[Export]
		private float minDistance = 1.0f;

        [Export]
        private bool squareInverse = true;
		
		private BasicAgent agent;
		
		public override void OnSetup(Agent agent)
		{
			this.agent = (BasicAgent)agent;
			
			this.agent.AddResetListener(this);
		}

		// Update is called once per frame
		public override void OnUpdate()
		{
			Vector3 d = (agent.GetAvatarBody() as Node3D).GlobalTransform.Origin - target.GlobalTransform.Origin;
			float dist = d.Length();
            if (squareInverse)
            {
                if (dist <= minDistance)
                {
                    agent.AddReward(maxReward, this);
                }
                else
                {
                    agent.AddReward(maxReward/(dist*dist));
                }
            }
            else
            {
                if (dist <= minDistance)
                {
                    agent.AddReward(0, this);
                }
                else
                {
                    agent.AddReward( Mathf.Max(dist, maxReward) );
                }
            }
		}

		public override void OnReset(Agent agent)
		{ 
			this.agent = (BasicAgent) agent;
		}
	}
}

