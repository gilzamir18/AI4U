using System.Collections;
using System.Collections.Generic;
using Godot;

namespace ai4u {
	public partial class ActionSensor : Sensor
	{
		[Export]
		public string actionName;
		
		[Export]
		public int actionSize;    
		
		private HistoryStack<float> history;
		private float[] previewsAction;

		public override void OnSetup(Agent agent) {
			this.agent = (RLAgent)agent;

			type = SensorType.sfloatarray;
			shape = new int[1]{actionSize*stackedObservations};
			previewsAction = new float[shape[0]];
			history = new HistoryStack<float>(shape[0]);
		}

		public override void OnReset(Agent aget)
		{
			history = new HistoryStack<float>(shape[0]);
			previewsAction = new float[shape[0]];
		}
		
		public override float[] GetFloatArrayValue()
		{
			if (agent.GetActionName()==actionName)
			{
				for (int i = 0; i < actionSize; i++)
				{
					history.Push(previewsAction[i]);
				}
				previewsAction = agent.GetActionArgAsFloatArray();
			}
			return history.Values;
		}
	}
}
