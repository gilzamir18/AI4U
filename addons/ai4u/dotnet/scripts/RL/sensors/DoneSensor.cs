using System.Collections;
using System.Collections.Generic;
using Godot;
using ai4u;

namespace ai4u
{
	public partial class DoneSensor : Sensor
	{
		public override void OnSetup(Agent agent)
		{
			this.agent = (RLAgent) agent;
			perceptionKey = "done";
			type = SensorType.sbool;
			shape = new int[]{stackedObservations};
		}

		public override bool GetBoolValue()
		{
			return agent.Done;    
		}
	}
}
