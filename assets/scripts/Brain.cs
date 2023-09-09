using Godot;
using System.Collections.Generic;

namespace ai4u;
	
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
public abstract class Brain
{
	public static byte FLOAT = 0;
	public static byte INT = 1;
	public static byte BOOL = 2;
	public static byte STR = 3;
	public static byte OTHER = 4;
	public static byte FLOAT_ARRAY = 5;
	
	protected Agent agent = null;
	
	protected string receivedcmd; 
	protected string[] receivedargs;

	private Dictionary<string, string[]> commandFields;
	
	public virtual void Setup(Agent agent)
	{
		
	}

	public virtual void Close()
	{
		
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
	
	public virtual void OnReset(Agent agent)
	{
		
	}
}
