using Godot;
using System;
using ai4u;

namespace ai4u.ext
{
	public class SequencialReward : CompositeRewardFunction
	{
		[Export]
		public float successReward = 1.0f;
		[Export]
		public float failReward = -1.0f;
		[Export]
		public bool resetAfterSuccess = false;
		[Export]
		public bool resetAfterFail = false;
		
		private int currentPos = 0;
		
		
		public override void OnCreate()
		{
			base.OnCreate();
			currentPos = 0;
		}

		public override void OnNotificationFrom(RewardFunc notifier, float reward)
		{
			var idx = events.IndexOf(notifier);
			if (idx != currentPos) 
			{
				NotifyAll(failReward);
				agent.AddReward(failReward, this, endEpisodeInFail);
				if (resetAfterFail)
					currentPos = 0;
			} else
			{
				currentPos ++;
				if (currentPos >= events.Count){
					NotifyAll(successReward);
					agent.AddReward(successReward, this, endEpisodeInSuccess);	
					if (resetAfterSuccess)
						currentPos = 0;
				}
			}
		}
		
		public override void OnReset(Agent agent)
		{
			this.currentPos = 0;
		}
	}
}
