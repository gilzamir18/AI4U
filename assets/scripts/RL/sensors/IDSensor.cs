using System.Collections;
using System.Collections.Generic;
using Godot;

namespace ai4u 
{
	public partial class IDSensor : Sensor
	{
		public override void OnSetup(Agent agent)
		{
			this.agent = (BasicAgent) agent;
			perceptionKey = "id";
			type = SensorType.sstring;
			shape = new int[]{stackedObservations};
		}

		public override string GetStringValue()
		{
			return agent.ID;
		}
	}
}

