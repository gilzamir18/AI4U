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

		private BasicAgent agent;

		public RBMoveActuator()
		{
			shape = new int[1]{4};
			isContinuous = true;
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

				RigidBody rBody = agent.GetAvatarBody() as RigidBody;
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
						if (Mathf.Abs(turn) < 0.01f)
						{
							turn = 0;
						}
						//state.transform.basis = state.transform.basis.rotated(Vector3.UP,the_rotation)
						var velocity = new Vector3(0, 0, 0);
						velocity.z += move + jumpForward;
						

						/*if (turn != 0)
						{
							PhysicsServer.BodySetState(
								rBody.GetRid(),
								PhysicsServer.BodyState.Transform,
								rBody.GlobalTransform.Rotated(rBody.GlobalTransform.basis.y, turn)
							);
						}*/
						
						//velocity.x += turn;
						
						
						var r = rBody.Transform.basis.y * turn;
						
						PhysicsServer.BodySetState(
							rBody.GetRid(),
							PhysicsServer.BodyState.AngularVelocity,
							r
						);	
						
						velocity.y += jump;
						velocity = velocity.Rotated(Vector3.Up, rBody.Rotation.y);
						rBody.ApplyCentralImpulse(velocity);
						//rBody.AddTorque(new Vector3() * 1000);
						//rBody.ApplyTorqueImpulse( rBody.GlobalTransform.basis.y * 10000);
					}
				}
				move = 0;
				turn = 0;
				jump = 0;
				jumpForward = 0;
			}
		}

		public override void OnSetup(Agent agent)
		{
			this.agent = (BasicAgent) agent;
			agent.AddResetListener(this);
			
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
