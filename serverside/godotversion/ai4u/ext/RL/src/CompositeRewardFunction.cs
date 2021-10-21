using Godot;
using System;
using System.Collections.Generic;
using ai4u;

namespace ai4u.ext
{
	public class CompositeRewardFunction : RewardFunc
	{
		
		[Export]
		public Godot.Collections.Array<NodePath> eventsPath = null;
		
		public List<RewardFunc> events;

		public override void OnCreate()
		{
			events = new List<RewardFunc>();
			if (eventsPath != null)
			{
				for(int i = 0; i < eventsPath.Count; i++)
				{
					RewardFunc rf = GetNode(eventsPath[i]) as RewardFunc;
					events.Add(rf);
					rf.Subscribe(this);
				}
			}
			else
			{
				foreach (Node node in GetChildren())
				{
					if ( node.GetType().IsSubclassOf(typeof(RewardFunc)) ) 
					{
						RewardFunc r = node as RewardFunc;
						events.Add(r);
						r.Subscribe(this);
					}
				}
			}
		}
	}
}
