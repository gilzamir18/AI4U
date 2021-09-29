using Godot;
using System;
using ai4u;

namespace ai4u.ext
{
	public class FallReward : RewardFunc
	{	
		[Export]
		public float threshold = 0.0f;
		[Export]
		public bool is2D = false;
		[Export]
		public float successReward = -1.0f;
		[Export]
		public float failReward = 0.0f;
		[Export]
		public bool isLocal;
		[Export]
		public bool onlyOnce = true;
		
		[Export]
		public bool endEpisodeInFail = false;
		
		[Export]
		public bool endEpisodeInSuccess = true;
		
		private bool applied = false;
		
		[Export(PropertyHint.Range, "0,2")]
		public int axis = 1;
		
		public override void OnCreate()
		{	
		}
		
		public override void OnReset(Agent agent)
		{
			applied = false;
		}
		
		public override void _Process(float delta)
		{
			if ( !agent.Done && (!onlyOnce || !applied) )
			{
				if (is2D) 
				{
					Node2D nd = (agent.GetBody() as Node2D);
					Vector2 pos;
					if (isLocal)
					{
						pos = nd.Position;
					} else 
					{
						pos = nd.GlobalPosition;	
					}
					if (pos[axis] < threshold)
					{
						agent.AddReward(successReward, this, endEpisodeInSuccess);
						if (onlyOnce){
							applied = true;
						}
					}
					else 
					{
						agent.AddReward(failReward, this, endEpisodeInFail);
					}
				} else 
				{
					Spatial sp = agent.GetBody() as Spatial;
					Vector3 pos;
					if (isLocal)
					{
						pos = sp.Translation;
					} 
					else
					{
						pos = sp.Transform.origin;
					}
					
					if (pos[axis] < threshold)
					{
						agent.AddReward(successReward, this, endEpisodeInSuccess);
						if (onlyOnce){
							applied = true;
						}
					}
					else 
					{
						agent.AddReward(failReward, this, endEpisodeInFail);
					}
				}						
			}
		}
	}
}
