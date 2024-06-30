using System.Collections;
using System.Collections.Generic;
using Godot;
using ai4u;
using System.Diagnostics;
using System;

namespace  ai4u
{
	/// <summary>
	/// WASD controller for RigidBody3D game objects.
	/// </summary>
	[Obsolete("This controller is deprecated. Use ArrowPhysicsMoveController instead.")]
	public partial class WASDRBMoveController : Controller
	{
		[Export]
		public string actuatorName = "move";
		[Export]
		public float speed = 0.5f;
		[Export]
		public float turnAmount = 1.0f;
		[Export]
		public float jumpPower = 1.0f;
		
		private float reward_sum = 0;
		
		private bool done = false;

		override public void OnSetup()
		{
		
		}
		
		override public void OnReset(Agent agent)
		{
			done = false;
		}

		override public string GetAction()
		{
			float[] actionValue = new float[4];
			string actionName = actuatorName;
			if (agent.Alive())
			{
				if (Input.IsKeyPressed(Key.W))
				{
					actionValue[0] = speed;
				}

				if (Input.IsKeyPressed(Key.S))
				{
					actionValue[0] = -speed;
				}

				if (Input.IsKeyPressed(Key.U))
				{
					actionValue[2] = jumpPower;
				}

				if (Input.IsKeyPressed(Key.J))
				{
					actionValue[3] = jumpPower;
				}

				if (Input.IsKeyPressed(Key.A))
				{
					actionValue[1] = -turnAmount;
				}

				if (Input.IsKeyPressed(Key.D))
				{
					actionValue[1] = turnAmount;
				}

				if (Input.IsKeyPressed(Key.R))
				{
					actionName = "__restart__";
				}

				if (actionName != "__restart__")
				{
					return ai4u.Utils.ParseAction(actionName, actionValue);
				}
			}
			else
			{
				if (Input.IsKeyPressed(Key.R))
				{
					actionName = "__restart__";
				}
			}
			
			if (actionName == "__restart__")
			{
					return ai4u.Utils.ParseAction("__restart__");
			}
			else
			{
				return ai4u.Utils.ParseAction("__waitnewaction__");
			}
		}

		override public void NewStateEvent()
		{
			if (!agent.Alive() && !done)
			{
				GD.Print("Episode Reward " + ((RLAgent)agent).EpisodeReward);
				done = true;
			}
		}
	}
}
