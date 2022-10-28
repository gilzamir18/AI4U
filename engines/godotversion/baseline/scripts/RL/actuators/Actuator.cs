using System.Collections;
using System.Collections.Generic;
using Godot;

namespace ai4u 
{
	public class Actuator : Node, IAgentResetListener
	{

		[Export]
		public string actionName;
		
		private bool actionDone = false;
		protected int[] shape;
		protected bool isContinuous;

		public int[] Shape
		{
			get
			{
				return shape;
			}
		}

		public bool IsContinuous
		{
			get
			{
				return isContinuous;
			}
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

		public virtual void OnSetup(Agent agent)
		{
		}

		public virtual void OnReset(Agent agent) {
		} 
	}
}
