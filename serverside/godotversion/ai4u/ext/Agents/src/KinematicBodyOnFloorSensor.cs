using Godot;
using System;
using ai4u;

namespace ai4u.ext
{
	public class KinematicBodyOnFloorSensor : Sensor
	{
			private KinematicBody target;

			public override bool GetBoolValue() {
				if (target != null)
					return target.IsOnFloor();
				else
					return false;
			}

			public override void OnBinding(Agent agent) 
			{
				this.agent = agent;
				this.target = this.agent.GetBody() as KinematicBody;
			}
	}
}
