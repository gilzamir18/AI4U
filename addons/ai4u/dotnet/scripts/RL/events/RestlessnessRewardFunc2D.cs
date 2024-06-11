using Godot;
using System;
using ai4u;

namespace ai4u;

public partial class RestlessnessRewardFunc2D : RewardFunc
{

    [Export]
    private float minDist = 0.001f;

    [Export]
    private float reward = -0.001f;


    [Export]
    private float increseRate = 0.005f;

    [Export]
    private float decreaseRate = -0.001f;

    
    private BasicAgent basicAgent;

    private Vector2 currentPosition;
    private Vector2 previousPosition;

    private float acmReward = 0.0f;

    public override void OnSetup(Agent agent)
    {
        basicAgent = (BasicAgent)agent;
        basicAgent.OnStepEnd += CheckDist;
    }

    private void CheckDist(BasicAgent basicAgent)
    {
        currentPosition = ((PhysicsBody2D)this.basicAgent.GetAvatarBody()).Position;
        float dist = currentPosition.DistanceTo(previousPosition);
        if (dist < minDist)
        {
            acmReward += reward;
        }
        else
        {
            previousPosition = currentPosition;
        }
    }

    public override void OnUpdate()
    {
        if (acmReward != 0)
        {
            basicAgent.AddReward(acmReward, causeEpisodeToEnd);
            acmReward = 0;
        }
    }

    public override void OnReset(Agent agent)
    {
        currentPosition = ((PhysicsBody2D)this.basicAgent.GetAvatarBody()).Position;
        previousPosition = currentPosition;
        acmReward = 0;
    }
}
