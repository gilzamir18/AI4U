using System.Collections;
using System.Collections.Generic;
using Godot;
using ai4u;
using System;

namespace  ai4u
{

	/// <summary>
	/// WASD controller for physics body (rigid or character) game objects.
	/// </summary>
	public partial class ArrowPhysicsMoveController : Controller
	{
		[Export]
		public string actuatorName = "move";
		[Export]
		public float speed = 0.5f;
		[Export]
		public float turnAmount = 1.0f;
		[Export]
		public float jumpPower = 1.0f;

		[Export]
		public int numberOfExtraActions = 0;

		[Export]
		private Godot.Collections.Dictionary<string, string> actionMap;

		[Export]
        private Godot.Collections.Dictionary<string, string> extraActionsIndex;

        [Export]
        private Godot.Collections.Dictionary<string, string> extraActionsValue;


        private float reward_sum = 0;
		
		private bool _firstTime = true;

		private float[] actionValue;
		private string actionName = "__waitnewaction__";
		
		private bool receivedResetAction = false;

		private int nbActions = 5;

		private const int NB_FIXED_ACTIONS = 5;
		
		private Godot.Collections.Dictionary<string, string> internalActionMap;

        public override void _Ready()
        {

			internalActionMap = new Godot.Collections.Dictionary<string, string>();
			internalActionMap["forward"] = "ui_up";
			internalActionMap["backward"] = "ui_down";
			internalActionMap["left"] = "ui_left";
			internalActionMap["right"] = "ui_right";
			internalActionMap["turn_left"] = "ui_page_up";
			internalActionMap["turn_right"] = "ui_page_down";
			internalActionMap["jump"] = "ui_select";
			internalActionMap["jump_forward"] = "ui_home";
			internalActionMap["reset"] = "ui_cancel";
 
            if (actionMap != null && actionMap.Count > 0)
            {
				foreach(var k in actionMap.Keys)
				{
					internalActionMap[k] = actionMap[k];
				}
            }

			nbActions = NB_FIXED_ACTIONS + numberOfExtraActions;
			actionValue = new float[nbActions];
		}

        override public void OnSetup()
		{
            nbActions = NB_FIXED_ACTIONS + numberOfExtraActions;
            actionValue = new float[nbActions];
            ((RLAgent) agent).OnEpisodeEnd += EndEpisodeHandler;
			_firstTime = true;
			receivedResetAction = false;
		}

        public override void _Process(double delta)
        {
            if (agent != null && agent.SetupIsDone && !receivedResetAction)
			{
				actionValue = new float[nbActions];
				if (agent.Alive())
				{
					actionName = "move";
					if (Input.IsActionPressed(internalActionMap["forward"]))
					{
						actionName = "move";
						actionValue[0] = speed;
					}

					if (Input.IsActionPressed(internalActionMap["backward"]))
					{
						actionName = "move";
						actionValue[0] = -speed;
					}
					
					if (Input.IsActionPressed(internalActionMap["left"]))
					{
						actionName = "move";
						actionValue[1] = -speed;
					}

					if (Input.IsActionPressed(internalActionMap["right"]))
					{
						actionName = "move";
						actionValue[1] = speed;
					}

					if (Input.IsActionPressed(internalActionMap["turn_left"]))
					{
						actionName = "move";
						actionValue[2] = -turnAmount;
					}

					if (Input.IsActionPressed(internalActionMap["turn_right"]))
					{
						actionName = "move";
						actionValue[2] = turnAmount;
					}

					if (Input.IsActionPressed(internalActionMap["jump"]))
					{
						actionName = "move";
						actionValue[3] = jumpPower;
					}

					if (Input.IsActionPressed(internalActionMap["jump_forward"]))
					{
						actionName = "move";
						actionValue[4] = jumpPower;
					}

					if (Input.IsActionJustPressed(internalActionMap["reset"]) )
					{
						actionName = "__reset__";
						receivedResetAction = true;
					}

				
					if (numberOfExtraActions > 0)
					{
						foreach (var k in extraActionsIndex.Keys)
						{
							if (Input.IsActionPressed(k))
							{
                                actionName = "move";
								var i = extraActionsIndex[k];
								actionValue[NB_FIXED_ACTIONS + int.Parse(i)] = float.Parse(extraActionsValue[k]);
							}
						}
					}
				}
				else
				{
					actionName = "__waitnewaction__";
					if (_firstTime)
                    {
                        _firstTime = false;
                        actionName = "__reset__";
					}
					else
					{
						if (Input.IsActionJustPressed(internalActionMap["reset"]) )
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
			actionName = "__waitnewaction__";
            if (receivedResetAction)
			{
				receivedResetAction = false;
			}
			return action;
		}

		public void EndEpisodeHandler(RLAgent agent)
		{
			GD.Print("Episode Reward " + ((RLAgent)agent).EpisodeReward);
		}
	}
}
