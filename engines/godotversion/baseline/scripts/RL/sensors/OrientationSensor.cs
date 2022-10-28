using System.Collections;
using System.Collections.Generic;
using Godot;

namespace ai4u {
	public class OrientationSensor : Sensor
	{
		[Export]
		private NodePath target;
		private Spatial targetNode;
		[Export]
		private NodePath reference;
		private Spatial referenceNode = null;
		
		[Export]
		private float maxDistance = 100;

		private HistoryStack<float> history;

		public override void OnSetup(Agent agent) {

			this.agent = (BasicAgent)agent;

			if (reference == null) {
				referenceNode = this.agent.GetAvatarBody() as Spatial;
			} else
			{
				referenceNode = GetNode(reference) as Spatial;
			}
			
			targetNode = GetNode(target) as Spatial;

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
		
			Vector3 f = referenceNode.GlobalTransform.basis.z;
			//Debug.Log("f = " + f.x  + ", " + f.y + ", " + f.z);
			Vector3 d = targetNode.GlobalTransform.origin - referenceNode.GlobalTransform.origin;
			//Debug.Log("d = " + d.x + ", " + d.y + ", " + d.z);
			float c = f.Dot(d.Normalized());
			//Debug.Log("c == " + c);
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
