using Godot;
using System;
using System.Collections.Generic;
using ai4u;

namespace ai4u.ext.npcs
{
	
	
	public class TimeStepSensor : Sensor
	{
		public override int GetIntValue()
		{
			return (agent as RLAgent).CurrentStep;
		} 

		public override void OnBinding(Agent agent)
		{
			this.agent = agent;
			type = SensorType.sint;
		}
	}
}
