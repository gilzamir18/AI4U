using Godot;
using System;
using ai4u;

public class CSGBoxAgent : Agent
{
	private CSGBox target;
	
	public override void OnSetup()
	{
		base.OnSetup();
		target = GetParent() as CSGBox;
	}
	
	public override Node GetBody(){
		return target;
	}
}
