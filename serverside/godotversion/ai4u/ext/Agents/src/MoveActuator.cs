using System.Collections;
using System.Collections.Generic;
using Godot;


namespace ai4u.ext {

	public class MoveActuator : Actuator
	{
		//forces applied on the x, y and z axes.
		private float fx, fy, fz;

		[Export]
		public float speed = 1;
		
		[Export]
		public bool is2D = false;

		public override void Act()
		{
			if (agent.GetActionName()==actionName)
			{
				float[] f = agent.GetActionArgAsFloatArray();
				fx = f[0]; fy = f[1];
				
				if (is2D)
				{
					Node2D body = agent.GetBody() as Node2D;
					body.Translate(new Vector2(fx, fy));
				} else 
				{
					fz = f[2];
					Spatial sp = agent.GetBody() as Spatial;
					sp.Translation = sp.Translation + new Vector3(fx, fy, fz);
				}
			}
		}
		
		public override void OnBinding(Agent agent) 
		{
			this.agent = GetParent() as Agent;
		}

		public override void OnReset(Agent agent)
		{
			fx = 0;
			fy = 0;
			fz = 0;
		}
	}
}
