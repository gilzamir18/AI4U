using Godot;
using System;
using System.Collections.Generic;
using System.Collections;
using ai4u;

namespace ai4u.ext
{
	public class AndNotRewardFunc : RewardFunc
	{
		private const int NB_OF_ARGS = 2;
		
		[Export]
		public float alfa = 0.5f;
		
		[Export]
		public float successReward = 1.0f;
		
		[Export]
		public float failReward = -1.0f;
		
		[Export]
		public bool repeatable = false;
		
		[Export]
		public NodePath positiveRewardFuncPath;

		[Export]
		public NodePath negativeRewardFuncPath;
		
		private RewardFunc positiveRewardFunc = null;
		private RewardFunc negativeRewardFunc = null;
		
		
		private bool arg1;
		private bool arg2;
		
		private float r  = 0.0f;
		
		
		private bool applied = false;
		
		public override void OnCreate()
		{
			r = 0.0f;
			arg1 = arg2 = false;
			RewardFunc[] funcs = new RewardFunc[NB_OF_ARGS];
			
			if (negativeRewardFuncPath != null && positiveRewardFuncPath != null)
			{
				funcs[0] = GetNode(positiveRewardFuncPath) as RewardFunc;
				funcs[1] = GetNode(negativeRewardFuncPath) as RewardFunc;
				funcs[0].Subscribe(this);
				funcs[1].Subscribe(this);
			} else 
			{			
				var count = 0;
				foreach (Node node in GetChildren())
				{
					if ( node.GetType().IsSubclassOf(typeof(RewardFunc)) ) 
					{
						RewardFunc r = node as RewardFunc;
						funcs[count++] = r;
						r.Subscribe(this);
						if (count >= NB_OF_ARGS)
						{
							break;
						}
					}
				}
			}
			positiveRewardFunc  = funcs[0];
			negativeRewardFunc = funcs[1];
		}

		public override void OnNotificationFrom(RewardFunc notifier, float reward)
		{
			if (applied && !repeatable)
			{
				return;
			}
			
			if (notifier.Name == positiveRewardFunc.Name) 
			{
				r += reward;
				arg1 = true;
			}
			
			if (notifier.Name == negativeRewardFunc.Name)
			{
				r += reward;
				arg2 = true;
			}
			
			if (arg1 && !arg2) 
			{
				float mixed = (1-alfa) * successReward + alfa * r;
				NotifyAll(mixed);
				agent.AddReward(mixed, this, endEpisodeInSuccess);
				if (repeatable) 
				{
					arg1 = arg2 = false;
				} else 
				{
					applied = true;
				}
				r = 0.0f;
			} else if (arg2)
			{
				float mixed = (1-alfa) * failReward + alfa * r;
				NotifyAll(mixed);
				agent.AddReward(mixed, this, endEpisodeInFail);
				if (repeatable) 
				{
					arg1 = arg2 = false;
				} else 
				{
					applied = true;
				}
				r = 0.0f;
			}
		}

		public override void OnReset(Agent agent)
		{
			r = 0.0f;
			arg1 = arg2 = false;
			applied = false;
		}
	}
}
