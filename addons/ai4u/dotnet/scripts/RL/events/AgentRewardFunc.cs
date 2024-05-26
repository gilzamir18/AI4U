using System;
using System.Collections;
using System.Collections.Generic;
using Godot;
using ai4u;

namespace ai4u {
	///<summary> 
    ///AgentRewardFunc is used internally by the agent for manually adding rewards.
	///</summary>
	[System.Serializable]
	public partial class AgentRewardFunc : RewardFunc
	{

        private float rewards = 0.0f;
        private bool requestDone = false;

        private BasicAgent agent;

		/// <summary>
		/// OnSetup class runs when the owner agent starts the cycle of life.
		/// </summary>
		/// <param name="agent">The reward function owner.</param>
		public override void OnSetup(Agent agent)
		{
            this.requestDone = false;
            rewards = 0;
            this.agent = (BasicAgent) agent;
		}


        public void Add(float v,  bool requestDone = false)
        {
            rewards += v;
            this.requestDone = requestDone;
        }

		/// <summary>
		/// This method runs after the end of one episode step.
		/// Here, it must add step accumulated reward to the agent through the 'agent.AddReward' method.
		/// </summary>
		public override void OnUpdate()
		{
            this.agent.AddReward(rewards, requestDone);
            rewards = 0;
            requestDone = false;
		}

		/// <summary>
		///  This method is the callback for restarting episode events.
		/// </summary>
		public override void OnReset(Agent agent) 
		{
            rewards = 0;
            requestDone = false;
		}
	}
}
