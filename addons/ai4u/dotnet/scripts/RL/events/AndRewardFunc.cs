using ai4u;
using Godot;
using System;

public partial class AndRewardFunc : RewardFunc
{   
	[Export]
	private float reward = 1.0f;
	[Export]
	private bool updateChildren = false;
	
	
	private RLAgent agent;
	private System.Collections.Generic.List<RewardFunc> rewards;
	
	private float acmReward = 0.0f;

	public override void OnSetup(Agent agent)
	{
		acmReward = 0.0f;
		rewards = new System.Collections.Generic.List<RewardFunc> ();
		agent.AddResetListener(this);
		this.agent = (RLAgent)agent;
		var children = GetChildren();
		for (int i = 0; i < children.Count; i++)
		{
			var r = children[i] as RewardFunc;
			rewards.Add(r);
			r.OnSetup(agent);
		}
	}

	public override void OnUpdate()
	{
		if (Eval())
		{
			agent.AddReward(acmReward, causeEpisodeToEnd);
		}
		acmReward = 0.0f;
	}

	public override bool Eval()
	{
		bool pass = true;
		for (int i = 0; i < rewards.Count; i++)
		{
			if (updateChildren)
			{
				rewards[i].OnUpdate();
			}
			if (!rewards[i].Eval())
			{
				pass = false;
			}
			rewards[i].ResetEval();
		}
		if (pass)
		{
			acmReward += reward;
			return true;
		}
		else
		{
			return false;
		}
	}

	public override void ResetEval()
	{
	}

	public override void OnReset(Agent agent)
	{
		acmReward = 0;
	}
}
