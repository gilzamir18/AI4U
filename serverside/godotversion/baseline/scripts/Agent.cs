using Godot;
using System.Collections.Generic;

namespace ai4u
{
	///<summary>
	///An agent is an object that supports the cycle of updating the state 
	///represented by the tuple (s[t], a, s[t + 1]), where s [t] is the current 
	///state, s [t+1] is the next state and 'a' is the action taken that resulted 
	///in s[t+1]. An agent receives an action or command from a controlle (instance of the Brain class),
	///executes this action in the environment and returns to the controller the resulting 
	///state named s[t+t1]. </summary>

	public abstract class Agent : Node
	{
		public delegate void ResetHandler(Agent source);

		public event ResetHandler resetEvent;

		protected Brain brain;
		public string ID = "0";

		public int numberOfFields = 0;
		
		protected int nSteps;
		protected string[] desc;
		protected byte[] types;
		protected string[] values;
		protected bool setupIsDone = false;

		public byte[] MessageType
		{
			get
			{
				return types;
			}
		}

		public int NSteps
		{
			get
			{
			   return nSteps; 
			}

			set
			{
				nSteps = value;
			}
		}

		public string[] MessageValue
		{
			get 
			{
				return values;
			}
		}

		public string[] MessageID
		{
			get
			{
				return desc;
			}
		}

		public void AddResetListener(IAgentResetListener listener) 
		{
			resetEvent += listener.OnReset;
		}

		public Brain Brain
		{
			get
			{
				return brain;
			}
		}

		/***
		This method receives client's command to apply to remote environment.
		***/
		public virtual void ApplyAction()
		{
		}

		public virtual void EndOfEpisode()
		{

		}

		public bool SetupIsDone
		{
			get
			{
				return setupIsDone;
			}
		}

		public virtual void Setup()
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

		public void NotifyReset() {
			if (resetEvent != null)
			{
				resetEvent(this);
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

		public float[] GetActionArgAsFloatArray()
		{
			return System.Array.ConvertAll(this.brain.GetReceivedArgs(), ParseFloat);
		}

		public int GetActionArgAsInt(int i = 0)
		{
			return int.Parse(this.brain.GetReceivedArgs()[i]);
		}

		public string GetFieldArgAsString(string cmdname, int argidx=0)
		{
			string[] args = brain.GetField(cmdname);
			return args[argidx];
		}

		public float GetFieldArgAsFloat(string cmdname, int i = 0)
		{
			string[] args = brain.GetField(cmdname);
			return float.Parse(args[i], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
		}

		public bool GetFieldArgAsBool(string cmdname, int i = 0)
		{
			string[] args = brain.GetField(cmdname);
			return bool.Parse( args[i] );
		}

		public float[] GetFieldArgAsFloatArray(string cmdname)
		{
			string[] args = brain.GetField(cmdname);
			return System.Array.ConvertAll(args, ParseFloat);
		}

		public int[] GetFieldArgAsIntArray(string cmdname)
		{
			string[] args = brain.GetField(cmdname);
			return System.Array.ConvertAll<string,  int>(args,  int.Parse);
		}

		public int GetFieldArgAsInt(string cmdname, int i = 0)
		{
			string[] args = brain.GetField(cmdname);
			return int.Parse(args[i]);
		}

		public string GetActionName()
		{
			return this.brain.GetReceivedCommand();
		}

		public virtual void UpdateState()
		{
		}

		public virtual void Reset()
		{
			nSteps = 0;
		}

		public virtual bool Alive()
		{
			return true;
		}

		public void SetBrain(Brain brain)
		{
			this.brain = brain;
		}
	}

	///<summary>The Brain class communicates with the character's controller, that is, with the remote or 
	///local mechanism it takes, selects the next action given the current state. This class does not fix
	/// any particular decision-making approach, but rather encapsulates a decision-making protocol that 
	///allows the agent to be controlled remotely (code in programming languages ​​other than those supported by Unity)
	///or locally (scripts that use languages ​​supported by Unity. Python is the naturally supported remote 
	///scripting language. But others may be supported in the future. AI4U provides two instances of Brain. 
	///One is a remote controller called RemoteBrain, which allows a remote controller to send 
	///commands to an avatar. a local controller, which allows commands to be sent without using 
	/// network protocols. A local controller can be used to adapt the use of a trained model 
	/// using a remote controller. This is a possible scenario given that there are many algorithms
	///and frameworks that they are easier for prototyping than with a Unity language.</summary>
	public abstract class Brain : Node
	{
		public static byte FLOAT = 0;
		public static byte INT = 1;
		public static byte BOOL = 2;
		public static byte STR = 3;
		public static byte OTHER = 4;
		public static byte FLOAT_ARRAY = 5;
		protected string receivedcmd; 
		protected string[] receivedargs;
		private Dictionary<string, string[]> commandFields;
		
		protected Agent agent = null;
		protected ControlRequestor controlRequestor;

		public ControlRequestor ControlRequestor
		{
			get
			{
				return controlRequestor;
			}
			
			set
			{
				controlRequestor = value;
			}
		}

		public void SetCommandFields(Dictionary<string, string[]>  cmdField)
		{
			this.commandFields = cmdField;
		} 

		public string[] GetField(string name)
		{
			return this.commandFields[name];
		}

		public void SetReceivedCommandName(string cmdname)
		{
			receivedcmd = cmdname;
		}

		public bool containsCommandField(string cmd)
		{
			if (commandFields != null)
			{
				return commandFields.ContainsKey(cmd);
			}
			return false;
		}

		public void SetReceivedCommandArgs(string[] args)
		{
			receivedargs = args;    
		}

		public string GetReceivedCommand()
		{
			return receivedcmd;
		}

		public string[] GetReceivedArgs(string cmd=null)
		{
			if (cmd == null)
			{
				return receivedargs;
			}
			else
			{
				return commandFields[cmd];
			}
		}
		
		// Called when the node enters the scene tree for the first time.
		public virtual void OnSetup(Agent agent)
		{
			this.agent = agent;
		}
	}

	public interface IAgentResetListener
	{
		void OnReset(Agent agent);
	}
}

