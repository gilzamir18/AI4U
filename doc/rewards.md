# Rewards

There are two ways to add rewards for an agent. Do not try any other method if you don't want to break the AI4U logic.

The first way is by adding a reward event object as a child of the RLAgent node that represents the agent. For example, you can create a node of type TouchRewardFunc, which will produce a reward every time the agent touches the target object (called *Target*). There are several ready-made reward events in the [scripts](../addons/ai4u/dotnet/scripts/RL/events).

The second way to add rewards is through the Rewards property of the agent's *RLAgent* object. Here is an example of code that uses the Rewards property:

```c#
using Godot;
using System;
using ai4u;

public partial class GameManager : Node
{
	[Export]
	private RLAgent agent;
	[Export]
	private ArrowPhysicsMoveController2D humanController;
	[Export]
	private CBMoveActuator2D actuator;
	[Export]
	private Label labelTimer;
	[Export]
	private HSlider jumperPower;
	[Export]
	private bool randomizeGravity = false;

	private bool agentOnLeftArea = true;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (jumperPower != null && humanController != null)
		{
			humanController.jumpPower = (float)jumperPower.Value;
			jumperPower.ValueChanged += (value) => humanController.jumpPower = (float)value;
		}

		agent.OnResetStart += OnReset; //register event handler.
		agent.OnStepStart += UpdateDisplay; //register event handler.
	}

	private void UpdateDisplay(RLAgent agent)
	{
		labelTimer.Text = "Time: " + agent.NSteps;
	}

	public void OnReset(RLAgent agent)
	{
		if (randomizeGravity)
		{	
			float k = 1.0f;
			if (GD.RandRange(0, 1) == 1)
			{
				k = 0.1f;
			}
			actuator.Gravity = 980 * k;	
		}
	
		labelTimer.Text = "Time: 0";
	}

	public void OnLeftAreaBodyEntered(Node2D node)
	{
		if (node.IsInGroup("AGENT"))
		{
			if (!agentOnLeftArea)
			{
				agent.Rewards.Add(10); //add reward for updating at end of current time step.
			}
			agentOnLeftArea = true;
		}
	}

	public void OnRightAreaBodyEntered(Node2D node)
	{
		if (node.IsInGroup("AGENT"))
		{
			if (agentOnLeftArea)
			{
				agent.Rewards.Add(10); //add reward for updating at end of current time step.
			}
			agentOnLeftArea = false;
		}
	}
}
```

This complete example can be found [here](.). Note that the call *agent.Rewards.Add(10)* is adding 10 reward points to the agent. In this case, you can add any *float* value as an argument to the *Add* method. You can also pass an additional second argument, *requestDone*. This argument is of type boolean and indicates whether the episode should end after this reward (*requestDone* is *true*) or not (*requestDone* is *false*, the default value).
