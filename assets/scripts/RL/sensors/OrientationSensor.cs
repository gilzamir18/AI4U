using System.Collections;
using System.Collections.Generic;
using Godot;

namespace ai4u {
	public partial class OrientationSensor : Sensor
	{
		[Export]
		private NodePath target;
		private Node3D targetNode;
		[Export]
		private NodePath reference;
		private Node3D referenceNode = null;
		
		[Export]
		private float maxDistance = 100;

		private HistoryStack<float> history;

		public override void OnSetup(Agent agent) {

			this.agent = (BasicAgent)agent;

			if (reference == null) {
				referenceNode = this.agent.GetAvatarBody() as Node3D;
			} else
			{
				referenceNode = GetNode(reference) as Node3D;
			}
			
			targetNode = GetNode(target) as Node3D;

			type = SensorType.sfloatarray;
			shape = new int[1]{2};
			history = new HistoryStack<float>(shape[0]*stackedObservations);
		}

		public override void OnReset(Agent aget)
		{
			history = new HistoryStack<float>(shape[0]*stackedObservations);
		}
		
		public override float[] GetFloatArrayValue()
		{
			if (target == null){
				GD.Print("OrientationSensor error: target don't specified! Game Object: " + Name);
			}
		
			Vector3 f = referenceNode.GlobalTransform.Basis.Z;

			Vector3 d = targetNode.GlobalTransform.Origin - referenceNode.GlobalTransform.Origin;

			float c = f.Dot(d.Normalized());

			history.Push(c);
			if (normalized)
			{
				history.Push(d.Length()/maxDistance);
			} 
			else
			{
				history.Push(d.Length());
			}
			return history.Values;
		}
	}
}
