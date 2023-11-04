using Godot;
using System;

public partial class HeliController : Node2D
{
	public bool Waiting => waiting;

	private AnimationPlayer player;
	private AnimatedSprite2D animatedSprite;

	[Export]
	private NodePath playerPath;
	[Export]
	private NodePath targetPath;

	private BotController playerController;
	private Node2D target;
	private int duration = 5000;
	private int currentTime = 0;

	private bool started = false;
	private bool waiting = false;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		player = GetNode<AnimationPlayer>("AnimationPlayer");
		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		playerController = GetNode<BotController>(playerPath);
		playerController.respawnEventHandler += Respawn;
		target = GetNode<Node2D>(targetPath);
	}

	private void Respawn()
	{
		player.Stop();
		currentTime = 0;
		started = false;
		waiting = false;
    }

    public override void _PhysicsProcess(double delta)
    {
		if (playerController.State == StateEnum.live)
		{
			currentTime += 1;
			if (currentTime > duration && !started)
			{
				started = true;
				player.Play("rescue", 0.1);
			}
		}
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
		if (playerController.State == StateEnum.win)
		{
			animatedSprite.Play("win");
		}
		else
		{
			animatedSprite.Play("default");
		}
		if (playerController.State == StateEnum.live)
		{
			//GD.Print(GlobalPosition.DistanceTo(target.Position));
			if (GlobalPosition.DistanceTo(target.Position) < 80)
			{
				waiting = true;
			}

			if (waiting && Mathf.Abs(playerController.GlobalPosition.X - GlobalPosition.X) < 50)
			{
				playerController.Rescue();
			}
		}
	}
}
