using System.Collections;
using System.Collections.Generic;
using Godot;


namespace ai4u {
	public class RBMoveActuator : Actuator
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
		
		private RigidBody rBody;

		public RBMoveActuator()
		{
			shape = new int[1]{4};
			isContinuous = true;
		}

		public override void OnSetup(Agent agent)
		{
			this.agent = (BasicAgent) agent;
			agent.AddResetListener(this);
			rBody = this.agent.GetAvatarBody() as RigidBody;
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
					if (Mathf.Abs(rBody.LinearVelocity.y) > 0.001)
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
						velocity.z += move * moveAmount + jumpForward * jumpForwardPower;
						
						var r = rBody.Transform.basis.y * turn;
						
						PhysicsServer.BodySetState(
							rBody.GetRid(),
							PhysicsServer.BodyState.AngularVelocity,
							r
						);
						
						velocity.y += jump * jumpPower + jumpForward * jumpPower;
						velocity = velocity.Rotated(Vector3.Up, rBody.Rotation.y);
						rBody.AddCentralForce(velocity);
					}
				}
				move = 0;
				turn = 0;
				jump = 0;
				jumpForward = 0;
			}
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
