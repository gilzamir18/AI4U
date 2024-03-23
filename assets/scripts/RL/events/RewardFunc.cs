using System;
using System.Collections;
using System.Collections.Generic;
using Godot;
using ai4u;

namespace ai4u {
	///<summary>
	///RewardFunc class represents an environmental event.  An environmental event can be:
	///	1) a rewarding event for your agent;
	///	2) an evaluation event generating evaluation for a boolean reward func (AndRewardFunc and ORRewardFunc);
	//	3) evaluation and reward events simultaneously.
	///</summary>
	[System.Serializable]
	public partial class RewardFunc : Node, IAgentResetListener
	{
		[Export]
		public bool causeEpisodeToEnd = false;

		/// <summary>
		/// OnSetup class runs when the owner agent starts the cycle of life.
		/// </summary>
		/// <param name="agent">The reward function owner.</param>
		public virtual void OnSetup(Agent agent)
		{

		}

		/// <summary>
		/// This method runs after the end of one episode step.
		/// Here, it must add step accumulated reward to the agent through the 'agent.AddReward' method.
		/// </summary>
		public virtual void OnUpdate()
		{

		}

		/// <summary>
		/// A boolean reward function uses this method to verify if a child reward event occurred.
		/// </summary>
		public virtual bool Eval()
		{
			return true;
		}

		/// <summary>
		///  Restart the reward evaluation to its initial state.
		/// </summary>
		public virtual void ResetEval()
		{

		}

		/// <summary>
		///  This method is the callback for restarting episode events.
		/// </summary>
		public virtual void OnReset(Agent agent) 
		{

		}
	}
}
