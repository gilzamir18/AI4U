using Godot;
using System;


namespace ai4u;

public partial class CBMoveActuator : MoveActuator
{
    //forces applied on the x, y and z axes.    
    private float move, turn, jump, jumpForward;

    [ExportCategory("Movement")]
    [Export]
    private float moveAmount = 1;
    [Export]
    private float backwarAmount = 0.1f;
    [Export]
    private float turnAmount = 1;
    [Export]
    private float jumpPower = 1;
    [Export]
    private float jumpForwardPower = 1;
    [Export]
    private float deadZone = 0.001f;
    [Export]
    private float velocityDump = 1f;
    [Export]
    private float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
 

    [ExportCategory("Action Shape")]
    [Export]
    private float[] actionRangeMin = new float[]{0, -1, -1, -1};
    [Export]
    private float[] actionRangeMax = new float[]{1, 1, 1, 1};

    private RLAgent agent;
    
    private CharacterBody3D body;
    private PhysicsDirectSpaceState3D spaceState;

    private CollisionShape3D collisionShape;

    public CBMoveActuator()
    {

    }

    public override void OnSetup(Agent agent)
    {
        shape = new int[1]{4};
        isContinuous = true;
        rangeMin = actionRangeMin;
        rangeMax = actionRangeMax;
        this.agent = (RLAgent) agent;
        agent.AddResetListener(this);
        body = this.agent.GetAvatarBody() as CharacterBody3D;
        this.spaceState = body.GetWorld3D().DirectSpaceState;
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
        Vector3 velocity = body.Velocity;
        if (agent != null && !agent.Done)
        {
            float[] action = agent.GetActionArgAsFloatArray();
            move = action[0];
            turn = action[1];
            jump = action[2];
            jumpForward = action[3];
            float amount = moveAmount;
            if (move < 0)
            {
                amount = backwarAmount;
            }

            if (Mathf.Abs(turn) < deadZone)
            {
                turn = 0;
            }
						
            if (Mathf.Abs(jump) < deadZone)
            {
                jump = 0;
            }
						
            if (Mathf.Abs(jumpForward) < deadZone)
            {
                jumpForward = 0;
            }

            if (Mathf.Abs(move) < deadZone)
            {
                move = 0;
            }
            
            // Add the gravity.
            if (!body.IsOnFloor())
            {
			    velocity.Y -= gravity * (float)delta * 10;
            }
            else
            {
               /* if ( Math.Abs(turn) > 0)
                {
                    body.Rotate(body.Basis.Y.Normalized(), Mathf.DegToRad(-turn * turnAmount * (float)delta * 100));
                }*/

                if (Math.Abs(turn) > 0)
                {
                    var newDir = -body.Basis.Z.Rotated(body.Basis.Y, Mathf.DegToRad(-turn * turnAmount));

                    body.LookAt(body.GlobalPosition + newDir);
                }

                // Get the input direction and handle the movement/deceleration.
                Vector3 direction = body.Transform.Basis.Z.Normalized();

                if (jump > 0)
                {
                    velocity += body.Transform.Basis.Y * jump * jumpPower;
                }

                bool forwarding = false;

                if (jumpForward > 0)
                {
                    velocity += body.Transform.Basis.Y * jumpForwardPower * jumpForward;
                    velocity += body.Transform.Basis.Z * jumpForwardPower * jumpForward;
                    forwarding = true;
                }
                
                if (Mathf.Abs(move) > deadZone)
                {
                    velocity +=  body.Transform.Basis.Z * amount * move;
                    forwarding = true;    
                }

                if (!forwarding)
                {
                    velocity.X = Mathf.MoveToward(velocity.X, 0, velocityDump);
                    velocity.Z = Mathf.MoveToward(velocity.Z, 0, velocityDump);
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
