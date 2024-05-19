using Godot;
namespace ai4u
{
	public class LocalBrain : Brain
	{
		public Controller Controller {get; }
		public LocalBrain(Controller ctrl)
		{
			this.Controller = ctrl;
		}
		
		public override void Setup(Agent agent)
		{
			this.agent = agent;
			if (this.Controller != null)
			{
				this.Controller.Setup(agent);
			}
			else
			{
				throw new System.Exception("LocalBrain is without a controller. Add a valid controller to LocalBrain for fix this error!");
			}
		}
		
		public override void OnReset(Agent agent)
		{
			this.Controller.OnReset(agent);
		}

		public override void OnStepReward(int step, float reward)
		{
			this.Controller.LastStep = step;
			this.Controller.LastReward = reward;
		}

		public string SendMessage(string[] desc, byte[] tipo, string[] valor)
		{
			this.Controller.ReceiveState(desc, tipo, valor);
			return this.Controller.GetAction();
		}
	}
}
