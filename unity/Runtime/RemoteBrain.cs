using UnityEngine;
using System.Text;
using System.Net.Sockets;
using System.Net;
using UnityEditor;

namespace ai4u
{
    ///
    /// <summary>This class defines a remote controller for an agent of type Agent of the AI4U.
    /// Estes agentes recebem comandos de um script por meio de uma interface de comunicação em rede.
    /// So, Brain is a generic controller, awhile RemoteBrain implements an agent's network controller.
    /// </summary>
    public class RemoteBrain : Brain
    {   
        ///<summary>If true, the remote brain will be 
        ///managed manually. Thus, in this case, command 
        ///line arguments do not alter the properties of 
        ///the remote brain.</summary>
        public bool managed = false;
        ///<summary>The IP of the ai4u2unity training server.</summary>
        public string host = "127.0.0.1";
        ///<summary>The server port of the ai4u2unity training server.</summary>
        public int port = 8080;
        public int receiveTimeout = 2000;
        public int receiveBufferSize = 8192;
        public int sendBufferSize = 8192;


        private string cmdname; //It's more recently received command/action name.
        private string[] args; //It's more recently command/action arguments.
        private float timeScale = 1.0f; //unity controll of the physical time.
        private bool runFirstTime = false;



        private IPAddress serverAddr; //controller address
        private EndPoint endPoint; //controller endpoint
        private Socket sockToSend; //Socket to send async message.

        private ControlRequestor controlRequestor;

        void Awake(){
            if (!isEnabled)
            {
                return;
            }
            //one time configuration
            sockToSend = TrySocket();
            if (!managed && runFirstTime){
                runFirstTime =false;
                string[] args = System.Environment.GetCommandLineArgs ();
                int i = 0;
                while (i < args.Length){
                    switch (args[i]) {
                        case "--ai4u_port":
                            port = int.Parse(args[i+1]);
                            i += 2;
                            break;
                        case "--ai4u_timescale":
                            this.timeScale = float.Parse(args[i+1], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                            Time.timeScale = this.timeScale;
                            i += 2;
                            break;
                        case "--ai4u_host":
                            host = args[i+1];
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
                        default:
                            i+=1;
                            break;
                    }
                }
            }
            if (agent == null) {
                Debug.LogWarning("You have not defined the agent that the remote brain must control. Game Object: " + gameObject.name);
            }
            agent.SetBrain(this);
            agent.Setup();
            controlRequestor = agent.ControlRequestor; 
            if (controlRequestor == null)
            {

                Debug.LogWarning("No ControlRequestor component added in RemoteBrain component.");
            }
        }

        public ControlRequestor ControlRequestor
        {
            get
            {
                return controlRequestor;
            }
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

        public bool sendData(byte[] data, out int total, byte[] received)
        {
            TrySocket().ReceiveTimeout = receiveTimeout;
            TrySocket().ReceiveBufferSize = receiveBufferSize;
            TrySocket().SendBufferSize = sendBufferSize;

            sockToSend.SendTo(data, endPoint);
            total = 0;
            try 
            { 
                total = sockToSend.Receive(received);
                return true;
            }
            catch(System.Exception e)
            {
                Debug.LogWarning($"Script ai4u2unity is not connected in agent {agent.ID}! Start the ai4u2unity script! Network error: {e.Message}");
                #if UNITY_EDITOR
                    EditorApplication.isPlaying = false;
                #endif
                Application.Quit();
                return false;
            }
        }
    }
}
