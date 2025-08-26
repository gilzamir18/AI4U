using Godot;
using System;

namespace ai4u;

[Tool]
[GlobalClass]
public partial class CBMoveActuator : MoveActuator
{
    //forces applied on the x, y and z axes.    
    private float move_v, move_h, turn, jump, jumpForward;

    private const int NB_ACTIONS = 5;

    [ExportCategory("Movement")]
    [Export]
    private float moveAmount = 10;
    [Export]
    private bool keepForward = true;
    [Export]
    private float turnAmount = 20;
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
    [Export(PropertyHint.ResourceType, "FieldRange")] public FieldRange[] actionRange = new FieldRange[NB_ACTIONS];

    [Export]
    private int extraActions = 0;

    private float[] actionRangeMin = new float[]{-1, -1, -1, -1, -1};
    private float[] actionRangeMax = new float[]{1, 1, 1, 1, 1};

    private Vector3 oldDir = Vector3.Zero;


    public float SpeedUp { get; set; } = 0;
    public float MoveNoise { get; set; } = 0;

    public float TurnNoise { get; set; } = 0;

    public float JumpNoise { get; set; } = 0;


    public float TurnAmount
    { 
        get { return turnAmount; } 
    }

    public float JumpPower
    { 
        get { return jumpPower; }
    }
    
    public float JumpForwardPower
    { 
        get { return jumpForwardPower; }
    }

    public float VelocityDump
    { 
        get { return velocityDump; } 
    }

    public float DeadZone
    { 
        get { return deadZone; } 
    }

    [Signal]
    public delegate void OnMoveStepEventHandler(CBMoveActuator actuator, Vector3 direction, float turn, float jump, float jumpForward, float[] extraActions);

    private RLAgent agent;
    
    private CharacterBody3D body;
    private PhysicsDirectSpaceState3D spaceState;

    private CollisionShape3D collisionShape;

    public CBMoveActuator()
    {
    }

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
        {
            if (actionRange.Length>=5)
            {
                actionRange[0] = new FieldRange(-1, 1);
                actionRange[1] = new FieldRange(-1, 1);
                actionRange[2] = new FieldRange(-1, 1);
                actionRange[3] = new FieldRange(-1, 1);
                actionRange[4] = new FieldRange(-1, 1);
            }
        }
    }   

    public override void OnSetup(Agent agent)
    {
        actionRangeMin = new float[actionRange.Length];
        actionRangeMax = new float[actionRange.Length];
        for (int i = 0; i < actionRange.Length; i++)
        {
            actionRangeMin[i] = actionRange[i].Min;
            actionRangeMax[i] = actionRange[i].Max;
        }
        shape = new int[1]{NB_ACTIONS+extraActions};
        isContinuous = true;
        rangeMin = actionRangeMin;
        rangeMax = actionRangeMax;
        this.agent = (RLAgent) agent;
        agent.AddResetListener(this);
        body = this.agent.GetAvatarBody() as CharacterBody3D;
        this.spaceState = body.GetWorld3D().DirectSpaceState;
        oldDir = body.Transform.Basis.Z.Normalized();
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
        float[] extraActionsOutput = null;
        if (agent != null && !agent.Done)
        {
            float[] action = agent.GetActionArgAsFloatArray();
            move_v = action[0] + (MoveNoise != 0 ? (float)GD.Randfn(0, MoveNoise): 0);
            move_h = action[1] + (MoveNoise != 0 ? (float)GD.Randfn(0, MoveNoise): 0);
            turn = action[2] + (TurnNoise != 0 ? (float)GD.Randfn(0, TurnNoise) : 0);
            jump = action[3] + (JumpNoise != 0 ? (float)GD.Randfn(0, JumpNoise) : 0);

            
            if (keepForward)
            {
                move_h = 0;
                move_v = Mathf.Clamp(move_v, 0, rangeMax[0]);
            }
            else
            {
                move_v = Mathf.Clamp(move_v, rangeMin[0], rangeMax[0]);
                move_h = Mathf.Clamp(move_h, rangeMin[1], rangeMax[1]);
            }
            turn = Mathf.Clamp(turn, rangeMin[2], rangeMax[2]);
            jump = Mathf.Clamp(jump, rangeMin[3], rangeMax[3]);
            jumpForward = Mathf.Clamp(action[4], rangeMin[4], rangeMax[4]);

            //GD.Print(move_v, " ", move_h, " ", turn, " ", jump, " ", jumpForward);

            if (extraActions > 0)
            {
                extraActionsOutput = new float[extraActions];
                for (int i = 0; i < extraActions; i++)
                {
                    extraActionsOutput[i] = action[NB_ACTIONS + i];
                }
            }
            else
            {
                extraActionsOutput = new float[0];
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

            if (Mathf.Abs(move_v) < deadZone)
            {
                move_v = 0;
            }

            if (Mathf.Abs(move_h) < deadZone)
            {
                move_h = 0;
            }

            if (jump > 0 && body.IsOnFloor())
            {
                velocity.Y = jump * jumpPower;
            }

            var direction = Vector3.Zero;

            // Add the gravity.
            if (!body.IsOnFloor())
            {
			    EmitSignal(nameof(OnMoveStep), this, direction, turn, jump, jumpForward, extraActionsOutput);
                velocity.Y -= gravity * (float)delta * 10;
            }
            else
            {
                if (turn != 0)
                {
        		    body.RotateY(-Mathf.DegToRad(turn * turnAmount));
                }

                var inputDir = new Vector2(move_h, -move_v);
             

                direction = (body.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
                EmitSignal(nameof(OnMoveStep), this, direction, turn, jump, jumpForward, extraActionsOutput);
                if (!direction.IsZeroApprox())
                {
                    velocity.X = direction.X * moveAmount;
                    velocity.Z = direction.Z * moveAmount;
                }
                else
                {
                    velocity.X = Mathf.MoveToward(velocity.X, 0, velocityDump);
                    velocity.Z = Mathf.MoveToward(velocity.Z, 0, velocityDump);
                }
            }


            body.Velocity = velocity;
            body.MoveAndSlide();
        }
        move_v = 0;
        move_h = 0;
        turn = 0;
        jump = 0;
        jumpForward = 0;
    }

    public override void OnReset(Agent agent)
    {
        turn = 0;
        move_v = 0;
        move_h = 0;
        jump = 0;
        jumpForward = 0;
    }
}
