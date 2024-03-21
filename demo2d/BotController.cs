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

	public int Energy => energy;
	public int SuperPower => superPower;
	public delegate void RespawnHandler();
	public event RespawnHandler respawnEventHandler;

	[Export]
	private float jumpThreshhold = 0.25f;

	[Export]
	private int energyDecayFreq = 50;

	[Export]
	private NodePath respawnPath;
	private Transform2D reference;

	private int step = 0;
	private AnimatedSprite2D sprite;   
	private bool forward = true;
	private StateEnum state;
	private StateEnum previousState;
	private ActionEnum currentAction;
	private RigidBody2D ball;
	private CollisionShape2D collisionShape;

	private int energy = 1000;
	private int maxEnergy = 1000;
	private int IDLECounter = 0;
	private int superPower = 0;
	private int maxSuperPower = 1000;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		reference = GetNode<Node2D>(respawnPath).Transform;
		currentAction = ActionEnum.idle;
		previousState = state;
		sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		step = 0;
	}

	public bool Live
	{
		get
		{
			return State == StateEnum.live;
		}
	}

	public bool Win
	{
		get
		{
			return State == StateEnum.win;
		}
	}

	public void AddSuperPower(int power)
	{
		superPower += power;
		if (superPower > maxSuperPower)
		{
			superPower = maxSuperPower;
		}
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

	public void SetAction(int value)
	{
		if (value == 0)
		{
			currentAction = ActionEnum.idle;
		}
		else if (value == 1)
		{
			currentAction = ActionEnum.right;
		}
		else if (value == 2)
		{
			currentAction = ActionEnum.left;
		}
		else if (value == 3)
		{
			currentAction = ActionEnum.attack;
		}
		else if (value == 4)
		{
			currentAction = ActionEnum.jump;
		}
	}

	public void Rescue()
	{
		if (State == StateEnum.live)
		{
			State = StateEnum.win;
		}
	}

	public void Kill()
	{
		if (State == StateEnum.live)
		{
			State = StateEnum.killed;
		}
	}

	public void AddEnergy(int v)
	{
		energy += v;
		if (energy > maxEnergy)
		{
			energy = maxEnergy;
		}
	}

	public void Reset()
	{
		Respawn();
	}

	public void Respawn()
	{
		this.State = StateEnum.live;
		Visible = true;
		IDLECounter = 0;
		energy = 1000;
		superPower = 0;
		forward = true;
		currentAction = ActionEnum.idle;

		PhysicsServer2D.BodySetState(
			GetRid(),
			PhysicsServer2D.BodyState.Transform,
			reference
		);

		PhysicsServer2D.BodySetState(
			GetRid(),
			PhysicsServer2D.BodyState.AngularVelocity,
			new Vector3(0, 0, 0)
		);

		PhysicsServer2D.BodySetState(
			GetRid(),
			PhysicsServer2D.BodyState.LinearVelocity,
			new Vector3(0, 0, 0)
		);

		if (respawnEventHandler != null)
		{
			respawnEventHandler();
		}
	}

	private void UpdateEnergy(int q=1)
	{
		IDLECounter += 1;
		if (IDLECounter % energyDecayFreq == 0)
		{
			energy -= q;
		}
		if (IDLECounter >= 10000)
		{
			IDLECounter = 0;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		//GD.Print(LinearVelocity.Y);
		if (State == StateEnum.win)
		{
			Visible = false;
		}
		else if (State == StateEnum.killed)
		{
			if (sprite.Frame <= 4)
			{
				sprite.FlipH = !forward;
				sprite.Play("KILLED");
			}
		}
		else if (State == StateEnum.live)
		{

			if (Mathf.Abs(LinearVelocity.Y) <= jumpThreshhold)
			{
				//GD.Print("NO CHAO");

				if (sprite.Name == "Jump" && sprite.Frame < 12)
				{
					sprite.FlipH = !forward;
					if (LinearVelocity.Y <= -jumpThreshhold)
					{
						sprite.Play("JUMP_UP");
					}
					else if (LinearVelocity.Y >= jumpThreshhold)
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
						UpdateEnergy();
						break;
					case ActionEnum.left:
						forward = false;
						sprite.FlipH = !forward;
						ApplyCentralForce(new Vector2(-100000 * (float)delta, 0));
						sprite.Play("WALK");
						UpdateEnergy(2);
						break;
					case ActionEnum.right:
						forward = true;
						sprite.FlipH = !forward;
						ApplyCentralForce(new Vector2(100000 * (float)delta, 0));
						sprite.Play("WALK");
						UpdateEnergy(2);
						break;
					case ActionEnum.attack:
						sprite.FlipH = !forward;
						sprite.Play("SLIDING");
						UpdateEnergy(4);
						break;
					case ActionEnum.jump:
						sprite.FlipH = !forward;
						sprite.Play("JUMP_UP");
						UpdateEnergy(4);
						ApplyCentralImpulse(new Vector2(0, -50000.0f * (float)delta ));
						break;
				}
			}
			else
			{
				sprite.FlipH = !forward;
				if (LinearVelocity.Y <= -jumpThreshhold)
				{
					sprite.Play("JUMP_UP");
				}
				else if (LinearVelocity.Y >= jumpThreshhold)
				{
					sprite.Play("JUMP_FALL");
				}
				UpdateEnergy(2);
			}
			if (energy < 0)
			{
				energy = 0;
				State = StateEnum.killed;
			}
		}
	}
}
