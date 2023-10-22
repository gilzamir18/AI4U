using ai4u;
using Godot;
using System;

public partial class Dog : RigidBody2D
{
	private AnimatedSpriteManager2D spriteManager;
    private bool forward = true;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		spriteManager = GetNode<AnimatedSpriteManager2D>("Sprite2DManager");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (Input.IsActionPressed("ui_right"))
		{
			/*_spriteWalk.Visible = true;
			_spriteJump.Visible = false;
			_spriteWalk.FlipH = false;
			_spriteWalk.Play("WALK");*/
			spriteManager.UpdateCurrentSprite("Walk");
			spriteManager.Current.FlipH = false;
			spriteManager.Play("WALK");
			ApplyForce(new Vector2(1200,0));
			forward = true;
		}
		else if (Input.IsActionPressed("ui_left"))
		{
            /*_spriteWalk.Visible = true;
			_spriteJump.Visible = false;
            _spriteWalk.FlipH = true;
            _spriteWalk.Play("WALK");*/
            spriteManager.UpdateCurrentSprite("Walk");
            spriteManager.Current.FlipH = true;
			spriteManager.Play("WALK");
            ApplyForce(new Vector2(-1200, 0));
            forward = false;
        }
		else if (Input.IsActionPressed("ui_up"))
		{

			spriteManager.UpdateCurrentSprite("Attack");
			spriteManager.Current.FlipH = !forward;
			spriteManager.Play("ATTACK");
			ApplyForce(new Vector2(0, 1000));
		}
        else if (Input.IsActionPressed("ui_select"))
        {
			/*_spriteJump.FlipH = !forward;
            _spriteWalk.Visible = false;
			_spriteJump.Visible = true;
            _spriteJump.Play("JUMP");*/
			spriteManager.UpdateCurrentSprite("Jump");
			spriteManager.Current.FlipH = !forward;
			spriteManager.Play("JUMP");
            ApplyForce(new Vector2(0, 1000));
        }
        else
		{
            LinearVelocity = Vector2.Zero;
            spriteManager.UpdateCurrentSprite("IDLE");
            spriteManager.Current.FlipH = !forward;
            spriteManager.Play("IDLE");
        }
	}
}
