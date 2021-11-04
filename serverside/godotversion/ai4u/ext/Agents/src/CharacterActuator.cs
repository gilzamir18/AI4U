using Godot;
using System;
using ai4u;

namespace ai4u.ext
{
	public class CharacterActuator : Actuator
	{
		private KinematicBody body;
		private bool done = false;
		
		public override void Act()
		{
			if (!done && agent.GetActionName()==actionName)
			{
				float value = 0;
				float[] f = agent.GetActionArgAsFloatArray();
				if (f[0] >= 0)
				{
					body.Set("left_strength", f[0]);
					value -= f[0];
				}
				
				if (f[1] >= 0)
				{
					body.Set("right_strength", f[1]);
					value -= f[1];
				}
				
				if (f[2] >= 0)
				{
					body.Set("down_strength", f[2]);
					value -= f[2];
				}
				
				if (f[3] >= 0)
				{
					body.Set("up_strength", f[3]);
					value -= f[3];
				}
				if (f[4] > 0)
				{
					body.Set("jump", true);
				}
				
				if (f[5] >= 0)
				{
					body.Set("jumpSpeed", f[5]);
					if (f[4] > 0)
					{
						value -= f[5];
					}
				}
				
				ActionValue = value;
			}
			body.Call("process_commands", agent.DeltaTime);
			body.Call("update_physics", agent.DeltaTime);
		}
		
		public override void OnBinding(Agent agent) 
		{
			this.agent = GetParent() as Agent;
			body = this.agent.GetBody() as KinematicBody;
			done = false;
		}
		
		public override void OnDone()
		{
			done = true;
			body.Set("left_strength", 0);
			body.Set("right_strength", 0);
			body.Set("down_strength", 0);
			body.Set("up_strength", 0);
			body.Set("jump", false);
			body.Set("jumpSpeed", 0);	
		}

		public override void OnReset(Agent agent)
		{
			body.Call("reload");
			body.Call("clear_parent");
			body.Call("rotateCharacter", new Vector3(0, 0, 1), 0);
			done = false;
		}
	}
}
