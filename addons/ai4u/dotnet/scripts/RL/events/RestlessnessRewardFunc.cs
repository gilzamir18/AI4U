using Godot;
using System;
using ai4u;

namespace ai4u;

public partial class RestlessnessRewardFunc : RewardFunc
{

	[Export]
	private float minDist = 0.001f;

	[Export]
	private float reward = -0.001f;


	[Export]
	private float increseRate = 0.005f;
	
	[Export]
	private float decreaseRate = -0.001f;

    public override void OnSetup(Agent agent)
    {
        base.OnSetup(agent);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
    }

    public override void OnReset(Agent agent)
    {
        base.OnReset(agent);
    }
}
