using Godot;
using System;
using System.Collections.Generic;
using System.Collections;
using ai4u;

namespace ai4u.ext
{
	public class AtLeastOneReward : CompositeRewardFunction
	{
		[Export]
		public float successReward = 1.0f;
		
		[Export]
		public bool resetAfterSuccess = true;
		
		private Dictionary<string, bool> test;
		
		private int size = 0;
		
		public override void OnCreate()
		{
			test = new Dictionary<string, bool>();
			base.OnCreate();
			size = 0;
		}

		public override void OnNotificationFrom(RewardFunc notifier, float reward)
		{
			if (!test.ContainsKey(notifier.ID))
			{	
				size++;
				test[notifier.ID] = true;
			}
			if (size > 0) 
			{
				NotifyAll(successReward);
				agent.AddReward(successReward, this, endEpisodeInSuccess);
				if (resetAfterSuccess)
				{
					test = new Dictionary<string, bool>();
					size = 0;
				}
			}
		}
		
		public override void OnReset(Agent agent)
		{
			test = new Dictionary<string, bool>();
			this.size = 0;
		}
	}
}
