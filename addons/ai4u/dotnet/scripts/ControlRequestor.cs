using System.Collections;
using System.Collections.Generic;
using Godot;
using System.Text;
using System;
using GodotPlugins.Game;

namespace ai4u
{
	public class Command  
	{
		public string name;
		public string[] args;
		public Command(string n, string[] args)
		{
			this.name = n;
			this.args = args;
		}
	}

	internal  class AgentComparer : IComparer<Agent>
	{
		public int Compare(Agent x, Agent y)
		{
			if (x.priority > y.priority)
			{
				return -1;
			} else if (x.priority < y.priority)
			{
				return 1;
			}
			else
			{
				return 0;
			}
		}
	}

	public partial class ControlRequestor : Node
	{
		[Export] private float defaultTimeScale = 1.0f; 
		[Export] private bool physicsMode = true;
		[Export] private bool computePhysicsTicks = true;
		[Export] int maxPhysicsFrames = 60;
		[Export] public Godot.Collections.Array<Agent> agentsList { get; set; }
		
		private List<Agent> agents;
		
		public override void _Ready()
		{
			agents = new List<Agent>();
			foreach (var agent in agentsList)
			{
				agents.Add(agent);
			}

			var parentNode = GetParent();
			if (parentNode != null && parentNode is RLAgent)
			{
				agents.Add((RLAgent)parentNode);
			}

            agents.Sort( new AgentComparer() );
			for (int i = 0; i < agents.Count; i++)
			{
				Agent a = agents[i];
				a.ControlInfo = new AgentControlInfo();
				a.SetupAgent(this);
			}

			Engine.TimeScale = defaultTimeScale;
			if (computePhysicsTicks)
			{
				Engine.PhysicsTicksPerSecond = Mathf.Max(maxPhysicsFrames, Mathf.RoundToInt(defaultTimeScale * maxPhysicsFrames));
            }
        }


        public override void _Notification(int what)
        {
			if (what == NotificationWMCloseRequest)
			{
                foreach (var agent in agents)
                {
                    RequestCommand request = new RequestCommand(3);
                    request.SetMessage(0, "__target__", ai4u.Brain.STR, "envcontrol");
                    request.SetMessage(1, "__stop__", ai4u.Brain.STR, "stop");
                    request.SetMessage(2, "id", ai4u.Brain.STR, agent.ID);
                    var cmds = RequestEnvControl(agent, request);
                }
            }
        }

		public Command[] RequestEnvControl(Agent agent, RequestCommand request)
		{
			string cmdstr = null;
			if (agent.Brain is LocalBrain)
			{
				cmdstr = ((LocalBrain) (agent.Brain)).SendMessage(request.Command, request.Type, request.Value);
			}
			else
			{
				cmdstr = SendMessageFrom((RemoteBrain)agent.Brain, request.Command, request.Type, request.Value);
			}
			
			if (cmdstr != null)
			{
				Command[] cmds = UpdateActionData(cmdstr);
				return cmds;
			}
			
			return null;
		}


		public Command[] RequestControl(Agent agent)
		{
			agent.UpdateState();
			string cmdstr = null;
			if (agent.Brain is LocalBrain)
			{
				cmdstr = ((LocalBrain) (agent.Brain)).SendMessage(agent.MessageID, agent.MessageType, agent.MessageValue);
				agent.ResetCommandBuffer();
			}
			else
			{

				cmdstr = SendMessageFrom((RemoteBrain)agent.Brain, agent.MessageID, agent.MessageType, agent.MessageValue);
				agent.ResetCommandBuffer();
			}

			if (cmdstr != null)
			{
				Dictionary<string, string[]> fields = new Dictionary<string, string[]>();
				Command[] cmds = UpdateActionData(cmdstr);
				if (cmds.Length > 0)
				{
					agent.Brain.SetReceivedCommandName(cmds[0].name);
					agent.Brain.SetReceivedCommandArgs(cmds[0].args);
				}
				for (int i = 1; i < cmds.Length; i++)
				{
					fields[cmds[i].name] = cmds[i].args; 
				}
				agent.Brain.SetCommandFields(fields);
				return cmds;
			}

			return null;
		}

