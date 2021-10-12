using Godot;
using System;
using ai4u;

namespace ai4u.ext
{
	public class NoOpActuator : Actuator
	{
		public override void Act()
		{
		}
		
		public override void OnBinding(Agent agent) 
		{
			this.agent = GetParent() as Agent;
		}

		public override void OnReset(Agent agent)
		{
		}
	}
}
