using Godot;
using System;
using System.Collections.Generic;
using System.Collections;
using ai4u;

namespace ai4u.ext
{
	public class XorRewardFunc : RewardFunc
	{
		
		private const int NB_OF_ARGS = 2;
		
		
		[Export]
		public float alfa = 0.5f;
		
		[Export]
		public float successReward = 1.0f;
		
		[Export]
		public bool repeatable = false;
		
		private RewardFunc[] arguments;
		
		private bool arg1;
		private bool arg2;
		
		private bool applied = false;

		[Export]
		public NodePath argOneRewardFuncPath;

		[Export]
		public NodePath argTwoRewardFuncPath;
		
		public override void OnCreate()
		{
			arguments = new RewardFunc[NB_OF_ARGS];
			if (argOneRewardFuncPath != null && argTwoRewardFuncPath != null)
			{
				arguments[0] = GetNode(argOneRewardFuncPath) as RewardFunc;
				arguments[1] = GetNode(argTwoRewardFuncPath) as RewardFunc;
				arguments[0].Subscribe(this);
				arguments[1].Subscribe(this);
			} else
			{
				var count = 0;
				foreach (Node node in GetChildren())
				{
					if ( node.GetType().IsSubclassOf(typeof(RewardFunc)) ) 
					{
						RewardFunc r = node as RewardFunc;
						arguments[count++] = r;
						r.Subscribe(this);
						if (count >= NB_OF_ARGS)
						{
							break;
						}
					}
				}
			}
		}

		private float  r = 0.0f;
		public override void OnNotificationFrom(RewardFunc notifier, float reward)
		{
			if (applied && !repeatable)
			{
				return;
			}
			
			if (notifier.Name == arguments[0].Name) 
			{
				r += reward;
				arg1 = true;
			}
			
			if (notifier.Name == arguments[1].Name)
			{
				r += reward;
				arg2 = true;	
			}
			
			if (arg1 ^ arg2) 
			{
				float mixed  = (1-alfa)*successReward + alfa * r;
				agent.AddReward(mixed, this, endEpisodeInSuccess);
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
