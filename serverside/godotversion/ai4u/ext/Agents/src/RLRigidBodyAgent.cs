using Godot;
using System;

namespace ai4u.ext
{
	public class RLRigidBodyAgent : RLAgent
	{
		private RigidBody body;
		private Vector3 initialPosition;
		private Vector3 initialRotation;
		
		public override void OnSetup()
		{
			this.body = GetParent() as RigidBody;
			this.initialPosition = this.body.Translation;
			this.initialRotation = this.body.Rotation;
			base.OnSetup();
		}
		
		public override Node GetBody()
		{
			return body;
		}
		
		public override void HandleOnResetEvent()
		{
			base.HandleOnResetEvent();
			body.AngularVelocity = new Vector3();
			body.LinearVelocity = new Vector3();
			body.Rotation = this.initialRotation;
			body.Translation = this.initialPosition;
		}
	}
}
