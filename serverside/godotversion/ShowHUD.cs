using Godot;
using System;
using ai4u.ext.npcs;
using ai4u.ext;
using ai4u;

public class ShowHUD : Node, ISensorListener, IAgentResetListener
{
	
	[Export]
	public NodePath energySensor;
	[Export]
	public NodePath stepsLabelPath;
	public Label stepsLabel;

	[Export]
	public NodePath energyLabelOutput;
	public Label energyLabel;

	[Export]
	public NodePath rewardLabelPath;
	public Label rewardLabel;
	
	[Export]
	public NodePath agentPath;
	private RLAgent agent;
	
	private int count = 0;

	private EnergySensor sensor;
	
	private bool registered = false;
	
	public override void _Ready()
	{
		energyLabel = GetNode(energyLabelOutput) as Label;
		stepsLabel = GetNode(stepsLabelPath) as Label;
		rewardLabel = GetNode(rewardLabelPath) as Label;
		
		agent = GetNode(agentPath) as RLAgent;
		
		sensor = GetNode(energySensor) as EnergySensor;
		sensor.Subscribe(this);
		
		energyLabel.Text = sensor.CurrentEnergy+"";	
		stepsLabel.Text = "Elapsed Time: " + agent.CurrentStep;
		rewardLabel.Text = "Reward: " + 0;
		
		agent.AddResetListener(this);
	}
	
	public override void _Process(float delta)
	{
		if (!agent.Done)
		{			
			if (count > 5) 
			{
				count = 0;
				stepsLabel.Text = "Elapsed Time: " + (agent.maxSteps - agent.CurrentStep);
			} 
			else
			{
				count++;
			}
		}
		if (!registered)
		{
			if (agent.RewardSensor != null)
			{
				agent.RewardSensor.Subscribe(this);
				registered = true;
			}
		}
	}
	
	public void OnReset(Agent agent)
	{
		count = 0;
		stepsLabel.Text = "Elapsed Time: " + this.agent.CurrentStep;
		rewardLabel.Text = "Reward: " + 0;
	}
	
	public void OnSensor(Sensor s)
	{
		if (sensor == s && !agent.Done)
		{
			energyLabel.Text = sensor.CurrentEnergy+"";
		} else if (agent.RewardSensor == s)
		{
			rewardLabel.Text = "Reward: " + agent.Reward;
		}
	}
}
