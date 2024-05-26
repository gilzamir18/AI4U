using System.Collections;
using System.Collections.Generic;
using Godot;
using ai4u;

namespace ai4u
{
	public partial class AgentArraySensor : Sensor, IAgentResetListener
	{
        private float[] data;

		public override void OnSetup(Agent agent)
		{
            stackedObservations = 1;
			this.agent = (BasicAgent) agent;
            this.agent.AddResetListener(this);
            data = new float[this.agent.initialInputSize];
			perceptionKey = "__initial_input__";
			type = SensorType.sfloatarray;
			shape = new int[]{stackedObservations*data.Length};
		}

        public void SetArray(int pos, float v)
        {
            data[pos] = v;
        }

        public int Count()
        {
            return this.agent.initialInputSize;
        }

        public override void OnReset(Agent agent)
        {
            data = new float[this.agent.initialInputSize];
        }

		public override float[] GetFloatArrayValue()
		{
			return this.data;   
		}
	}
}
