using Godot;
using System;
using ai4u;
using ai4u.ext;

public class RLCSGBoxAgent : RLAgent
{
	private CSGBox body;
	private Vector3 initialPosition;
	public override void OnSetup()
	{
		base.OnSetup();
		body = GetParent() as CSGBox;
		initialPosition = body.Translation;
	}

	public override Node GetBody()
	{
		return body;
	}
	
	public override void HandleOnResetEvent()
	{
		base.HandleOnResetEvent();
		body.Translation = initialPosition;
	}
}
