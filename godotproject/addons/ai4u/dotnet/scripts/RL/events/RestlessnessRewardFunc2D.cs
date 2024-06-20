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

    [Export(PropertyHint.Range, "-1, 1")]
    private int ignoredAxis = -1;
    
    private BasicAgent basicAgent;

    private Vector2 currentPosition;
    private Vector2 previousPosition;

    private float acmReward = 0.0f;

    public int IgnoredAxis { get => ignoredAxis; set => ignoredAxis = value; }

    public override void OnSetup(Agent agent)
    {
        if (ignoredAxis != -1 && ignoredAxis != 0 &&  ignoredAxis != 1)
        {
            GD.PrintErr("Invalid range of the property IgnoredAx is. Try: -1 (no axis ignored), 0 (axis x ignored) or 1 (axis y ignored).");
        }
        basicAgent = (BasicAgent)agent;
        basicAgent.OnStepEnd += CheckDist;
    }

    private void CheckDist(BasicAgent basicAgent)
    {
        currentPosition = ((PhysicsBody2D)this.basicAgent.GetAvatarBody()).Position;
        float dist = 0;
        if (ignoredAxis == -1)
        {
            dist = currentPosition.DistanceTo(previousPosition);
        }
        else 
        if (ignoredAxis == 0)
        {
            dist = Math.Abs(currentPosition.Y - previousPosition.Y);
        }
        else
        {
            dist = Mathf.Abs(currentPosition.X - previousPosition.X);
        }

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
