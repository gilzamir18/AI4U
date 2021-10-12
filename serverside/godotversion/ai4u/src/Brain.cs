using Godot;
using System;
using System.Collections.Generic;

namespace ai4u
{
	public abstract class Brain : Node
	{
		public static byte FLOAT = 0;
		public static byte INT = 1;
		public static byte BOOL = 2;
		public static byte STR = 3;
		public static byte OTHER = 4;
		public static byte FLOAT_ARRAY = 5;
		protected string receivedcmd; 
		protected string[] receivedargs;

		public abstract void SendMessage(string[] desc, byte[] tipo, string[] valor);

		protected Agent agent = null;
		
		[Export]
		public bool fixedUpdate = true;
		[Export]
		public bool updateStateOnUpdate = false;

		private float deltaTime;

		public float DeltaTime
		{
			get
			{
				return deltaTime;
			}
			
			set
			{
				deltaTime = value;
			}
		}

		public string GetReceivedCommand()
		{
			return receivedcmd;
		}


		public string[] GetReceivedArgs()
		{
			return receivedargs;
		}
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			
			agent = GetParent() as Agent;
		}
	}
}
