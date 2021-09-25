using Godot;
using System;
using System.Collections.Generic;

namespace ai4u
{
	public abstract class Controller: Node, IAgentResetListener
	{
		protected string[] desc;
		protected byte[] type;
		protected string[] value;
		
		[Export]
		public bool showLastStateDescription = true;
		
		[Export]
		public bool resettable = true;

		private Dictionary<string, object> lastValues = new Dictionary<string, object>();

		private int printCounter = 0;

		public Controller(): base()
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
		}


		public virtual object[] GetAction()
		{
			return new System.Object[] { };
		}


		public virtual void NewStateEvent()
		{
			if (showLastStateDescription)
				for (int i = 0; i < desc.Length; i++) 
				{
					if (printCounter == 0 || !lastValues[desc[i]].Equals(value[i]))
					{
						GD.Print("States(" + desc[i] + ") = " + value[i]);		
					}
					lastValues[desc[i]] = value[i];
				}
			printCounter += 1;
		}

		public object[] GetFloatArrayAction(string action, float[] value)
		{
			return new object[] { action, new string[] { string.Join(" ", value) } };
		}

		public object[] GetIntAction(string action, int value)
		{
			return new object[] { action, new string[] { value.ToString() } };
		}

		public object[] GetFloatAction(string action, float value)
		{
			return new object[] { action, new string[] { value.ToString(System.Globalization.CultureInfo.InvariantCulture) } };
		}

		public object[] GetStringAction(string action, string value)
		{

			return new object[] {action, new string[] { value }};
		}

		public object[] GetBoolAction(string action, bool value)
		{
				return new object[] { action, new string[] { value ? "1" : "0" } };
		}

		public object[] GetByteArrayAction(string action, byte[] value)
		{
			return new object[] { action, new string[] { System.Convert.ToBase64String(value) }};
		}

		public string GetStateAsString(int i = 0)
		{
			return value[i];
		}

		public float GetStateAsFloat(int i = 0)
		{
			return float.Parse(value[i], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
		}

		public bool GetStateAsBool(int i = 0)
		{
			return bool.Parse(value[i]);
		}

		public string GetStateName(int i = 0)
		{
			return desc[i];
		}

		public float[] GetStateAsFloatArray(int i = 0)
		{
			return System.Array.ConvertAll(value[i].Trim().Split(' '), float.Parse);
		}

		public int GetStateAsInt(int i = 0)
		{
			return int.Parse(value[i]);
		}

		public void ReceiveState(string[] desc, byte[] type, string[] val)
		{
			this.type = type;
			this.value = val;
			this.desc = desc;
			this.NewStateEvent();
		}
		
		public virtual void OnReset(Agent agent) 
		{
			lastValues = new Dictionary<string, object>();	
			printCounter = 0;
		}
	}
}
