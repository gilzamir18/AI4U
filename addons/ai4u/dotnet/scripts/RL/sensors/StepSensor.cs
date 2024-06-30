using System.Collections;
using System.Collections.Generic;
using Godot;

namespace ai4u 
{
	public partial class StepSensor : Sensor
	{
		
		[Export]
		private int maxSize=300;
		[Export]
		private bool realNumber;
		
		public override void OnSetup(Agent agent)
		{
			this.agent = (RLAgent) agent;
			perceptionKey = "steps";
			if (!realNumber)
			{
				type = SensorType.sint;
			}
			else
			{
				type = SensorType.sfloat; 
			}
			shape = new int[]{stackedObservations};
		}

		public override int GetIntValue()
		{
			return agent.NSteps;    
		}
		
		public override float GetFloatValue()
		{
			if (normalized)
			{
				return agent.NSteps/maxSize;
			}
			else
			{
				return agent.NSteps;
			}
		}
	}
}

