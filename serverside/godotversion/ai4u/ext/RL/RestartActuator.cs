using Godot;
using System;
using ai4u;

namespace ai4u.ext
{
	public class RestartActuator : Actuator
	{
		public override void Act()
		{
			this.agent.NotifyReset();
		}
	}
}
