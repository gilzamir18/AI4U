using Godot;
using System;

namespace ai4u;

public partial class DiscretActuator : Actuator
{
    //forces applied on the x, y and z axes.    
    [Export]
    private int size = 5;

    private BasicAgent agent;

    public override void OnSetup(Agent agent)
    {
        shape = new int[1] { size };
        isContinuous = false;
        rangeMin = new float[size];
        rangeMax = new float[size];
        this.agent = (BasicAgent)agent;
        agent.AddResetListener(this);
    }

    public override void Act()
    {
        if (agent != null && !agent.Done)
        {
            int action = agent.GetActionArgAsInt();
            agent.GetAvatarBody().Call("SetAction", action);
        }
    }

    public override void OnReset(Agent agent)
    {
        this.agent.GetAvatarBody().Call("Reset");
    }
}
