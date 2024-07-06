using Godot;
using System.Text;
using System.Net.Sockets;
using System.Net;

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
		public bool Managed {get; set;} = false;
		///<summary>The IP of the ai4upy training server.</summary>
		public string Host {get; set;} = "127.0.0.1";
		///<summary>The server port of the ai4upy training server.</summary>
		public int Port {get; set;} = 8080;
		public int ReceiveTimeout {get; set;} = 2000;
		public int ReceiveBufferSize {get; set;} = 81920;
		public int SendBufferSize {get; set;} = 81920;
		
		private bool runFirstTime = false;

		private IPAddress serverAddr; //controller address
		private EndPoint endPoint; //controller endpoint
		private Socket sockToSend; //Socket to send async message.

		public override void Setup(Agent agent){
			this.agent = agent;
			//one time configuration
			sockToSend = TrySocket();
            if (!Managed && !runFirstTime){
				runFirstTime = true;
				string[] args = OS.GetCmdlineArgs();
				int i = 0;
				while (i < args.Length)
				{
					switch (args[i])
					{
						case "--ai4u_port":
							Port = int.Parse(args[i+1]);
							i += 2;
							break;
						case "--ai4u_host":
							Host = args[i+1];
                            i += 2;
							break;
						case "--ai4u_rid":
							agent.ID = args[i + 1];
							i += 2;
							break;
						default:
							i+=1;
							break;
					}
				}
			}
		}

		public Socket TrySocket()
		{
			if (sockToSend == null)
			{
					serverAddr = IPAddress.Parse(Host);
					endPoint = new IPEndPoint(serverAddr, Port);
					sockToSend = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			}
			return sockToSend;
		}

		public override void Close()
		{
			if (sockToSend != null)
			{
				//Debug.Log("Socket is closed...");
				sockToSend.Close();
			}
		}

		public bool sendData(byte[] data, out int total, byte[] received)
		{
			TrySocket().ReceiveTimeout = ReceiveTimeout;
			sockToSend.SendTo(data, endPoint);
			total = 0;
			try 
			{ 
				total = sockToSend.Receive(received);
				return true;
			}
			catch(System.Exception e)
			{
				if (agent != null)
				{
                    GD.PrintErr("Script ai4u2unity is not connected! Start the ai4u2unity script! Network error: " + e.Message);
                    agent.GetTree().Quit();
				}
				return false;
            }
        }
	}
}
