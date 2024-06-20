using Godot;
using System;

public partial class AgentBody : CharacterBody2D
{
	[Export]
	private float speed = 300.0f;
	
	[Export]
	private float turnAmount = 100;

	private int action = -1;

	public void SetAction(int newAction)
	{
		action = newAction;
	}

	public void Reset()
	{
		action = -1;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;



		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		if (action <= 0)
		{
			MoveAndSlide();
			return;
		}


		Vector2 direction = Vector2.Zero;
		if (action == 1)
		{
			direction = GlobalTransform.X.Normalized();
		}
		else if (action == 2)
		{
			direction = -GlobalTransform.X.Normalized();
		}
		else if (action == 3)
		{
			Rotate(Mathf.DegToRad(turnAmount));
			// Get the input direction and handle the movement/deceleration.
            direction = GlobalTransform.X.Normalized();
		}


		if (direction != Vector2.Zero)
		{
			velocity = direction * speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
			velocity.Y = Mathf.MoveToward(Velocity.Y, 0, speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
