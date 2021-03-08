using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace ai4u
{
    ///
    /// <summary>This class defines a remote controller for an agent of type Agent of the AI4U.
    /// Estes agentes recebem comandos de um script por meio de uma interface de comunicação em rede.
    /// So, Brain is a generic controller, awhile RemoteBrain implements an agent's network controller.
    /// </summary>
    public class RemoteBrain : Brain
    {
        private static bool clientRequestReceived = false; //This flag indicates if remote message was received.
        public int port = 8081; //defines the port on which the controller will receive remote commands.
        public int buffer_size = 8192; //number of bytes of the network buffer.
        private UdpClient udpSocket; //socket to start listening controller messages/requests.

        private UdpClient socket; //socket to response controller messages/requests. 
        
        private Socket sockToSend; //Socket to send async message.
        private IPAddress serverAddr; //controller address
        private EndPoint endPoint; //controller endpoint

        ///<summary>controller IP</summary>
        public string remoteIP = "127.0.0.1";
        
        ///<summary>controller port</summary>
        public int remotePort = 8080;

        ///<summary>updates the agent's physics even if the remote controller 
        ///has not sent any actions/messages. It's dont work very well with 
        ///reinforcement learning algorithms. So, leave this disabled for reinforcement
        /// learning.</summary>
        public bool alwaysUpdate = false;
        
        ///<summary>If true, the remote brain will be 
        ///managed manually. Thus, in this case, command 
        ///line arguments do not alter the properties of 
        ///the remote brain.</summary>
        public bool managed = false;
        
        ///<summary>If waitClientRequest is equals true, remote brain pause physical update until it receives
        ///another controller message. This is essential for converging the reinforcement learning training 
        ///algorithm, as it guarantees the properties of a Markov Decision Process.
        ///</summary>
        public bool waitClientRequest = true;
   

        private string cmdname; //It's more recently received command/action name.
        private string[] args; //It's more recently command/action arguments.
        private bool message_received = false; //flag indicating message received.
        private bool firstMsgSended = false; //it's true if received message is a first...
        private System.AsyncCallback async_call; //call back function called after message received.
        private IPEndPoint source;
        private bool runFirstTime = true; //it indicates if it's the first time that aWake method runs.
        private float timeScale = 1.0f; //unity controll of the physical time.
    
        
        void Awake(){
            //one time configuration
            if (!managed && runFirstTime){
                runFirstTime =false;
                string[] args = System.Environment.GetCommandLineArgs ();
                int i = 0;
                while (i < args.Length){
                    switch (args[i]) {
                        case "--ai4u_inputport":
                            port = int.Parse(args[i+1]);
                            i += 2;
                            break;
                        case "--ai4u_outputport":
                            remotePort = int.Parse(args[i+1]);
                            i += 2;
                            break;
                        case "--ai4u_timescale":
                            this.timeScale = float.Parse(args[i+1], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                            Time.timeScale = this.timeScale;
                            i += 2;
                            break;
                        case "--ai4u_remoteip":
                            remoteIP = args[i+1];
                            i += 2;
                            break;
                        case "--ai4u_targetframerate":
                            Application.targetFrameRate = int.Parse(args[i+1]);
                            i += 2;
                            break;
                        case "--ai4u_vsynccount":
                            QualitySettings.vSyncCount = int.Parse(args[i+1]);
                            i += 2;
                            break;
                        case "--ai4u_waitclientrequest":
                            this.waitClientRequest = bool.Parse(args[i+1]);
                            i += 2;
                            break;
                        default:
                            i+=1;
                            break;
                    }
                }
            }
        }


        // Use this for initialization
        void Start()
        {

            if (agent == null) {
                Debug.LogWarning("You have not defined the agent that the remote brain must control. Game Object: " + gameObject.name);
            }

            source = null;
            async_call = new System.AsyncCallback(ReceiveData);
            firstMsgSended = false;
            if (sockToSend == null)
            {
                sockToSend = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            }
            serverAddr = IPAddress.Parse(remoteIP);
            endPoint = new IPEndPoint(serverAddr, remotePort);
            agent.SetBrain(this);
            agent.StartData();
            clientRequestReceived = false;
            Reset();
        }

        public void Reset()
        {
            clientRequestReceived = false;
            try
            {
                IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, port);
                udpSocket = new UdpClient(remoteIpEndPoint);
                udpSocket.BeginReceive(async_call, udpSocket);
                //Debug.Log("Listening");
            }
            catch (System.Exception e)
            {
                // Something went wrong
                Debug.Log("Socket error: " + e);
            }
        }

        void OnDisable()
        {
            if (udpSocket != null)
            {
                //Debug.Log("Socket is closed...");
                udpSocket.Close();
            }
        }


        private void ApplyWaitClientRequest() {
            if (waitClientRequest){
                Time.timeScale = 0;
            }
        }

        void RemoteUpdate()
        {
            if (message_received)
            {
                this.receivedcmd = cmdname;
                this.receivedargs = args;
                agent.ApplyAction();
                if (!alwaysUpdate){
                    agent.UpdatePhysics();
                }
            }
            if (alwaysUpdate){
                agent.UpdatePhysics();
            }

            if (message_received){   
                if (!updateStateOnUpdate) {
                    message_received = false;
                 
                    agent.UpdateState();
                    agent.GetState();
                    firstMsgSended = true;
                    socket.Client.ReceiveBufferSize = buffer_size;
                    socket.BeginReceive(async_call, udpSocket);
                }
            }

            if (!updateStateOnUpdate){
                if (!firstMsgSended)
                {
                    agent.UpdateState();
                    agent.GetState();
                    firstMsgSended = true;
                }
            }
            if (!updateStateOnUpdate) {
                ApplyWaitClientRequest();
            }
        }

        void FixedUpdate()
        {
            if (fixedUpdate)
            {
                RemoteUpdate();
            }
        }

        void Update() {
            if (clientRequestReceived) {
                clientRequestReceived = false;
                Time.timeScale = this.timeScale;
            }
        }

        void LateUpdate()
        {
            if (!fixedUpdate)
            {
                RemoteUpdate();
            } else if (updateStateOnUpdate) {
                if (message_received) {
                    message_received = false;
                    agent.UpdateState();
                    agent.GetState();
                    firstMsgSended = true;
                    socket.Client.ReceiveBufferSize = buffer_size;
                    socket.BeginReceive(async_call, udpSocket);
                }

                if (!firstMsgSended)
                {
                    agent.UpdateState();
                    agent.GetState();
                    firstMsgSended = true;
                }
                ApplyWaitClientRequest();
            }
        }

        public void ReceiveData(System.IAsyncResult result)
        {
            socket = null;
            try
            {
                
                socket = result.AsyncState as UdpClient;
                if (source == null)
                    source = new IPEndPoint(0, 0);
                
                byte[] data = socket.EndReceive(result, ref source);
                socket.Client.ReceiveBufferSize = 0;
                if (data != null)
                {
                    string cmd = Encoding.UTF8.GetString(data);
                    string[] tokens = cmd.Trim().Split(';');
                    if (tokens.Length < 2)
                    {
                        throw new System.Exception("Invalid command exception: number of arguments is less then two!!!");
                    }
                    string cmdname = tokens[0].Trim();
                    int nargs = int.Parse(tokens[1].Trim());

                    string[] args = new string[nargs];

                    for (int i = 0; i < nargs; i++)
                    {
                        args[i] = tokens[i+2];
                    }
                    this.cmdname = cmdname;
                    this.args = args;
                    message_received = true;
                }
                clientRequestReceived = true;
            }
            catch (System.Exception e)
            {
                Debug.Log(e.Message);
                Debug.Log("Inexpected error: " + e.StackTrace);
            } 
        }
   
        public override void SendMessage(string[] desc, byte[] tipo, string[] valor)
        {
            SendMessageFrom(desc, tipo, valor);
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
        public void SendMessageFrom(string[] desc, byte[] tipo, string[] valor)
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
            this.sendData(b);
        }

        public void sendData(byte[] data)
        {
            sockToSend.SendTo(data, endPoint);
        }
    }
}
