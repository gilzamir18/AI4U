using Godot;
using System;

namespace ai4u;

public partial class RBMoveActuator2D : Actuator
{
    //forces applied on the x, y and z axes.    
    private float move, jump, turn, jumpForward;
    [Export]
    public float moveAmount = 1;
    [Export]
    public float jumpPower = 1;
    [Export]
    public float jumpForwardPower = 1;
    [Export]
    public float minActivityThreshold = 0.001f;

    private RLAgent agent;

    private RigidBody2D rBody;

    public override void OnSetup(Agent agent)
    {
        shape = new int[1] { 4 };
        isContinuous = true;
        rangeMin = new float[] { 0, -1, 0, 0 };
        rangeMax = new float[] { 1, 1, 1, 1 };
        this.agent = (RLAgent)agent;
        agent.AddResetListener(this);
        rBody = this.agent.GetAvatarBody() as RigidBody2D;
    }

    private bool onGround = false;

    public bool OnGround
    {
        get
        {
            return onGround;
        }
    }

    public override void Act()
    {
        if (agent != null && !agent.Done)
        {
            float[] action = agent.GetActionArgAsFloatArray();
            move = action[0];
            turn = action[1];
            jump = action[2];
            jumpForward = action[3];

            if (rBody != null)
            {
                if (Mathf.Abs(rBody.LinearVelocity.Y) > 0.001)
                {
                    onGround = false;
                }
                else
                {
                    onGround = true;
                }
                if (onGround)
                {
                    if (Mathf.Abs(turn) < minActivityThreshold)
                    {
                        turn = 0;
                    }

                    if (Mathf.Abs(jump) < minActivityThreshold)
                    {
                        jump = 0;
                    }

                    if (Mathf.Abs(jumpForward) < minActivityThreshold)
                    {
                        jumpForward = 0;
                    }
                }
            }
        }
        move = 0;
        jump = 0;
        turn = 0;
        jumpForward = 0;
    }

    public override void OnReset(Agent agent)
    {
        turn = 0;
        move = 0;
        jump = 0;
        jumpForward = 0;
        onGround = false;
    }
}