		private Command[] UpdateActionData(string cmd)
		{   

			if (cmd == "halt")
			{
                throw new System.Exception("System halted by client!");
            }
			string[] cmdTokens = cmd.Trim().Split('@');
			int nCmds = cmdTokens.Length;
			Command[] res = new Command[nCmds];
			int c = 0;
			foreach(string cmdToken in cmdTokens)
			{ 
				string[] tokens = cmdToken.Trim().Split(';');
				if (tokens.Length < 2)
				{
					string msg = "Invalid command exception: number of arguments is less then two : " + cmd;
					throw new System.Exception(msg);
				}
				
				string cmdname = tokens[0].Trim();
				
				int nargs = int.Parse(tokens[1].Trim());
				string[] args = new string[nargs];

				for (int i = 0; i < nargs; i++)
				{
					args[i] = tokens[i+2];
				}

				res[c] = new Command(cmdname, args);
				c++;
			}
			return res;
		}

		private bool CheckCmd(Command[] cmds, string cmd)
		{
			if (cmds != null && cmds.Length > 0)
			{
				return cmds[0].name == cmd;
			}
			return false;
		}


		public override void _PhysicsProcess(double delta)
		{
			if (physicsMode)
			{
				AI4UUpdate(delta);
			}
		}

		public override void _Process(double delta)
		{
			if (!physicsMode)
			{
				AI4UUpdate(delta);
			}
		}

		void AI4UUpdate(double delta)
		{
			foreach(var agent in agents)
			{
				AgentUpdate(agent, delta);
			}
		}

		private string last_cmd = "";
		private void AgentUpdate(Agent agent, double delta)
		{
			if (agent == null || !agent.SetupIsDone)
			{
				if (agent == null)
				{
					GD.Print("ControlRequest update loop called with null agent!");
				}
				return;
			}
			AgentControlInfo ctrl = agent.ControlInfo;
			ctrl.deltaTime = delta;
			if (!ctrl.envmode)
			{
				if (!ctrl.applyingAction)
				{
					var cmd = RequestControl(agent);
					if (CheckCmd(cmd, "__stop__") && !ctrl.stopped)
					{
						ctrl.stopped = true;
						ctrl.applyingAction = false;
						ctrl.frameCounter = 0;
						agent.NSteps = 0;
					}
					else if (CheckCmd(cmd, "__restart__") || CheckCmd(cmd, "__reset__"))
					{
						if (cmd[0].args.Length > 0)
						{
							ctrl.lastResetId = cmd[0].args[0];
						}
						ctrl.frameCounter = 0;
						agent.NSteps = 0;
						ctrl.applyingAction = false;
						ctrl.paused = false;
						ctrl.stopped = false;
						agent.AgentReset();
					}
					else if (CheckCmd(cmd, "__pause__"))
					{
						ctrl.applyingAction = false;
						ctrl.paused = true;
					}
					else if (CheckCmd(cmd, "__resume__"))
					{
						ctrl.paused = false;
					}
					else if (CheckCmd(cmd, "__envcontrol__"))
					{
						ctrl.envmode = true;
						ctrl.paused = true;
					}
					else if (!CheckCmd(cmd, "__waitnewaction__") && !(ctrl.paused || ctrl.stopped))
					{
						ctrl.applyingAction = true;
						ctrl.frameCounter = 1;
						agent.ResetReward();
						agent.BeginOfStep();
						agent.ApplyAction();
						if (!agent.Alive())
						{
							agent.EndOfEpisode();
							ctrl.stopped = true;
							ctrl.applyingAction = false;
							agent.UpdateReward();
							ctrl.paused = false;
							ctrl.frameCounter = 0;
							agent.NSteps = 0;
						}
					}
				}
				else if (!ctrl.stopped && !ctrl.paused)
				{
					if (ctrl.frameCounter >= ctrl.skipFrame)
					{
						agent.UpdateReward();
						ctrl.frameCounter = 0;
						ctrl.applyingAction = false;
						agent.NSteps = agent.NSteps + 1;
						if (!agent.Alive())
						{
							agent.EndOfEpisode();
						}
					}
					else
					{
						if (ctrl.repeatAction)
						{
							agent.ApplyAction();
						} 
						
						if (!agent.Alive())
						{
							agent.EndOfEpisode();
							ctrl.stopped = true;
							ctrl.applyingAction = false;
							agent.UpdateReward();
							ctrl.paused = false;
							ctrl.frameCounter = 0;
							agent.NSteps = 0;
						}
						else
						{                        
							ctrl.frameCounter ++;
						}
					}
				}
			} else
			{
				RequestCommand request = new RequestCommand(4);
				request.SetMessage(0, "__target__", ai4u.Brain.STR, "envcontrol");
				request.SetMessage(1, "wait_command", ai4u.Brain.STR, "restart, resume, start_and_reset, start");
				request.SetMessage(2, "id", ai4u.Brain.STR, agent.ID);
                request.SetMessage(3, last_cmd, ai4u.Brain.STR, last_cmd);
				last_cmd = "";
                var cmds = RequestEnvControl(agent, request);
				if (cmds == null)
				{
					GD.PrintErr($"AI4U connection error! Agent ID: {agent.ID}.");
                    agent.GetTree().Quit();
                }
				if (CheckCmd(cmds, "__restart__")) //obsolete
				{
					last_cmd = "__restart__";
                    ctrl.frameCounter = -1;
					agent.NSteps = 0;
					Dictionary<string, string[]> fields = new Dictionary<string, string[]>();
					for (int i = 0; i < cmds.Length; i++)
					{
						fields[cmds[i].name] = cmds[i].args;
					}
					agent.Brain.SetCommandFields(fields);
					if (cmds[0].args.Length > 0)
					{
							ctrl.lastResetId = cmds[0].args[0];
					}
					ctrl.paused = false;
					ctrl.stopped = false;
					ctrl.applyingAction = false;
					ctrl.envmode = false;
					agent.AgentStart();
				}
				else if (CheckCmd(cmds, "__start_agent__"))
				{
					last_cmd = "__start_agent__";
                    ctrl.frameCounter = -1;
					agent.NSteps = 0;
					Dictionary<string, string[]> fields = new Dictionary<string, string[]>();
					for (int i = 0; i < cmds.Length; i++)
					{
						fields[cmds[i].name] = cmds[i].args;
					}
					agent.Brain.SetCommandFields(fields);
					if (cmds[0].args.Length > 0)
					{
							ctrl.lastResetId = cmds[0].args[0];
					}
					ctrl.paused = false;
					ctrl.stopped = false;
					ctrl.applyingAction = false;
					ctrl.envmode = false;
					agent.AgentStart();
				}
				else if (CheckCmd(cmds, "__reset__"))
				{
					last_cmd = "__reset__";
                    ctrl.frameCounter = 0;
					agent.NSteps = 0;
					Dictionary<string, string[]> fields = new Dictionary<string, string[]>();
					for (int i = 0; i < cmds.Length; i++)
					{
						fields[cmds[i].name] = cmds[i].args;
					}
					agent.Brain.SetCommandFields(fields);
					if (cmds[0].args.Length > 0)
					{
							ctrl.lastResetId = cmds[0].args[0];
					}
					ctrl.paused = false;
					ctrl.stopped = false;
					ctrl.applyingAction = false;
					ctrl.envmode = false;
					agent.AgentStart();
					agent.AgentReset();
				}
			}
		}

