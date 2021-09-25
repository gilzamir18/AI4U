using Godot;
using System;
using ai4u;

namespace ai4u.ext
{
	public class FloatSensor: Sensor {
		private float data = 0.0f;
		
		public float Data
		{
			get
			{
				return data;
			}
			
			set
			{
				data = value;
			}
		}
		
		public override float GetFloatValue()
		{
			return data;
		}
		
		public override void OnReset(Agent agent)
		{
			data = 0.0f;
		}
	}
}

