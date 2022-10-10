using System;
using System.Collections;
using System.Collections.Generic;
using Godot;
using ai4u;

namespace ai4u {
	[System.Serializable]
	public class RewardFunc : Node, IAgentResetListener
	{
		[Export]
		public bool causeEpisodeToEnd = false;
		
		public virtual void OnSetup(Agent agent)
		{

		}

		public virtual void OnUpdate()
		{

		}

		public virtual void OnReset(Agent agent) {

		}
	}
}
