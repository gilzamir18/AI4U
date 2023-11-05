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
            if (Input.IsKeyPressed(Key.Right))
            {
                actionValue = 1;
            }

            if (Input.IsKeyPressed(Key.Left))
            {
                actionValue = 2;
            }

            if (Input.IsKeyPressed(Key.Up))
            {
                actionValue = 3;
            }

            if (Input.IsKeyPressed(Key.Space))
            {
                actionValue = 4;
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
            GD.Print("Episode Reward " + ((BasicAgent)agent).AcummulatedReward);
            done = true;
        }
    }
}
