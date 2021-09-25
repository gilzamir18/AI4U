using Godot;
using System;

namespace ai4u.ext
{
	public class RigidBodyAgent : Agent
	{
		private RigidBody body;
		
		public override void OnSetup()
		{
			body = GetParent() as RigidBody;
		}
		
		public override Node GetBody()
		{
			return body;
		}
	}
}