		/// <summary>
		/// Sends a message to the customer in the following format:
		/// [numberofields] [[descsize] [desc] [type] [valorsize] [value]] +
		/// where desc is a description of the message, type is the type of the message given as an integer such that:
		/// 0 = float
		/// 1 = int
		/// 2 = boolean
		/// 3 = string
		/// 4 = byte array
		/// 5 = float array
		/// 6 = int array
		/// 7 = string array
		/// @return the value of the information sent.
		/// </summary>
		public string SendMessageFrom(RemoteBrain rbrain, string[] desc, byte[] tipo, string[] valor)
		{
			StringBuilder sb = new StringBuilder();
			int numberoffields = desc.Length;
			sb.Append(numberoffields.ToString().PadLeft(4,' ').PadRight(4, ' '));

			for (int i = 0; i < desc.Length; i++)
			{
				StringBuilder field = new StringBuilder();
				int descsize = Encoding.UTF8.GetByteCount(desc[i]);
				field.Append(descsize.ToString().PadLeft(4, ' ').PadRight(4,' '));
				field.Append(desc[i]);
				field.Append((int)tipo[i]);
				field.Append(Encoding.UTF8.GetByteCount(valor[i]).ToString().PadLeft(8, ' ').PadRight(8, ' '));
				field.Append(valor[i]);
				string fstr = field.ToString();
				sb.Append(fstr);
			}
			byte[] b = Encoding.UTF8.GetBytes(sb.ToString());
			byte[] received = new byte[1000];
			int total = 0;
			if (rbrain.sendData(b, out total, received))
			{
				return Encoding.UTF8.GetString(received);
			}
			return null;
		}
	}
}
