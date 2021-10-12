using Godot;
using System;
using ai4u;

namespace ai4u.ext
{
	public class CharacterActuator : Actuator
	{
		private KinematicBody body;
		
		public override void Act()
		{
			if (agent.GetActionName()==actionName)
			{
				float[] f = agent.GetActionArgAsFloatArray();
				if (f[0] >= 0)
					body.Set("left_strength", f[0]);
				
				if (f[1] >= 0)
					body.Set("right_strength", f[1]);
				
				if (f[2] >= 0)
					body.Set("down_strength", f[2]);
	
				if (f[3] >= 0)
					body.Set("up_strength", f[3]);
	
				if (f[4] > 0)
				{
					body.Set("jump", true);
				}
				
				if (f[5] >= 0)
				{
					body.Set("jumpSpeed", f[5]);
				}
			}
			body.Call("process_commands", agent.DeltaTime);
			body.Call("update_physics", agent.DeltaTime);
		}
		
		public override void OnBinding(Agent agent) 
		{
			this.agent = GetParent() as Agent;
			body = this.agent.GetBody() as KinematicBody;
		}

		public override void OnReset(Agent agent)
		{
			body.Set("jump", false);
		}
	}
}
