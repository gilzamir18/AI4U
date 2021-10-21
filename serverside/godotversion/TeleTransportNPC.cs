using Godot;
using System;
using ai4u;
using ai4u.ext;

public class TeleTransportNPC : RewardFunc
{
	[Export]
	public NodePath rewardEventPath;
	private RewardFunc rewardEvent;
	
	private KinematicBody body; 
	
	public override void OnCreate()
	{
		body = agent.GetBody() as KinematicBody;
		rewardEvent = GetNode(rewardEventPath) as RewardFunc;
		rewardEvent.Subscribe(this);
	}
	
	public override void OnNotificationFrom(RewardFunc r, float v)
	{
			body.Translation = new Vector3(2.432f, -0.848f, 2.531f);
	}
}
