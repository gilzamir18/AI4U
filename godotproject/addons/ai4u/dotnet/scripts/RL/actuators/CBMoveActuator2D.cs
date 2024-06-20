using Godot;
using System;


namespace ai4u;

public partial class CBMoveActuator2D : MoveActuator
{
    //forces applied on the x, y and z axes.    
    private float move, turn, jump, jumpForward;

    [ExportCategory("Movement")]
    [Export]
    private float moveAmount = 1;
    [Export]
    private float turnAmount = 1;
    [Export]
    private float jumpPower = 1;
    [Export]
    private float jumpForwardPower = 1;
    [Export]
    private float precision = 0.001f;
    [Export]
    private float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
    
    [Export]
    private float lerpFactor = 0.4f;

    [Export]
    private bool flipWhenTurn = true;

    [ExportCategory("Action Shape")]
    [Export]
    private float[] actionRangeMin = new float[]{0, -1, -1, -1};
    [Export]
    private float[] actionRangeMax = new float[]{1, 1, 1, 1};

    private BasicAgent agent;
    
    private CharacterBody2D body;
    private PhysicsDirectSpaceState2D spaceState;

    private CollisionShape2D collisionShape;

    public CBMoveActuator2D()
    {

    }

    public float Gravity 
    {
        get
        {
            return gravity;
        }

        set 
        {
            gravity = value;
        }
    }

    public override void OnSetup(Agent agent)
    {
        shape = new int[1]{4};
        isContinuous = true;
        rangeMin = actionRangeMin;
        rangeMax = actionRangeMax;
        this.agent = (BasicAgent) agent;
        agent.AddResetListener(this);
        body = this.agent.GetAvatarBody() as CharacterBody2D;
        this.spaceState = body.GetWorld2D().DirectSpaceState;
    }

    public override bool OnGround
    {
        get
        {
            return body.IsOnFloor();
        }
    }

    public override void Act()
    {
        double delta = this.agent.ControlInfo.deltaTime;
        Vector2 velocity = body.Velocity;
        if (agent != null && !agent.Done)
        {
            float[] action = agent.GetActionArgAsFloatArray();
            move = action[0];
            turn = action[1];
            jump = action[2];
            jumpForward = action[3];
            if (Mathf.Abs(turn) < precision)
            {
                turn = 0;
            }
						
            if (Mathf.Abs(jump) < precision)
            {
                jump = 0;
            }
						
            if (Mathf.Abs(jumpForward) < precision)
            {
                jumpForward = 0;
            }

            if (Mathf.Abs(move) < precision)
            {
                move = 0;
            }
            
            // Add the gravity.
            if (!body.IsOnFloor() && body.MotionMode == CharacterBody2D.MotionModeEnum.Grounded)
            {
			    velocity.Y += gravity * 10 * (float)delta;
            }
            else
            {
                if ( Math.Abs(turn) > precision)
                {
                    if (flipWhenTurn && Math.Abs(turnAmount) > precision)
                    {
                        if ( (turn > 0 && body.Transform.Scale.Y < 0) || (turn < 0 && body.Transform.Scale.Y > 0))
                        {
                            body.Scale *= new Vector2(-1, 1);
                        }
                    }
                    else
                    {
                        body.Rotate(Mathf.DegToRad(-turn * turnAmount));
                    }
                }

                // Get the input direction and handle the movement/deceleration.
                Vector2 direction = body.GlobalTransform.X.Normalized();

                if (jump > 0)
                {
                    velocity -= body.GlobalTransform.Y * jump * jumpPower * 100;
                }

                bool forwarding = false;

                if (jumpForward > 0)
                {
                    velocity -= body.GlobalTransform.Y * jumpForwardPower * jumpForward * 100;
                    velocity += body.GlobalTransform.X * jumpForwardPower * jumpForward * 100;
                    forwarding = true;
                }
                
                if (Mathf.Abs(move) > precision)
                {
                    velocity +=  body.GlobalTransform.X * moveAmount * move * Mathf.Sign(body.Transform.Scale.Y);
                    forwarding = true;    
                }

                if (!forwarding)
                {
                    velocity.X = Mathf.Lerp(velocity.X, 0, lerpFactor);
                    velocity.Y = Mathf.Lerp(velocity.Y, 0, lerpFactor);
                }
            }
            body.Velocity = velocity;
            body.MoveAndSlide();
        }
        move = 0;
        turn = 0;
        jump = 0;
        jumpForward = 0;
    }

    public override void OnReset(Agent agent)
    {
        turn = 0;
        move = 0;
        jump = 0;
        jumpForward = 0;
    }
}
