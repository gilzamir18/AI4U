using System.Collections;
using System.Collections.Generic;
using Godot;

namespace ai4u.ext 
{
	
	public interface IActionListener
	{
		void OnAction(Actuator actuator);
	}
	
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
		
		private List<IActionListener> listeners = new List<IActionListener>();
		
		private bool isOperation = false;
		
		public ActionReward actionReward;

		protected Agent agent;
		
		private bool actionDone = false;
		
		private float actionValue;

		public float ActionValue
		{
			get
			{
				return actionValue;
			}
			
			set 
			{
				actionValue = value;
			}
		}


		public void NotifyListeners()
		{
			foreach(IActionListener a in listeners)
			{
				a.OnAction(this);
			}
		}

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
		
		public void Subscribe(IActionListener listener)
		{
			this.listeners.Add(listener);
		}
		
		public void Unsubscribe(IActionListener listener)
		{
			this.listeners.Remove(listener);
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
		
		public virtual void OnDone()
		{
			
		}

		public virtual void OnReset(Agent agent) {

		} 
	}
}
