using Godot;
using System;

namespace ai4u
{
	public class LocalBrain : Brain
	{
		private Controller controller;
		
		public void Start()
		{
			agent = GetParent() as Agent;
			foreach (Node node in GetChildren())
			{
				if ( node.GetType().IsSubclassOf(typeof(Controller)) ) 
				{
					controller = node as Controller;
					break;
				}
			}

			if (controller == null) {
				GD.Print("You must specify a controller for the game object: " + GetParent().GetParent().Name);
			}

			if (agent == null) {
				GD.Print("You must specify an agent for the game object: " + GetParent().GetParent().Name);
			}
			
			if (controller.resettable) {
				agent.AddResetListener(controller);
			}
			agent.SetBrain(this);
			agent.StartData();
		}

		private void LocalDecision()
		{
			object[] msg = controller.GetAction();
			receivedcmd = (string)msg[0];
			receivedargs = (string[])msg[1];
			agent.ApplyAction();
			agent.UpdatePhysics();
			
			agent.UpdateState();
			agent.GetState();
		}

		public void FixedUpdate()
		{
			if (fixedUpdate)
			{
				LocalDecision();
			}
		}

		public void Update()
		{
			if (!fixedUpdate)
			{
				LocalDecision();
			}
		}

		public override void SendMessage(string[] desc, byte[] tipo, string[] valor)
		{
			controller.ReceiveState(desc, tipo, valor);
		}
		
		public override void _Ready()
		{
			Start();
		}	

	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	 	public override void _Process(float delta)
	  	{
			Update();
		}

		public override void _PhysicsProcess(float delta)
		{
			FixedUpdate();
		}
	}
}
