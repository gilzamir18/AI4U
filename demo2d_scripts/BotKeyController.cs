using ai4u;
using Godot;
using System;

public partial class BotKeyController: Node
{

	[Export]
	private NodePath controllerRef;

	private BotController controller;

	private AnimatedSpriteManager2D spriteManager;
    private bool forward = true;
	private bool reflect = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		controller = GetNode<BotController>(controllerRef);
		controller.State = StateEnum.nostarted;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (Input.IsKeyPressed(Key.R))
		{
            controller.Respawn();
			return;
		}
		if (controller.State == StateEnum.nostarted)
		{
			controller.Respawn();
			return;
		}
        if (Input.IsActionPressed("ui_right"))
		{
			controller.SetAction(ActionEnum.right);
		}
		else if (Input.IsActionPressed("ui_left"))
		{
			controller.SetAction(ActionEnum.left);
        }
		else if (Input.IsActionPressed("ui_up"))
		{
			controller.SetAction(ActionEnum.attack);
		}
        else if (Input.IsActionPressed("ui_select"))
        {
			controller.SetAction(ActionEnum.jump);
        }
        else
		{
			controller.SetAction(ActionEnum.idle);
        }
	}
}
