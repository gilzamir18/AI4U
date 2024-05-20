using Godot;
using ai4u;

namespace ai4u;

public partial class TouchSensor : Sensor
{

		[Export]
		private Node target;

		private HistoryStack<float> stack;
		// Start is called before the first frame update

		private bool state = false;

		public override void OnSetup(Agent agent)
		{
			this.type = SensorType.sfloatarray;
			this.shape = new int[1]{stackedObservations};
			this.agent = (BasicAgent) agent;
			stack = new HistoryStack<float>(stackedObservations);
			RigidBody3D body = this.agent.GetAvatarBody() as RigidBody3D;
			body.BodyEntered += OnEntered;
			body.BodyExited += OnExited;
		}

		public override void OnReset(Agent agent)
		{
			stack = new HistoryStack<float>(stackedObservations);
		}

		public void OnEntered(Node body) {
			if (body == target) {
				state = true;
			}
		}

		public void OnExited(Node body)
		{
			if (body == target)
			{
				state = false;
			}
		}

		public override float[] GetFloatArrayValue()
		{
			stack.Push(state? 1.0f : -1.0f);
			return stack.Values;
		}
}


