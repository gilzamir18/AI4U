using Godot;
using System;
using System.Text;

namespace ai4u
{
    /// <summary>
    /// A controller is an object that sends actions to an actuator through 
    /// callback function GetAction. GetAction returns a string action. A string
    /// action is a AI4U compressed action description that an Agent (BasicAgent, for example)
    /// understand.
    /// </summary>
    public abstract partial class Controller: Node
	{
		
		public int LastStep {get; set;}
		public float LastReward {get; set;}
		
		protected Agent agent;		
		private string[] desc;
		private byte[] type;
		private string[] value;
		
		public Controller(): base()
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
		}


		public void Setup(Agent agent)
		{
			this.agent = agent;
			OnSetup();
		}

		public virtual void OnSetup()
		{

		}
		
		public virtual string GetAction()
		{
			return "";
		}
		
		public virtual void NewStateEvent()
		{
			
		}
		
		public string GetStateAsString(int i = 0)
		{
			return value[i];
		}

		public int GetStateSize()
		{
			return this.desc.Length;
		}

		public float GetStateAsFloat(int i = 0)
		{
			return float.Parse(value[i], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
		}

		public bool GetStateAsBool(int i = 0)
		{
			bool vb;
			if (bool.TryParse(value[i], out vb))
			{
				return vb;
			}
			else
			{
				int vi = 0;
				if (int.TryParse(value[i], out vi))
				{
					return vi != 0;
				}
				else
				{
					throw new InvalidCastException($"String {value[i]} cannot casted in boolean!");
				}
			}
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
			
		}
	}
}
