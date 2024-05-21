using System.Collections;
using System.Collections.Generic;
using Godot;

namespace ai4u
{
	public partial class GroundStatusSensor : Sensor
	{
		private HistoryStack<float> stack;
		// Start is called before the first frame update
		
		[Export]
		private NodePath rBMoveActuator;
		private RBMoveActuator moveActuator;

		public override void OnSetup(Agent agent)
		{
			moveActuator = GetNode(rBMoveActuator) as RBMoveActuator;
			
			this.type = SensorType.sfloatarray;
			shape = new int[]{stackedObservations};
			this.agent = (BasicAgent) agent;
			stack = new HistoryStack<float>(stackedObservations);
		}

        public override void OnReset(Agent agent)
        {
			stack = new HistoryStack<float>(stackedObservations);
        }

        public override float[] GetFloatArrayValue()
		{
			stack.Push(moveActuator.OnGround? 1.0f : -1.0f);
			return stack.Values;
		}
	}
}

