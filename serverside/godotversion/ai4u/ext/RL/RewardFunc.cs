using System.Collections;
using System.Collections.Generic;
using Godot;
using ai4u;

namespace ai4u.ext {
	public class RewardFunc : Node, IAgentResetListener
	{
		public RLAgent[] agents;

		public bool causeEpisodeToEnd = false;
	
		public virtual void OnReset(Agent agent) {

		} 
	}
}
