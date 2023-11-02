using ai4u;
using Godot;
using Godot.Collections;
using System;

public enum StateEnum
{
    live = 0,
    killed = 1,
    win = 2,
    timeouted,
    nostarted
}

public enum ActionEnum
{
    idle = 0,
    right = 1,
    left = 2,
    attack = 3,
    jump = 4,
    kill = 5,
}

public partial class BotController : RigidBody2D
{

    [Export]
    private float pivotHeight = .0f;
    private int step = 0;
    private AnimatedSprite2D sprite;   
    private bool forward = true;
    private StateEnum state;
    private StateEnum previousState;
    private ActionEnum currentAction;
    private RigidBody2D ball;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        currentAction = ActionEnum.idle;
        state = StateEnum.nostarted;
        previousState = state;
        sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        step = 0;
    }

    public StateEnum State
    {
        set
        {
            state = value;
            previousState = value;
            step = 0;
        }
        get
        {
            return state;
        }
    }

    public void SetAction(ActionEnum action)
    {
        currentAction = action; 
    }

    public bool IsOnTheFloor()
    {
        if (Transform.Origin.Y > 396.5)
        {
            GD.Print(this.GlobalPosition.Y);
        }
        return Position.Y <= pivotHeight;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
        GD.Print(LinearVelocity.Y);
        if (Mathf.Abs(LinearVelocity.Y) <= 0.1)
        {
            GD.Print("NO CHAO");

            if (sprite.Name == "Jump" && sprite.Frame < 12)
            {
                sprite.FlipH = !forward;
                if (LinearVelocity.Y <= 0)
                {
                    sprite.Play("JUMP_UP");
                }
                else
                {
                    sprite.Play("JUMP_FALL");
                }
            }

            switch (currentAction)
            {
                case ActionEnum.idle:
                    LinearVelocity = Vector2.Zero;
                    sprite.FlipH = !forward;
                    sprite.Play("IDLE");
                    break;
                case ActionEnum.left:
                    forward = false;
                    sprite.FlipH = !forward;
                    ApplyCentralForce(new Vector2(-1200, 0));
                    sprite.Play("WALK");
                    break;
                case ActionEnum.right:
                    forward = true;
                    sprite.FlipH = !forward;
                    ApplyCentralForce(new Vector2(1200, 0));
                    sprite.Play("WALK");
                    break;
                case ActionEnum.attack:
                    if (forward)
                    {
                        sprite.FlipH = !forward;
                        sprite.Play("SLIDING");
                        ApplyForce(new Vector2(0, 1200));
                    }
                    else
                    {
                        sprite.FlipH = forward;
                        sprite.Play("ATTACKLEFT");
                        ApplyForce(new Vector2(0, 1200));
                    }
                    break;
                case ActionEnum.jump:
                    sprite.FlipH = !forward;
                    sprite.Play("JUMP_UP");
                    ApplyCentralImpulse(new Vector2(0, -500));
                    break;
            }
        }
        else
        {
            sprite.FlipH = !forward;
            if (LinearVelocity.Y <= 0)
            {
                sprite.Play("JUMP_UP");
            }
            else
            {
                sprite.Play("JUMP_FALL");
            }
        }
    }
}
