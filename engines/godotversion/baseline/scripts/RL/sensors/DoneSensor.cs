using System.Collections;
using System.Collections.Generic;
using Godot;
using ai4u;

namespace ai4u
{
	public class DoneSensor : Sensor
	{
		public override void OnSetup(Agent agent)
		{
			this.agent = (BasicAgent) agent;
			perceptionKey = "done";
			type = SensorType.sbool;
			shape = new int[0];
		}

		public override bool GetBoolValue()
		{
			return agent.Done;    
		}
	}
}
