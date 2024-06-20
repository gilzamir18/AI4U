using Godot;
using System;
using ai4u;

namespace ai4u
{
	public partial class DynamicFallReward : RewardFunc
	{	
		
		public Node3D dynamicReference {get; set;}

		[Export]
		private float threshold = 10;
		
		[Export]
		private float successReward = -1.0f;
		
		[Export]
		private float failReward = 0.0f;
		
		[Export]
		private bool isLocal = false;
		
		[Export]
		private int axis = 1;
		
		[Export]
		private int maxRewards = 1;
		
		private float acmReward = 0.0f;
		private int rewards = 0;
		private BasicAgent agent;
		
		public override void OnSetup(Agent agent)
		{	
			this.agent = (BasicAgent) agent;
			agent.AddResetListener(this);
		}
		
		public override void OnReset(Agent agent)
		{
			acmReward = 0.0f;
			rewards = 0;
		}
		
		public override void OnUpdate()
		{
			
			if (rewards < maxRewards && dynamicReference != null)
			{
				Node3D sp = agent.GetAvatarBody() as Node3D;
				Vector3 pos;

                Vector3 dynPos;

				if (isLocal)
				{
                    dynPos = dynamicReference.Position;
					pos = sp.Position;
				} 
				else
				{
					pos = sp.Transform.Origin;
                    dynPos = dynamicReference.Transform.Origin;
				}
				
				if (pos[axis] < (dynPos[axis]-threshold))
				{
					acmReward += successReward;
				}
				else 
				{
					acmReward += failReward;
				}					
				agent.AddReward(acmReward, this);
				if (acmReward != 0)
				{
					rewards ++;
				}
			}
			acmReward = 0;
		}

	}
}
