using System.Collections;
using System.Collections.Generic;
using Godot;

namespace ai4u 
{
	public class IDSensor : Sensor
	{
		public override void OnSetup(Agent agent)
		{
			this.agent = (BasicAgent) agent;
			perceptionKey = "id";
			type = SensorType.sstring;
			shape = new int[0];
		}

		public override string GetStringValue()
		{
			return agent.ID;
		}
	}
}

