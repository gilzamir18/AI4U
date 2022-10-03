using System.Collections;
using System.Collections.Generic;
using Godot;

namespace ai4u 
{
	public class StepSensor : Sensor
	{
		public override void OnSetup(Agent agent)
		{
			this.agent = (BasicAgent) agent;
			perceptionKey = "steps";
			type = SensorType.sint;
			shape = new int[0];
		}

		public override int GetIntValue()
		{
			return agent.NSteps;    
		}
	}
}

