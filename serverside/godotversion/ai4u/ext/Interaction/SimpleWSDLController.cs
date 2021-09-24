using Godot;
using System;
using ai4u;

namespace ai4u.ext 
{
	public class SimpleWSDLController: Controller
	{
		[Export]
		public float speed = 1.0f;

		[Export]
		public string actionName = "move";
		
		[Export]
		public bool showLastStateDescription = false;

		override public object[] GetAction()
		{
			float[] actionValue = new float[3];
			if (Input.IsKeyPressed((int)KeyList.W))
			{
				actionValue[2] += -speed;
			}

			if (Input.IsKeyPressed((int)KeyList.S))
			{
				actionValue[2] += speed;
			}

			if (Input.IsKeyPressed((int)KeyList.U))
			{
				actionValue[1] += speed;
			}

			if (Input.IsKeyPressed((int)KeyList.J))
			{
				actionValue[1] += -speed;
			}

			if (Input.IsKeyPressed((int)KeyList.A))
			{
				actionValue[0] += -speed;
			}

			if (Input.IsKeyPressed((int)KeyList.D))
			{
				actionValue[0] += speed;
			}

			if (Input.IsKeyPressed((int)KeyList.R))
			{
				actionName = "restart";
			}

			if (actionName != "restart")
			{
				return GetFloatArrayAction(actionName, actionValue);
			} else
			{
				return GetBoolAction("restart", true);
			}
		}

		override public void NewStateEvent()
		{
			if (showLastStateDescription)
			for (int i = 0; i < desc.Length; i++) {
				GD.Print("States(" + desc[i] + ") = " + value[i]);		
			}
		}
	} 
}
