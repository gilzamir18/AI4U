using Godot;
using System;
using ai4u;

namespace ai4u.ext 
{
	public class SimpleCharacterController: Controller
	{
		[Export]
		public float speed = 1;

		[Export]
		public string actionName = "character";
		
		[Export]
		public string debugModeExclusionFilter = "";
		
		[Export]
		public float jumpPower = 10;
		
		[Export]
		public bool debugMode = true;

		private bool isActionApplied = false;

		public override void OnCreate()
		{
			this.showLastStateDescription = false;
		}

		public override void NewStateEvent()
		{

		}

		override public object[] GetAction()
		{
			string action = "";
			float[] actionValue = null;
			if (Input.IsKeyPressed((int)KeyList.W))
			{
				action = actionName;
				actionValue = new float[]{0, 0, 0, speed, 0, jumpPower};
				isActionApplied = true;
			}

			if (Input.IsKeyPressed((int)KeyList.S))
			{
				action = actionName;
				actionValue = new float[]{0, 0, speed, 0, 0, jumpPower};;
				isActionApplied = true;
			}

			if (Input.IsKeyPressed((int)KeyList.U))
			{
				action = actionName;
				actionValue = new float[]{0, 0, 0, speed, speed, jumpPower};
				isActionApplied = true;
			}

			if (Input.IsKeyPressed((int)KeyList.J))
			{
				action = actionName;
				actionValue = new float[]{0, 0, 0, 0, 0, jumpPower};
				isActionApplied = true;
			}

			if (Input.IsKeyPressed((int)KeyList.A))
			{
				action = actionName;
				actionValue = new float[]{speed, 0, 0, 0, 0, jumpPower};
				isActionApplied = true;
			}

			if (Input.IsKeyPressed((int)KeyList.D))
			{
				action = actionName;
				actionValue = new float[]{0, speed, 0, 0, 0, jumpPower};
				isActionApplied = true;
			}

			if (Input.IsKeyPressed((int)KeyList.R))
			{
				action = "restart";
				isActionApplied = true;
			}

			if (action == actionName)
			{
				return GetFloatArrayAction(action, actionValue);
			} else if (action == "restart")
			{
				return GetBoolAction("restart", true);
			} else 
			{
				return GetFloatArrayAction(actionName, new float[]{0, 0, 0, 0, 0, jumpPower});
			}
		}
		
		public override void HandleActionApplied(object[] args)
		{
			if (isActionApplied && debugMode)
			{
				for (int i = 0; i < desc.Length; i++) 
				{
					if ( !debugModeExclusionFilter.Contains(desc[i]) )
						GD.Print("States(" + desc[i] + ") = " + value[i]);		
				}
				isActionApplied = false;
			}
		}
	} 
}
