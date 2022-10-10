using System.Collections;
using System.Collections.Generic;
using Godot;

namespace ai4u
{
	public class PositionSensor : Sensor
	{

		
		[Export]
		private NodePath target;
		private Spatial targetNode;
		[Export]
		private bool getLocal = true;
		
		
		private HistoryStack<float> stack;
	
		public float xMin = 0;
		public float xMax = 500;

		public float yMin = 0;
		public float yMax = 500;

		public float zMin = 0;
		public float zMax = 500;

		private float Preprocessing(float v, float min, float max)
		{
			return (v - min)/(max-min);
		}

		public override void OnSetup(Agent agent)
		{
			targetNode = GetNode(target) as Spatial;
			this.type = SensorType.sfloatarray;
			this.shape = new int[1]{3};
			this.agent = (BasicAgent) agent;
			stack = new HistoryStack<float>(3 * stackedObservations);
		}

		public override void OnReset(Agent agent)
		{
			stack = new HistoryStack<float>(3 * stackedObservations);
			GetFloatArrayValue();
		}

		public override float[] GetFloatArrayValue()
		{
			if (getLocal)
			{
				stack.Push(Preprocessing(targetNode.Transform.origin.x, xMin, xMax));
				stack.Push(Preprocessing(targetNode.Transform.origin.y, yMin, yMax));
				stack.Push(Preprocessing(targetNode.Transform.origin.z, zMin, zMax));
			}
			else
			{
				stack.Push(Preprocessing(targetNode.GlobalTransform.origin.x, xMin, xMax));
				stack.Push(Preprocessing(targetNode.GlobalTransform.origin.y, yMin, yMax));
				stack.Push(Preprocessing(targetNode.GlobalTransform.origin.z, zMin, zMax));				
			}
			return stack.Values;
		}
	}
}
