using Godot;
using System;
using ai4u;

namespace ai4u.ext
{
	public class BoolSensor: Sensor {
		private bool data = false;
	
		public bool Data
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
		
		public override bool GetBoolValue()
		{
			return data;
		}
		
		public override void OnReset(Agent agent)
		{
			data = false;
		}
	}
}
