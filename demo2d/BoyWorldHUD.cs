using Godot;
using System;

public partial class BoyWorldHUD : Node
{
	[Export]
	private NodePath playerPath;
	[Export]
	private NodePath helicopterPath;
	private HeliController heliController;
	private BotController playerController;
	private RichTextLabel energyDisplay;
    private RichTextLabel powerDisplay;
	private Label resultDisplay;
	private Label waitingDisplay;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		energyDisplay = GetNode<RichTextLabel>("EnergyDisplay");
        powerDisplay = GetNode<RichTextLabel>("PowerDisplay");
        playerController = GetNode<BotController>(playerPath);
		heliController = GetNode<HeliController>(helicopterPath);
		resultDisplay = GetNode<Label>("ResultDisplay");
		waitingDisplay = GetNode<Label>("WaitingDisplay");

		playerController.respawnEventHandler += Respawn;
	}

	public void Respawn()
	{
		energyDisplay.Text = "";
		powerDisplay.Text = "";
		resultDisplay.Text = "";
		waitingDisplay.Text = "";
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (playerController.State == StateEnum.live)
		{
			energyDisplay.Text = "" + playerController.Energy;
			powerDisplay.Text = "" + playerController.SuperPower;
			if (heliController.Waiting)
			{
				waitingDisplay.Text = "Go to the helicopter position!";
			}
		}
		else if (playerController.State == StateEnum.win)
		{
			resultDisplay.Text = "You Win!!!";
		}
		else if (playerController.State == StateEnum.killed)
		{
			resultDisplay.Text = "Game over!!";
		}
	}
}
