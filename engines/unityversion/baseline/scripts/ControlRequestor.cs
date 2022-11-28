using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

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

    public class ControlRequestor : MonoBehaviour
    {
        public float defaultTimeScale = 1.0f; 

        private SortedList<string, Agent> agents;

        private bool initialized = false;

        public void SetAgent(Agent agent)
        {
            if (! initialized)
            {
                Initialize();
                initialized = true;
            }
            string pkey = agent.ID;
            this.agents.Add(pkey, agent);
            agent.ControlInfo = new AgentControlInfo();
        }

        // Start is called before the first frame update
        void Initialize()
        {
            this.agents = new SortedList<string, Agent>();
            Time.timeScale = defaultTimeScale;
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
            }
            else
            {

                cmdstr = SendMessageFrom((RemoteBrain)agent.Brain, agent.MessageID, agent.MessageType, agent.MessageValue);
            }

            if (cmdstr != null)
            {
                Dictionary<string, string[]> fields = new Dictionary<string, string[]>();
                Command[] cmds = UpdateActionData(cmdstr);
                foreach(Command cmd in cmds)
                {
                    agent.Brain.SetReceivedCommandName(cmd.name);
                    agent.Brain.SetReceivedCommandArgs(cmd.args);
                    fields[cmd.name] = cmd.args; 
                }
                agent.Brain.SetCommandFields(fields);
                return cmds;
            }

            return null;
        }

        private Command[] UpdateActionData(string cmd)
        {   
            string[] cmdTokens = cmd.Trim().Split('@');
            int nCmds = cmdTokens.Length;
            Command[] res = new Command[nCmds];
            int c = 0;
            foreach(string cmdToken in cmdTokens)
            { 
                string[] tokens = cmdToken.Trim().Split(';');
                if (tokens.Length < 2)
                {
                    string msg = "Invalid command exception: number of arguments is less then two : " + cmdToken;
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
        

        void FixedUpdate()
        {
            foreach(var entry in agents)
            {
                AgentUpdate(entry.Value);
            }
        }

        private void AgentUpdate(Agent agent)
        {
            if (!agent.SetupIsDone)
            {
                return;
            }
            AgentControlInfo ctrl = agent.ControlInfo;

            if (agent != null && !ctrl.stopped && !ctrl.paused)
            {

                if (agent == null)
                {
                    Debug.LogWarning("ControlRequest requires an Agent! Use the method 'SetAgent' of the ControlRequest" 
                                      + " component to set an agent!");
                }
                if (!ctrl.applyingAction)
                {
                    var cmd = RequestControl(agent);
                    
                    if (CheckCmd(cmd, "__stop__"))
                    {
                        ctrl.stopped = true;
                        ctrl.applyingAction = false;
                        ctrl.frameCounter = 0;
                        agent.NSteps = 0;
                        agent.Reset();
                    }
                    else if (CheckCmd(cmd, "__restart__"))
                    {
                        ctrl.frameCounter = 0;
                        agent.NSteps = 0;
                        ctrl.applyingAction = false;
                        ctrl.paused = false;
                        ctrl.stopped = false;
                        agent.Reset();
                    }
                    else if (CheckCmd(cmd, "__pause__"))
                    {
                        ctrl.applyingAction = false;
                        ctrl.paused = true;
                    }
                    else if (!CheckCmd(cmd, "__waitnewaction__"))
                    {
                        ctrl.applyingAction = true;
                        ctrl.frameCounter = 1;
                        ((BasicAgent)agent).ResetReward();
                        agent.ApplyAction();
                        if (!agent.Alive())
                        {
                            ctrl.applyingAction = false;
                            ((BasicAgent)agent).UpdateReward();
                            ctrl.lastCmd = RequestControl(agent);
                            ctrl.stopped = true;
                            ctrl.frameCounter = 0;
                            agent.NSteps = 0;
                        }
                    }
                }
                else
                {
                    if (ctrl.frameCounter >= agent.Brain.skipFrame)
                    {
                        ((BasicAgent)agent).UpdateReward();
                        ctrl.frameCounter = 0;
                        ctrl.applyingAction = false;
                        agent.NSteps = agent.NSteps + 1;
                    }
                    else
                    {
                        if (agent.Brain.repeatAction)
                        {
                            agent.ApplyAction();
                        } 
                        
                        if (!agent.Alive())
                        {
                            ctrl.applyingAction = false;
                            ((BasicAgent)agent).UpdateReward();
                            ctrl.lastCmd = RequestControl(agent);
                            ctrl.stopped = true;
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
                Command[] cmds = null;
                if (ctrl.lastCmd != null)
                {
                    cmds = ctrl.lastCmd;
                    ctrl.lastCmd = null;
                }
                else
                {
                    RequestCommand request = new RequestCommand(3);
                    request.SetMessage(0, "__target__", ai4u.Brain.STR, "envcontrol");
                    request.SetMessage(1, "wait_command", ai4u.Brain.STR, "restart, resume");
                    request.SetMessage(2, "id", ai4u.Brain.STR, agent.ID);
                
                    cmds = RequestEnvControl(agent, request);
                }

                if (cmds == null)
                {
                    throw new System.Exception("ai4u2unity connection error!");
                }

                if (CheckCmd(cmds, "__restart__"))
                {
                    ctrl.frameCounter = -1;
                    agent.NSteps = 0;
                    Dictionary<string, string[]> fields = new Dictionary<string, string[]>();
                    for (int i = 0; i < cmds.Length; i++)
                    {
                        fields[cmds[i].name] = cmds[i].args;
                    }
                    agent.Brain.SetCommandFields(fields);
                    ctrl.paused = false;
                    ctrl.stopped = false;
                    ctrl.applyingAction = false;
                    agent.Reset();
                } else if (ctrl.paused && CheckCmd(cmds, "__resume__"))
                {
                    ctrl.paused = false;
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
        /// e value is the value of the information sent.
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
