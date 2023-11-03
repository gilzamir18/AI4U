using Godot;
using System;

public partial class BoyWorldHUD : Node
{
	[Export]
	private NodePath playerPath;
	private BotController playerController;
	private RichTextLabel energyDisplay;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		energyDisplay = GetNode<RichTextLabel>("EnergyDisplay");
		playerController = GetNode<BotController>(playerPath);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		energyDisplay.Text = "Energy: " + playerController.Energy;
	}
}
