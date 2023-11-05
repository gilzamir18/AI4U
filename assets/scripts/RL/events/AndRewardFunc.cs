using ai4u;
using Godot;
using System;

public partial class AndRewardFunc : RewardFunc
{   
    [Export]
    private float reward = 1.0f;
    [Export]
    private bool updateChildren = false;
    
    
    private BasicAgent agent;
    private System.Collections.Generic.List<RewardFunc> rewards;
    private bool eval = false;

    public override void OnSetup(Agent agent)
    {
        eval = false;
        rewards = new System.Collections.Generic.List<RewardFunc> ();
        agent.AddResetListener(this);
        this.agent = (BasicAgent)agent;
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
            GD.Print("PASS OK");
            agent.AddReward(reward, causeEpisodeToEnd);
            eval = true;
        }
    }

    public override bool Eval()
    {
        return eval;
    }

    public override void ResetEval()
    {
        eval = false;
    }

    public override void OnReset(Agent agent)
    {
        eval = false;
    }
}
