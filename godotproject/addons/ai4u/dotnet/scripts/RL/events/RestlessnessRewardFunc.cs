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

    private float restlessness = 0.0f;

    public float Restlessness => restlessness;

    private BasicAgent basicAgent;

    private Vector3 currentPosition;
    private Vector3 previousPosition;

    private float acmReward = 0.0f;

    public override void OnSetup(Agent agent)
    {
        basicAgent = (BasicAgent) agent;
        basicAgent.OnStepEnd += CheckDist;
    }

    private void CheckDist(BasicAgent basicAgent)
    {
        currentPosition = ((PhysicsBody3D)this.basicAgent.GetAvatarBody()).Position;
        float dist = currentPosition.DistanceTo(previousPosition);
        if (dist > minDist)
        {
            restlessness += increseRate;
            previousPosition = currentPosition;
            acmReward += reward;
        }
        else
        {
            restlessness -= decreaseRate;
        }
    }

    public override void OnUpdate()
    {
        if (acmReward != 0)
        {
            basicAgent.AddReward(acmReward, causeEpisodeToEnd);
        }
    }

    public override void OnReset(Agent agent)
    {
        currentPosition = ((PhysicsBody3D)this.basicAgent.GetAvatarBody()).Position;
        previousPosition = currentPosition;
        restlessness = 0.0f;
    }
}
