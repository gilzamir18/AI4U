using Godot;
using System;

public partial class RemoteConfiguration : Node
{
	///<summary>If true, the remote brain will be 
	///managed manually. Thus, in this case, command 
	///line arguments do not alter the properties of 
	///the remote brain.</summary>
	[Export]
	public bool managed = false;
	
	///<summary>The IP of the bemaker2unity training server.</summary>
	[Export]
	public string host = "127.0.0.1";
	
	///<summary>The server port of the bemaker2unity training server.</summary>
	[Export]
	public int port = 8080;
	[Export]
	public int receiveTimeout = 2000;
	[Export]
	public int receiveBufferSize = 8192;
	[Export]
	public int sendBufferSize = 8192;
}
