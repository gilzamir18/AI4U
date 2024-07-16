using System.Collections;
using System.Collections.Generic;
using Godot;

namespace ai4u
{
	public partial class RewardSensor : Sensor
	{
		[Export]
		public float rewardScale = 1.0f;
		public override void OnSetup(Agent agent)
		{
			this.agent = (RLAgent) agent;
			perceptionKey = "reward";
			type = SensorType.sfloat;
			shape = new int[]{stackedObservations};
			this.agent = (RLAgent) agent;
		}

		public override float GetFloatValue()
		{
			return agent.Reward * rewardScale;
		}
	}
}
