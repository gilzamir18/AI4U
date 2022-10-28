using System.Collections;
using System.Collections.Generic;
using Godot;

namespace ai4u 
{
	public class StepSensor : Sensor
	{
		
		[Export]
		private int maxSize=300;
		[Export]
		private bool realNumber;
		
		public override void OnSetup(Agent agent)
		{
			this.agent = (BasicAgent) agent;
			perceptionKey = "steps";
			if (!realNumber)
			{
				type = SensorType.sint;
			}
			else
			{
				type = SensorType.sfloat; 
			}
			shape = new int[0];
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

