using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityStandardAssets.Characters.ThirdPerson;
using UnityEngine.SceneManagement;
namespace unityremote
{

    public class Remote : MonoBehaviour
    {

        public static byte FLOAT = 0;
        public static byte INT = 1;
        public static byte BOOL = 2;
        public static byte STR = 3;
        public static byte OTHER = 4;

        public int port = 8081;
        private UdpClient udpSocket;

        private static UdpClient socket;
        private static bool commandReceived;

        private static Socket sock;
        private static IPAddress serverAddr;
        private static EndPoint endPoint;

        public string remoteIP = "127.0.0.1";
        public int remotePort = 8080;

        private static Remote instance;
       
        private System.Collections.Queue queue = new System.Collections.Queue();

        public GameObject player;

        // Use this for initialization
        void Start()
        {
            commandReceived = false;
            if (sock == null)
            {
                sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            }
            serverAddr = IPAddress.Parse(remoteIP);
            endPoint = new IPEndPoint(serverAddr, remotePort);
            Reset();
            player.SendMessage("SetRemote", this);
        }

        public void Reset()
        {
            try
            {
                instance = this;
                udpSocket = new UdpClient(port);
                udpSocket.BeginReceive(new System.AsyncCallback(ReceiveData), udpSocket);

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

        private void Update()
        {

            if (this.queue.Count > 0)
            {
                object[] cmd = (object[])this.queue.Dequeue();
                string cmdname = (string)cmd[0];
                string[] args = (string[])cmd[1];
                if (player != null)
                    if (args.Length > 0)
                        player.SendMessage(cmdname, args);
                    else
                        player.SendMessage(cmdname);
            }
        }

        public static void ReceiveData(System.IAsyncResult result)
        {
            socket = null;
            try
            {
                socket = result.AsyncState as UdpClient;
                IPEndPoint source = new IPEndPoint(0, 0);
                
                byte[] data = socket.EndReceive(result, ref source);
               
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

                    instance.queue.Enqueue(new object[] { cmdname, args});
                    
                    socket.BeginReceive(new System.AsyncCallback(ReceiveData), instance.udpSocket);
                }
            }
            catch (System.Exception e)
            {
                Debug.Log("Inexpected error: " + e.Message);
            }
        }

        public void SendMessage(string[] desc, byte[] tipo, string[] valor)
        {
            Remote.SendMessageFrom(desc, tipo, valor);
        }

        /// <summary>
        /// Envia uma mensagem para o cliente com o seguinte formato:
        ///     [numberofields][[descsize][desc][tipo][valorsize][valor]]+
        ///     onde desc é uma descrição da mensagem, tipo é o tipo da mensagem dado como um inteiro tal que:
        ///         0 = float
        ///         1 = int
        ///         2 = boolean
        ///         3 = string
        ///         4 = array de bytes
        ///    e valor é o valor da informacao enviada.
        /// </summary>
        public static void SendMessageFrom(string[] desc, byte[] tipo, string[] valor)
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
                field.Append(Encoding.UTF8.GetByteCount(valor[i].ToString()).ToString().PadLeft(8, ' ').PadRight(8, ' '));
                field.Append(valor[i]);
                string fstr = field.ToString();
                //int c = Encoding.UTF8.GetByteCount(fstr);                
                sb.Append(fstr);
            }
            byte[] b = Encoding.UTF8.GetBytes(sb.ToString());
            sendData(b);
        }

        public static void sendData(byte[] data)
        {
            sock.SendTo(data, endPoint);
        }
    }
}
