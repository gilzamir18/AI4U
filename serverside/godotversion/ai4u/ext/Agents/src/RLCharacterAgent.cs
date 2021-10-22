using Godot;
using System;

namespace ai4u.ext
{
	public class RLCharacterAgent : RLAgent
	{
		[Export]
		public bool globalPositionSensor = false;
		
		private KinematicBody body;
		private Vector3 initialPosition, initialRotation;
		private KinematicBodyOnFloorSensor onFloorSensor;
		private PositionSensor positionSensor;
		
		public override void OnSetup()
		{
			body = GetParent()  as KinematicBody;
			initialPosition = body.Translation;
			initialRotation = body.Rotation;
			base.OnSetup();
			onFloorSensor = new KinematicBodyOnFloorSensor();
			onFloorSensor.perceptionKey = "is_on_floor";
			onFloorSensor.type = SensorType.sbool;
			onFloorSensor.shape = new int[]{};
			onFloorSensor.isState = true;
			onFloorSensor.resettable = true;
			onFloorSensor.OnBinding(this);
			AddSensor(onFloorSensor);
			
			positionSensor = new PositionSensor();
			positionSensor.perceptionKey = "my_position";
			positionSensor.type = SensorType.sfloatarray;
			positionSensor.shape = new int[]{3};
			positionSensor.isState = true;
			positionSensor.isGlobal = this.globalPositionSensor;
			positionSensor.resettable = true;
			positionSensor.OnBinding(this);
			AddSensor(positionSensor);
		}

		public override Node GetBody()
		{
			return body;
		}
		
		public override void HandleOnResetEvent()
		{
			base.HandleOnResetEvent();
			body.Translation = this.initialPosition;
			body.Rotation = this.initialRotation;
			body.Call("reload");
		}
		
		public override void HandleOnDone() 
		{
			base.HandleOnDone();
			body.Call("set_done_true");
			body.Call("reload");
		}
	}
}
