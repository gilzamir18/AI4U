using Godot;
using System;
using System.Collections.Generic;
using ai4u;

namespace ai4u.ext
{
	public class RewardEventsSensor : Sensor
	{
		public override string GetStringValue()
		{
			List<RewardInfo> rewards = (agent as RLAgent).RewardOcurrence;
			return Utils.ListToPythonList<RewardInfo>( rewards );
		}
	
		public override void OnBinding(Agent agent) 
		{
			this.type = SensorType.sstring;
			this.agent = agent;
		}
	}
}
