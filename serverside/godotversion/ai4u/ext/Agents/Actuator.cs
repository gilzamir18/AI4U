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
		public NodePath ActionRewardPath;
		
		public ActionReward actionReward;

		protected Agent agent;
		
		private bool actionDone = false;

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
		
		public virtual void OnBinding(Agent agent) {
			agent = GetParent() as Agent;
			if (ActionRewardPath != null) {
				actionReward = GetNode(ActionRewardPath) as ActionReward;
			}
		}

		public virtual void OnReset(Agent agent) {

		} 
	}
}
