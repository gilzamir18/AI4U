using System.Collections;
using System.Collections.Generic;
using Godot;
using ai4u;

namespace  ai4u
{
	public class WASDRBMoveController : Controller
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

		override public string GetAction()
		{
			float[] actionValue = new float[4];
			string actionName = actuatorName;

			if (Input.IsKeyPressed((int)KeyList.W))
			{
				actionValue[0] = speed;
			}

			if (Input.IsKeyPressed((int)KeyList.S))
			{
				actionValue[0] = -speed;
			}

			if (Input.IsKeyPressed((int)KeyList.U))
			{
				actionValue[2] = jumpPower;
			}

			if (Input.IsKeyPressed((int)KeyList.J))
			{
				actionValue[3] = jumpPower;
			}

			if (Input.IsKeyPressed((int)KeyList.A))
			{
				actionValue[1] = -turnAmount;
			}

			if (Input.IsKeyPressed((int)KeyList.D))
			{
				actionValue[1] = turnAmount;
			}

			if (Input.IsKeyPressed((int)KeyList.R))
			{
				actionName = "__restart__";
			}

			if (actionName != "__restart__")
			{
				return ai4u.Utils.ParseAction(actionName, actionValue);
			} else
			{
				return ai4u.Utils.ParseAction("__restart__");
			}
		}

		override public void NewStateEvent()
		{
			int n = GetStateSize();
			for (int i = 0; i < n; i++)
			{
				if (GetStateName(i) == "reward" || GetStateName(i) == "score")
				{
					float r = GetStateAsFloat(i);
					reward_sum += r;
				}
				else if (GetStateName(i) == "done" && GetStateAsFloat(i) > 0)
				{
					GD.Print("Reward Episode: " + reward_sum);
					reward_sum = 0;
				}
			}
		}
	}
}
