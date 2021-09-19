using Godot;
using System;
using System.Collections.Generic;

namespace ai4u
{
	public class Agent : Node
	{
		protected Brain brain;
		public int numberOfFields = 0;
		protected string[] desc;
		protected byte[] types;
		protected string[] values;
		private List<IAgentResetListener> resetListener = new List<IAgentResetListener>();

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

		/***
		This method receives client's command to apply to remote environment.
		***/
		public virtual void ApplyAction()
		{
		}

		public virtual void StartData()
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
			desc = new string[numberOfFields];
			types = new byte[numberOfFields];
			values = new string[numberOfFields];
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
			
		public virtual void UpdatePhysics()
		{
		}

		public virtual void UpdateState()
		{
		}

		public void SetBrain(Brain brain)
		{
			this.brain = brain;
		}
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			
		}
	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	//  public override void _Process(float delta)
	//  {
	//      
	//  }
	}

	public interface IAgentResetListener
	{
		void OnReset(Agent agent);
	}
}
