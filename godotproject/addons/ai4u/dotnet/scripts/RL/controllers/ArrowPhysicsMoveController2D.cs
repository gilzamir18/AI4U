using System.Collections;
using System.Collections.Generic;
using Godot;
using ai4u;

namespace  ai4u
{
	/// <summary>
	/// WASD controller for physics body (rigid or character) game objects.
	/// </summary>
	public partial class ArrowPhysicsMoveController2D : Controller
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
		
		private bool _firstTime = true;

		private float[] actionValue = new float[4];
		private string actionName = "__waitnewaction__";

		private bool receivedResetAction = false;

		override public void OnSetup()
		{
			((BasicAgent) agent).OnEpisodeEnd += EndEpisodeHandler;
			_firstTime = true;
			receivedResetAction = false;
		}

        public override void _Process(double delta)
        {
            if (agent != null && agent.SetupIsDone && !receivedResetAction)
			{
				actionValue = new float[4];
				if (agent.Alive())
				{
					actionName = "move";
					if (Input.IsActionPressed("ui_up"))
					{
						actionName = "move";
						actionValue[2] = jumpPower;
					}

					if (Input.IsActionPressed("ui_select"))
					{
						actionName = "move";
						actionValue[2] = jumpPower;
					}

					if (Input.IsKeyPressed(Key.W))
					{
						actionName = "move";
						actionValue[3] = jumpPower;
					}

					if (Input.IsKeyPressed(Key.A))
					{
						actionName = "move";
						actionValue[1] = -turnAmount;
					}

					if (Input.IsKeyPressed(Key.D))
					{
						actionName = "move";
						actionValue[1] = turnAmount;
					}

					if (Input.IsActionPressed("ui_left"))
					{
						actionName = "move";
						actionValue[0] = -speed;
					}

					if (Input.IsActionPressed("ui_right"))
					{
						actionName = "move";
						actionValue[0] = speed;
					}

					if (Input.IsActionJustPressed("ui_cancel") )
					{
						actionName = "__reset__";
						receivedResetAction = true;
					}
				}
				else
				{
					actionName = "__waitnewaction__";
					if (_firstTime)
					{
						actionName = "__reset__";
						_firstTime = false;
					}
					else
					{
						if (Input.IsActionJustPressed("ui_cancel") )
						{
							receivedResetAction = true;
							actionName = "__reset__";
						}
					}
				}
			}
        }

        override public string GetAction()
		{
			var action = ai4u.Utils.ParseAction(actionName, actionValue);
			if (receivedResetAction)
			{
				receivedResetAction = false;
			}
			return action;
		}

		public void EndEpisodeHandler(BasicAgent agent)
		{
			GD.Print("Episode Reward " + ((BasicAgent)agent).EpisodeReward);
		}
	}
}
