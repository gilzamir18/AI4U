using System.Collections;
using System.Collections.Generic;
using Godot;

namespace ai4u
{
	public class RewardSensor : Sensor
	{
		public float rewardScale = 1.0f;
		public override void OnSetup(Agent agent)
		{
			this.agent = (BasicAgent) agent;
			perceptionKey = "reward";
			type = SensorType.sfloat;
			shape = new int[0];
			this.agent = (BasicAgent) agent;
		}

		public override float GetFloatValue()
		{
			return agent.Reward * rewardScale;
		}
	}
}
