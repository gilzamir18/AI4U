using Godot;
using System;

namespace ai4u.ext 
{
	public class PositionSensor : Sensor
	{
		[Export]
		public NodePath targetPath ;
		
		[Export]
		public bool is2D = false;
		[Export]
		public bool isGlobal = false;
		private Node target;
		
		public override float[] GetFloatArrayValue() {
			if (this.is2D) {
				Node2D nd = (Node2D) target;
				if (!isGlobal)
					return new float[]{nd.Position.x, nd.Position.y};
				else 
					return new float[]{nd.Transform.origin.x, nd.Transform.origin.y};
			} else {
				Spatial sp = (Spatial) target;
				if (!isGlobal)
					return new float[]{sp.Translation.x, 
										sp.Translation.y,
										sp.Translation.z};
				else
					return new float[]{sp.Transform.origin.x, 
										sp.Transform.origin.y,
										sp.Transform.origin.z};
			}
		}

		public override void OnBinding(Agent agent) 
		{
			this.agent = agent;
			if (this.targetPath != null && this.targetPath != "") {
				this.target = GetNode(this.targetPath);
			} else {
				this.target = agent.GetBody();
			}
		}
	}
}
