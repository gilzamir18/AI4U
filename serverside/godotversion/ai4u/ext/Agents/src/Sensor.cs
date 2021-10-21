using System.Collections;
using System.Collections.Generic;
using Godot;

namespace ai4u.ext
{	
	public enum SensorType {
		sfloat,
		sstring,
		sbool,
		sint,
		sbytearray,
		sfloatarray,
		sintarray
	}

	public class Sensor : Node, IAgentResetListener
	{
		[Export]
		public bool resettable = true;
		
		[Export]
		public string perceptionKey = "";
		
		[Export]
		public SensorType type;
		
		[Export]
		public bool isState;
		
		[Export]
		public int[] shape;
		
		private bool enabled = true;
		
		protected Agent agent;

		public void SetAgent(Agent own)
		{
			agent = own;
		}

		public virtual float GetFloatValue() {
			return 0;
		}

		public virtual string GetStringValue() {
			return string.Empty;
		}

		public virtual bool GetBoolValue() {
			return false;
		}

		public virtual byte[] GetByteArrayValue() {
			return null;
		}

		public virtual int GetIntValue() {
			return 0;
		}

		public virtual int[] GetIntArrayValue() {
			return null;
		}

		public virtual float[] GetFloatArrayValue() {
			return null;
		}

		public virtual void OnBinding(Agent agent) {
			
		}

		public bool Enabled
		{
			get
			{
				return enabled;
			}
			
			set
			{
				enabled = value;
			}
		}

		public virtual void OnReset(Agent agent) {

		}
	}
}
