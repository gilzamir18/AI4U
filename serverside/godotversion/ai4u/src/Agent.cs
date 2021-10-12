using Godot;
using System;
using System.Collections.Generic;
using ai4u.ext;

namespace ai4u
{
	public abstract class Agent : Node
	{
		protected Brain brain;
		protected string[] desc;
		protected byte[] types;
		protected string[] values;
		
		protected List<Sensor> sensors;
		protected List<Actuator> actuators; 
			
		private List<IAgentResetListener> resetListener = new List<IAgentResetListener>();
		private int numberOfFields = 0;
		private int numberOfActions = 0;
		
		public Agent() {
			
		}
		
		public Sensor GetSensor(int i)
		{
			return this.sensors[i];	
		}
		
		public int GetNumberOfSensors()
		{
			return this.numberOfFields;
		}
		
		public int GetNumberOfActions()
		{
			return this.numberOfActions;
		}
		
		public float DeltaTime
		{
			get 
			{
				return brain.DeltaTime;
			}
		}
		
		public virtual void StartData()
		{
			sensors = new List<Sensor>();
			actuators = new List<Actuator>();
			OnSetup();
			foreach (Node node in GetChildren())
			{
				if ( node.GetType().IsSubclassOf(typeof(Sensor)) ) {
					Sensor s = node as Sensor;
					sensors.Add(s);
					if (s.resettable)
					{
						AddResetListener(s);	
					}
					s.OnBinding(this);
				}
				if ( node.GetType().IsSubclassOf(typeof(Actuator)) ) {
					Actuator a = node as Actuator;
					actuators.Add(a);
					if (a.resettable)
					{
						AddResetListener(a);	
					}
					a.OnBinding(this);
				}
			}
			
			numberOfFields = sensors.Count;
			numberOfActions = actuators.Count;
			
			if (numberOfFields == 0) {
				GD.Print("The agent should have at least one sensor! Target name: " + GetParent().Name);
			}
			
			if (numberOfActions == 0) {
				GD.Print("The agent should have at least one actuator! Target name: " + GetParent().Name);
			}

			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
			desc = new string[numberOfFields];
			types = new byte[numberOfFields];
			values = new string[numberOfFields];
			OnSetupDone();
		}
		
		public void AddSensor(Sensor s) 
		{
			this.sensors.Add(s);
		}
		
		public void AddActuator(Actuator a)
		{
			this.actuators.Add(a);
		}

		public virtual void OnSetup() 
		{

		}
		
		public virtual void OnSetupDone()
		{
			
		}
		
		public virtual void AtBeginingOfTheStateUpdate()
		{
			
		}
		
		public virtual void AtEndOfTheStateUpdate()
		{
			
		}
		
		public virtual bool FilterAction(Actuator a)
		{
			return true;
		}

		public abstract Node GetBody();
		
		/***
		This method receives client's command to apply to remote environment.
		***/
		public virtual void ApplyAction()
		{
			for (int i = 0; i < numberOfActions; i++)
			{
				if (
					actuators[i].always ||
					(actuators[i].actionName == GetActionName() && FilterAction(actuators[i]))
				)
				{
					actuators[i].Act();	
				}
			}
		}
		
					
		public virtual void UpdatePhysics()
		{
		}

		public virtual void UpdateState()
		{
			AtBeginingOfTheStateUpdate();
			for (int i = 0; i < numberOfFields; i++) {
				switch(sensors[i].type)
				{
					case SensorType.sfloatarray:
						SetStateAsFloatArray(i, sensors[i].perceptionKey, sensors[i].GetFloatArrayValue());
						break;
					case SensorType.sfloat:
						SetStateAsFloat(i, sensors[i].perceptionKey, sensors[i].GetFloatValue());
						break;
					case SensorType.sint:
						SetStateAsInt(i, sensors[i].perceptionKey, sensors[i].GetIntValue());
						break;
					case SensorType.sstring:
						SetStateAsString(i, sensors[i].perceptionKey, sensors[i].GetStringValue());
						break;
					case SensorType.sbool:
						SetStateAsBool(i, sensors[i].perceptionKey, sensors[i].GetBoolValue());
						break;
					case SensorType.sbytearray:
						SetStateAsByteArray(i, sensors[i].perceptionKey, sensors[i].GetByteArrayValue());
						break;
					default:
						break;
				}
			}
			AtEndOfTheStateUpdate();
		}

