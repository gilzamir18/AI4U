using System.Collections;
using System.Collections.Generic;
using Godot;

namespace ai4u.ext
{	
	
	public interface ISensorListener
	{
		void OnSensor(Sensor sensor);
	}
	
	public enum SensorType {
		sfloat,
		sstring,
		sbool,
		sint,
		sbytearray,
		sfloatarray,
		sintarray
	}

	public class Sensor : Node, IAgentResetListener, IActionListener
	{
		
		private List<ISensorListener> sensorListeners = new List<ISensorListener>();
		
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
		
		public virtual void OnAction(Actuator actuator)
		{
			
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
		
		public void Subscribe(ISensorListener listener)
		{
			sensorListeners.Add(listener);
		}
		
		public void Unsubscribe(ISensorListener listener)
		{
			sensorListeners.Remove(listener);
		}
		
		public void NotifyListeners()
		{
			foreach(ISensorListener s in sensorListeners)
			{
				s.OnSensor(this);
			}
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
