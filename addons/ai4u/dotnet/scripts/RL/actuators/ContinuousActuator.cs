using ai4u;
using Godot;
using System;

public partial class ContinuousActuator : Actuator
{
    //forces applied on the x, y and z axes.    
    [Export]
    public int size = 5;

    [Export]
    public Node ActionHandler { get; set; }

    private RLAgent agent;

    public override void OnSetup(Agent agent)
    {
        shape = new int[1] { size };
        isContinuous = false;
        rangeMin = new float[size];
        rangeMax = new float[size];

        for (int i = 0; i < size; i++)
        {
            rangeMin[i] = -1;
            rangeMax[i] = 1;
        }

        this.agent = (RLAgent)agent;

        if (ActionHandler == null)
        {
            ActionHandler = this.agent.GetAvatarBody();
        }

        agent.AddResetListener(this);
    }

    public override void Act()
    {
        if (agent != null && !agent.Done)
        {
            float[] action = agent.GetActionArgAsFloatArray();
            ActionHandler.Call("SetAction", action);
        }
    }

    public override void OnReset(Agent agent)
    {
        this.agent.GetAvatarBody().Call("Reset");
    }
}
