using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

namespace ai4u
{

    public struct Command  
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
        ///<summary>The IP of the ai4u2unity training server.</summary>
        public string host = "127.0.0.1";
        
        ///<summary>The server port of the ai4u2unity training server.</summary>
        public int port = 8080;

        public int receiveTimeout = 10;

        ///<summary>If autoMode is True, ControlRequestor runs itself according to 'Skip Frame' and 'Repeat Action' attributes.</summary>
        public bool autoMode = true;

        public int skipFrame = 0;
        public bool repeatAction = false;

        private IPAddress serverAddr; //controller address
        private EndPoint endPoint; //controller endpoint
        private Socket sockToSend; //Socket to send async message.
        private int frameCounter = -1;
        private Agent agent;
        private bool stoped = false;

        public void SetAgent(Agent agent)
        {
            this.agent = agent;
        }

        // Start is called before the first frame update
        void Awake()
        {
            frameCounter = -1;
            sockToSend = TrySocket();
        }

        public Socket TrySocket()
        {
            if (sockToSend == null)
            {
                    serverAddr = IPAddress.Parse(host);
                    endPoint = new IPEndPoint(serverAddr, port);
                    sockToSend = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            }
            return sockToSend;
        }

        void OnDisable()
        {
            if (sockToSend != null)
            {
                //Debug.Log("Socket is closed...");
                sockToSend.Close();
            }
        }



        public Command[] RequestEnvControl(RequestCommand request)
        {
            string cmdstr = null;
            if (agent.Brain is LocalBrain)
            {
                cmdstr = ((LocalBrain) (agent.Brain)).SendMessage(request.Command, request.Type, request.Value);
            }
            else
            {
                cmdstr = SendMessageFrom(request.Command, request.Type, request.Value);
            }

            if (cmdstr != null)
            {
                Command[] cmds = UpdateActionData(cmdstr);
                return cmds;
            }
            return null;
        }


        public Command[] RequestControl()
        {
            agent.UpdateState();
            string cmdstr = null;
            if (agent.Brain is LocalBrain)
            {
                cmdstr = ((LocalBrain) (agent.Brain)).SendMessage(agent.MessageID, agent.MessageType, agent.MessageValue);
            }
            else
            {
                cmdstr = SendMessageFrom(agent.MessageID, agent.MessageType, agent.MessageValue);
            }

            if (cmdstr != null)
            {
                Command[] cmds = UpdateActionData(cmdstr);
                foreach(Command cmd in cmds)
                {
                    agent.Brain.SetReceivedCommandName(cmd.name);
                    agent.Brain.SetReceivedCommandArgs(cmd.args);
                    agent.ApplyAction();
                }
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
            if (agent != null && autoMode && !stoped && agent.SetupIsDone)
            {
                if (agent == null)
                {
                    Debug.LogWarning("ControlRequest requires an Agent! Use the method 'SetAgent' of the ControlRequest" 
                                      + " component to set an agent!");
                }
                if (frameCounter < 0)
                {
                    agent.Reset();
                }
                if (frameCounter <= 0)
                {
                    bool palive = agent.Alive();

                    var cmd = RequestControl();

                    if (palive  && !agent.Alive())
                    {
                        agent.EndOfEpisode();
                    }
                    if (CheckCmd(cmd, "__stop__"))
                    {
                        stoped = true;
                        frameCounter = -1;
                        agent.NSteps = 0;
                        agent.Reset();
                    }
                    else if (CheckCmd(cmd, "__restart__"))
                    {
                        frameCounter = -1;
                        agent.NSteps = 0;
                        agent.Reset();
                    }
                    else
                    {
                        agent.NSteps = agent.NSteps + 1;
                        frameCounter = 1;
                    }
                }
                else
                {
                    if (repeatAction)
                    {
                        agent.ApplyAction();
                    }
                    frameCounter ++;
                    if (frameCounter >= skipFrame)
                    {
                        frameCounter = 0;
                    }
                }
            } else
            {
                RequestCommand request = new RequestCommand(3);
                request.SetMessage(0, "__target__", ai4u.Brain.STR, "envcontrol");
                request.SetMessage(1, "wait_command", ai4u.Brain.STR, "restart");
                request.SetMessage(2, "id", ai4u.Brain.STR, agent.ID);
            
                var cmds = RequestEnvControl(request);
                if (cmds == null)
                {
                    throw new System.Exception("ai4u2unity connection error!");
                }

                if (CheckCmd(cmds, "__restart__"))
                {
                    frameCounter = -1;
                    agent.NSteps = 0;
                    stoped = false;
                    agent.Reset();
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
        public string SendMessageFrom(string[] desc, byte[] tipo, string[] valor)
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
            if (this.sendData(b, out total, received))
            {
                return Encoding.UTF8.GetString(received);
            }
            return null;
        }

        public bool sendData(byte[] data, out int total, byte[] received)
        {
            TrySocket().ReceiveTimeout = receiveTimeout;
            sockToSend.SendTo(data, endPoint);
            total = 0;
            try 
            {
                total = sockToSend.Receive(received);
                return true;
            }
            catch(System.Exception e)
            {
                Debug.LogWarning("Script ai4u2unity is not connected! Start the ai4u2unity script! Network error: " + e.Message);
                 #if UNITY_EDITOR
                EditorApplication.isPlaying = false;
                #endif
                return false;
            }
        }
    }
}