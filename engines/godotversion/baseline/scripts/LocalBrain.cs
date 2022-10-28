using Godot;
namespace ai4u
{
	public class LocalBrain : Brain
	{
		[Export]
		private NodePath controllerPath;
		private Controller controller;
	
		public override void OnSetup(Agent agent)
		{
			controller = GetNode<Controller>(controllerPath);
			base.OnSetup(agent);
			if (agent == null)
			{
				agent = GetParent<Agent>();
			}
			if (controller == null) {
				GD.Print("You must specify a controller for the game object: " + Name);
			}

			if (agent == null) {
				GD.Print("You must specify an agent for the game object: " + Name);
			}

			agent.SetBrain(this);
			agent.Setup();
		}

		public string SendMessage(string[] desc, byte[] tipo, string[] valor)
		{
			controller.ReceiveState(desc, tipo, valor);
			return controller.GetAction();
		}
	}
}