		public void AddResetListener(IAgentResetListener listener) 
		{
			resetListener.Add(listener);
		}

		public void RemoveResetListenerAt(int pos) {
			resetListener.RemoveAt(pos);
		}

		public bool RemoveResetListenerAt(IAgentResetListener listener) {
			return resetListener.Remove(listener);
		}
		
		public void SetState(int i, string desc, byte type, string value)
		{
			this.desc[i] = desc;
			this.types[i] = type;
			this.values[i] = value;
		}

		public void SetStateAsFloatArray(int i, string desc, float[] value)
		{
			this.desc[i] = desc;
			this.types[i] = Brain.FLOAT_ARRAY;
			this.values[i] = string.Join(" ", value);
		}

		public void SetStateAsInt(int i, string desc, int value)
		{
			this.desc[i] = desc;
			this.types[i] = Brain.INT;
			this.values[i] = value.ToString();
		}

		public virtual void HandleOnResetEvent() {
		}

		public void NotifyReset() {
			this.HandleOnResetEvent();
			foreach (IAgentResetListener listener in resetListener) {
				listener.OnReset(this);
			}
		}

		public void SetStateAsFloat(int i, string desc, float value)
		{
			this.desc[i] = desc;
			this.types[i] = Brain.FLOAT;
			this.values[i] = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
		}

		public void SetStateAsString(int i, string desc, string value)
		{
			this.desc[i] = desc;
			this.types[i] = Brain.STR;
			this.values[i] = value;
		}

		public void SetStateAsBool(int i, string desc, bool value)
		{
			this.desc[i] = desc;
			this.types[i] = Brain.BOOL;
			this.values[i] = value ? "1" : "0";
		}

		public void SetStateAsByteArray(int i, string desc, byte[] value)
		{
			this.desc[i] = desc;
			this.types[i] = Brain.OTHER;
			this.values[i] = System.Convert.ToBase64String(value);
		}

		private static float ParseFloat(string v) {
			return float.Parse(v, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
		}

		public int GetStateIndex(string description)
		{
			for (int i = 0; i < this.desc.Length; i++) {
				if (desc[i] == description) 
				{
					return i;
				}
			}
			return -1;
		}

		public string[] GetStateDescriptions()
		{
			return (string[])this.desc.Clone();
		}

		public byte GetStateType(int idx)
		{
			return this.types[idx];
		}

		public string GetStateValue(int idx)
		{
			return this.values[idx];
		}

		public string GetActionArgAsString(int i=0)
		{
			return this.brain.GetReceivedArgs()[i];
		}

		public float GetActionArgAsFloat(int i=0)
		{
			return float.Parse(this.brain.GetReceivedArgs()[i], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
		}

		public bool GetActionArgAsBool(int i = 0)
		{
			return bool.Parse(this.brain.GetReceivedArgs()[i]);
		}

		public float[] GetActionArgAsFloatArray(int i = 0)
		{
			return System.Array.ConvertAll(this.brain.GetReceivedArgs()[i].Split(' '), ParseFloat);
		}

		public int GetActionArgAsInt(int i = 0)
		{
			return int.Parse(this.brain.GetReceivedArgs()[i]);
		}

		public string GetActionName()
		{
			return this.brain.GetReceivedCommand();
		}

		public void GetState()
		{
			if (this.brain == null)
			{
				GD.Print("Remote not found!");
				return;
			}
			try
			{
				this.brain.SendMessage(desc, types, values);
			}
			catch (System.Exception e)
			{
				GD.Print(e.Message);
				GD.Print(e.StackTrace);
			}
		}

		public void SetBrain(Brain brain)
		{
			this.brain = brain;
		}
	}

	public interface IAgentResetListener
	{
		void OnReset(Agent agent);
	}
}
