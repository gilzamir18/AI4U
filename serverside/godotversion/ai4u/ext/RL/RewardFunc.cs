using System.Collections;
using System.Collections.Generic;
using Godot;
using ai4u;

namespace ai4u.ext {
	public abstract class RewardFunc : Node, IAgentResetListener
	{	
		[Export]
		public bool resettable = true;
		
		protected RLAgent agent;
	
		protected bool finishEpisode = false;
	
		public override void _Ready() 
		{
			agent = GetParent() as RLAgent;	
			if (resettable) 
			{
				agent.AddResetListener(this);	
			}
			OnCreate();
		}
		
		public virtual void OnCreate()
		{
			
		}

		public virtual void OnReset(Agent agent) {
		
		}
	}
}
