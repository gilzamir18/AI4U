using ai4u;
using Godot;
using System;

/// <summary>
/// DiscretArrowController represents a keyboard controller that sends actions based on keyboard commands.
/// </summary>
public partial class DiscretArrowController : Controller
{

	/// <summary>
	/// Action name of the DiscretActuator that this DiscretArrowController is bound.
	/// </summary>
	[Export]
	public string actuatorName = "move";

	private bool done = true; //flag of end of episode.

	override public void OnSetup()
	{
		done = true;
	}

	override public void OnReset(Agent agent)
	{
		done = false;
	}

	override public string GetAction()
	{
		int actionValue = 0;
		string actionName = actuatorName;
		if (agent.Alive())
		{
			if (Input.IsActionJustPressed("ui_right"))
			{
				actionValue = 1;
			}

			if (Input.IsActionJustPressed("ui_left"))
			{
				actionValue = 2;
			}

			if (Input.IsActionJustPressed("ui_up"))
			{
				actionValue = 3;
			}

			if (Input.IsActionJustPressed("ui_down"))
			{
				actionValue = 4;
			}

			if (Input.IsActionJustPressed("ui_accept"))
			{
				actionValue = 5;
			}

			if (Input.IsKeyPressed(Key.R))
			{
				actionName = "__restart__";
				return ai4u.Utils.ParseAction(actionName, actionValue);
			}

			if (actionName != "__restart__")
			{
				return ai4u.Utils.ParseAction(actionName, actionValue);
			}
		}
		else
		{
			if (Input.IsKeyPressed(Key.R))
			{
				actionName = "__restart__";
			}
		}

		if (actionName == "__restart__")
		{
			return ai4u.Utils.ParseAction("__restart__");
		}
		else
		{
			return ai4u.Utils.ParseAction("__waitnewaction__");
		}
	}

	override public void NewStateEvent()
	{
		if (!agent.Alive() && !done)
		{
			GD.Print("Episode Reward " + ((RLAgent)agent).EpisodeReward);
			done = true;
		}
	}
}
