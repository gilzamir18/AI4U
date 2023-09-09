using System.Collections;
using System.Collections.Generic;
using Godot;
using ai4u;

namespace  ai4u
{
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
		
		private bool hasNewAction = false;

		override public void OnSetup()
		{
		
		}
		
		override public void OnReset(Agent agent)
		{
			hasNewAction = false;
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
					hasNewAction = true;
				}

				if (Input.IsKeyPressed(Key.S))
				{
					actionValue[0] = -speed;
					hasNewAction = true;
				}

				if (Input.IsKeyPressed(Key.U))
				{
					actionValue[2] = jumpPower;
					hasNewAction = true;
				}

				if (Input.IsKeyPressed(Key.J))
				{
					actionValue[3] = jumpPower;
					hasNewAction = true;
				}

				if (Input.IsKeyPressed(Key.A))
				{
					actionValue[1] = -turnAmount;
					hasNewAction = true;
				}

				if (Input.IsKeyPressed(Key.D))
				{
					actionValue[1] = turnAmount;
					hasNewAction = true;
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
			if (hasNewAction)
			{
				int n = GetStateSize();
				for (int i = 0; i < n; i++)
				{
					if (GetStateName(i) == "reward" || GetStateName(i) == "score")
					{
						float r = GetStateAsFloat(i);
						reward_sum += r;
					}
					if (GetStateName(i) == "done")
					{
						GD.Print("Reward Episode: " + reward_sum);
						reward_sum = 0;
					}
				}
			}
		}
	}
}
