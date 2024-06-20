using System.Collections;
using System.Collections.Generic;
using Godot;
using ai4u;

namespace  ai4u;

public partial class TrainController : Controller
{
	[Export]
	public NodePath trainerPath;

	[Export]
	public bool tryInitialize = false;

	private Trainer trainer;

	private string cmdName = null;
	private float[] fargs = null;
	private int[] iargs = null;
	private bool initialized = false; //indicates if episode has been initialized.

	override public void OnSetup()
	{		
		trainer = GetNode(trainerPath) as Trainer;
		trainer.Initialize(this, agent);
	}
	
	override public void OnReset(Agent agent)
	{
		trainer.OnReset(agent);
	}

	public void RequestContinuousAction(string name, float[] args)
	{
		this.cmdName = name;
		this.fargs = args;
	}

	public void RequestAction(string name, int[] args)
	{
		this.cmdName = name;
		this.iargs = args;
	}

	override public string GetAction()
	{
		if (GetStateAsString(0) == "envcontrol")
		{
			if (GetStateAsString(1).Contains("restart"))
			{
				return ai4u.Utils.ParseAction("__restart__");
			}
			return ai4u.Utils.ParseAction("__noop__");			
		}
		if (cmdName != null && !((BasicAgent)agent).Done )
		{
			if (iargs != null) 
			{
				string cmd = cmdName;
				int[] args = iargs;
				ResetCmd();
				return ai4u.Utils.ParseAction(cmd, args);
			}
			else if (fargs != null)
			{
				string cmd = cmdName;
				float[] args = fargs;
				ResetCmd();
				return ai4u.Utils.ParseAction(cmd, args);
			}
			else
			{
				string cmd = cmdName;
				ResetCmd();
				return ai4u.Utils.ParseAction(cmd);
			}
		}
		else 
		{
			if (initialized)
			{
				if (tryInitialize)
				{
					initialized = true;
					return ai4u.Utils.ParseAction("__restart__");
				}
				return ai4u.Utils.ParseAction("__noop__");
			}
			else
			{
				initialized = true;
				return ai4u.Utils.ParseAction("__restart__");
			}
		}
	}

	override public void NewStateEvent()
	{
		if (GetStateAsString(0) == "envcontrol")
		{
			trainer.EnvironmentMessage();			
		} else
		{
			trainer.StateUpdated();
		}
	}

	private void ResetCmd()
	{
		this.cmdName = null;
		this.iargs = null;
		this.fargs = null;
	}
}

