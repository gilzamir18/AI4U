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
		private Node target;
		
		public override float[] GetFloatArrayValue() {
			if (this.is2D) {
				Node2D nd = (Node2D) target;
				return new float[]{nd.Position.x, nd.Position.y};
			} else {
				Spatial sp = (Spatial) target;
				return new float[]{sp.Translation.x, 
									sp.Translation.y,
									sp.Translation.z};
			}
		}

		public override void OnBinding(Agent agent) 
		{
			this.agent = GetParent() as Agent;
			
			if (this.targetPath != "") {
				this.target = GetNode(this.targetPath);
			} else {
				this.target = agent.GetBody();
			}
		}
	}
}
