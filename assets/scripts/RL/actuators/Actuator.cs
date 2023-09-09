using System.Collections;
using System.Collections.Generic;
using Godot;

namespace ai4u 
{
	public partial class Actuator : Node, IAgentResetListener
	{

		[Export]
		public string actionName;
		
		[Export]
		public bool isOutput = true;

		private bool actionDone = false;
		protected int[] shape;
		protected bool isContinuous;

		protected float[] rangeMin;
		protected float[] rangeMax;


		public float[] RangeMin
		{
			get 
			{
				return rangeMin;
			}
		}

		public float[] RangeMax 
		{
			get
			{
				return rangeMax;
			}
		}


		public int[] Shape3D
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
