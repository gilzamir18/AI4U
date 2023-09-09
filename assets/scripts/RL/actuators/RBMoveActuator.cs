using System.Collections;
using System.Collections.Generic;
using Godot;


namespace ai4u {
	public partial class RBMoveActuator : Actuator
	{
		//forces applied on the x, y and z axes.    
		private float move, turn, jump, jumpForward;
		[Export]
		public float moveAmount = 1;
		[Export]
		public float turnAmount = 1;
		[Export]
		public float jumpPower = 1;
		[Export]
		public float jumpForwardPower = 1;

		[Export]
		public float minActivityThreshold = 0.001f;

		private BasicAgent agent;
		
		private RigidBody3D rBody;

		public RBMoveActuator()
		{

		}

		public override void OnSetup(Agent agent)
		{
			shape = new int[1]{4};
			isContinuous = true;
			rangeMin = new float[]{0, -1, 0, 0};
			rangeMax = new float[]{1, 1, 1, 1};
			this.agent = (BasicAgent) agent;
			agent.AddResetListener(this);
			rBody = this.agent.GetAvatarBody() as RigidBody3D;
		}


		private bool onGround = false;

		public bool OnGround
		{
			get
			{
				return onGround;
			}
		}

		public override void Act()
		{
			if (agent != null && !agent.Done)
			{
				float[] action = agent.GetActionArgAsFloatArray();
				move = action[0];
				turn = action[1];
				jump = action[2];
				jumpForward = action[3];

				if (rBody != null)
				{
					if (Mathf.Abs(rBody.LinearVelocity.Y) > 0.001)
					{
						onGround = false;
					}
					else
					{
						onGround = true;
					}
					if (onGround)
					{	
						if (Mathf.Abs(turn) < minActivityThreshold)
						{
							turn = 0;
						}
						
						if (Mathf.Abs(jump) < minActivityThreshold)
						{
							jump = 0;
						}
						
						if (Mathf.Abs(jumpForward) < minActivityThreshold)
						{
							jumpForward = 0;
						}
						
						var velocity = new Vector3(0, 0, 0);
						velocity.Z += move * moveAmount + jumpForward * jumpForwardPower;
						
						var r = rBody.Transform.Basis.Y * turn;
						
						PhysicsServer3D.BodySetState(
							rBody.GetRid(),
							PhysicsServer3D.BodyState.AngularVelocity,
							r
						);
						
						velocity.Y += jump * jumpPower + jumpForward * jumpPower;
						velocity = velocity.Rotated(Vector3.Up, rBody.Rotation.Y);
						//rBody.AddConstantCentralForce(velocity);
						PhysicsServer3D.BodySetState(
							rBody.GetRid(),
							PhysicsServer3D.BodyState.LinearVelocity,
							velocity
						);
					}
				}
			}
			move = 0;
			turn = 0;
			jump = 0;
			jumpForward = 0;
		}

		public override void OnReset(Agent agent)
		{
			turn = 0;
			move = 0;
			jump = 0;
			jumpForward = 0;
			onGround = false;
		}
	}
}
