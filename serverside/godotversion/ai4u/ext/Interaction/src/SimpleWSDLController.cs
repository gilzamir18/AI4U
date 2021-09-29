using Godot;
using System;
using ai4u;

namespace ai4u.ext 
{
	public class SimpleWSDLController: Controller
	{
		[Export]
		public float speed = 1;

		[Export]
		public string actionName = "move";

		override public object[] GetAction()
		{
			string action = "";
			float[] actionValue = new float[3];
			if (Input.IsKeyPressed((int)KeyList.W))
			{
				action = actionName;
				actionValue[2] += -speed;
			}

			if (Input.IsKeyPressed((int)KeyList.S))
			{
				action = actionName;
				actionValue[2] += speed;
			}

			if (Input.IsKeyPressed((int)KeyList.U))
			{
				action = actionName;
				actionValue[1] += speed;
			}

			if (Input.IsKeyPressed((int)KeyList.J))
			{
				action = actionName;
				actionValue[1] += -speed;
			}

			if (Input.IsKeyPressed((int)KeyList.A))
			{
				action = actionName;
				actionValue[0] += -speed;
			}

			if (Input.IsKeyPressed((int)KeyList.D))
			{
				action = actionName;
				actionValue[0] += speed;
			}

			if (Input.IsKeyPressed((int)KeyList.R))
			{
				action = "restart";
			}

			if (action == actionName)
			{
				return GetFloatArrayAction(action, actionValue);
			} else if (action == "restart")
			{
				return GetBoolAction("restart", true);
			} else 
			{
				return GetStringAction("noop", "nooperation");
			}
		}
	} 
}
