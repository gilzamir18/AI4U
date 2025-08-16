using System.Collections;
using System.Collections.Generic;
using Godot;
using ai4u;

namespace ai4u
{
	public partial class AgentArraySensor : Sensor, IAgentResetListener
	{
        private int size = 0;

        private float[] data;

		public override void OnSetup(Agent agent)
		{
			this.agent = (RLAgent) agent;
            this.agent.AddResetListener(this);
            this.size = stackedObservations * this.agent.arrayInputSize;
            this.data = new float[this.size];
			perceptionKey = "__initial_input__";
			type = SensorType.sfloatarray;
			shape = new int[]{this.size};
        }

        public void SetValue(int idx, float v)
        {
            data[idx] = v;
        }

        public void SetValues(float[] values)
        {
            this.data = values;
        }

        public int Count()
        {
            return shape[0];
        }

        public override void OnReset(Agent agent)
        {
            this.data = new float[this.size];
        }

		public override float[] GetFloatArrayValue()
		{
			return data;
		}
	}
}
