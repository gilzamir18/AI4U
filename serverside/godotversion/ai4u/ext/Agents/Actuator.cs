using System.Collections;
using System.Collections.Generic;
using Godot;

namespace ai4u.ext 
{
	public class Actuator : Node, IAgentResetListener
	{
		[Export]
		public bool resettable = true;
		
		[Export]
		public string actionName = "";
		
		[Export]
		public bool always = false;
		
		[Export]
		public NodePath actionRewardPath;
		
		private bool isOperation = false;
		
		public ActionReward actionReward;

		protected Agent agent;
		
		private bool actionDone = false;

		public bool IsOperation
		{
			get
			{
				return this.isOperation;
			}
			
			set
			{
				this.isOperation = value;
			}
		}

		public virtual void NotifyEndOfEpisode() 
		{

		}

		public bool ActionDone{
			get {
				return actionDone;
			}

			set {
				actionDone = value;
			}
		}
		
		public virtual void Act()
		{

		}
		
		public virtual void OnBinding(Agent agent) 
		{
			this.agent = agent;
			
			if (actionRewardPath != null && !actionRewardPath.Equals("")) {
				actionReward = GetNode(actionRewardPath) as ActionReward;
			}
		}

		public virtual void OnReset(Agent agent) {

		} 
	}
}
