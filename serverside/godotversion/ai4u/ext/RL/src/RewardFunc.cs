using System.Collections;
using System.Collections.Generic;
using Godot;
using ai4u;

namespace ai4u.ext {
	public abstract class RewardFunc : Node, IAgentResetListener
	{	
		[Export]
		public bool resettable = true;

	
		[Export]
		public bool endEpisodeInFail = false;
		
		[Export]
		public bool endEpisodeInSuccess = false;
		
		[Export]
		public bool propagateNegativeReward = false;
		[Export]
		public bool propagatePositiveReward = true;
		[Export]
		public bool propagateZeroReward = true;
		[Export]
		public bool usePathAsID = true;
		[Export]
		public bool isJustAnEvent = false;
		
		protected RLAgent agent;
	
		protected bool finishEpisode = false;
	
		protected List<RewardFunc> notificationList  = new List<RewardFunc>();
		
		private string id;
		
		
		public override void _Ready() 
		{	
			Node n = this;
			Node rootNode = GetNode("/root");
			while (agent == null &&  n != rootNode)
			{
				Node parent = n.GetParent();
				n = parent;
				agent =  parent as RLAgent;
			}

			if (resettable) 
			{
				agent.AddResetListener(this);	
			}
			if (usePathAsID)
			{
				this.id = GetPath();
			} else 
			{
				this.id = Name;
			}
			OnCreate();
		}
		
		public string ID
		{
			get 
			{
				return id;	
			}
		}
		
		public virtual void OnCreate()
		{
				
		}
		
		public void Subscribe(RewardFunc rf)
		{
			notificationList.Add(rf);
		}
		
		public void Unsubscribe(RewardFunc rf)
		{
			notificationList.Remove(rf);
		}
		
		protected void NotifyAll(float reward)
		{
			if ( (reward > 0  && propagatePositiveReward) ||
					(reward < 0  && propagateNegativeReward) ||
					(reward == 0  && propagateZeroReward) )
			{ 
				foreach(RewardFunc f in notificationList)
				{
					f.OnNotificationFrom(this, reward);
				}
			}
		}
		
		public virtual void OnNotificationFrom(RewardFunc notifier, float reward)
		{
			
		}

		public virtual void OnReset(Agent agent) {

		}
	}
}
