using System.Collections;
using System.Collections.Generic;
using Godot;
using ai4u;
using System.Text;

namespace ai4u.ext {
	
	public struct RewardInfo
	{
		public float reward;
		public int timeStep;
		public string name;
		public NodePath nodePath;
		
		public RewardInfo(string name, float r, int ts, NodePath path)
		{
			this.name = name;
			this.reward = r;
			this.timeStep = ts;
			this.nodePath = path;
		}
		
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("{");
			sb.Append("\"name\":");
			sb.Append("\"" + name + "\"");
			sb.Append(",");
			sb.Append("\"reward\":");
			sb.Append("" + reward);
			sb.Append(",");
			sb.Append("\"timeStep\":");
			sb.Append("" + timeStep);
			sb.Append("}");
			return sb.ToString();
		}
	} 
		
	public abstract class RLAgent : Agent
	{
		protected float reward;
		private int id;
		private bool done = false;
		private FloatSensor rewardSensor;
		private BoolSensor doneSensor;
		private int nSteps = 0;
		[Export]
		public int maxSteps = 500;

		private List<RewardInfo> rewardOcurrence; //store the ocurrence of reward groups
		private Dictionary<string, bool> rewardRegistry;
		
		public float Reward
		{
			get
			{
				return reward;
			}
		}
		
		public List<RewardInfo> RewardOcurrence
		{
			get
			{
				return rewardOcurrence;	
			}
		}
		
		public FloatSensor RewardSensor
		{
			get
			{
				return rewardSensor;
			}
		}
		
		public BoolSensor BoolSensor
		{
			get
			{
				return doneSensor;
			}
		}
		
		public int CurrentStep
		{
			get 
			{
				return nSteps;	
			}
		}

		public override void OnSetup()
		{
			rewardOcurrence = new List<RewardInfo>();
			rewardRegistry = new Dictionary<string, bool>();
			rewardSensor = new FloatSensor();
			rewardSensor.perceptionKey = "reward";
			rewardSensor.type = SensorType.sfloat;
			rewardSensor.shape = new int[]{};
			rewardSensor.isState = false;
			rewardSensor.resettable = true;
			rewardSensor.OnBinding(this);
			AddSensor(rewardSensor);

			doneSensor = new BoolSensor();
			doneSensor.perceptionKey = "done";
			doneSensor.type = SensorType.sbool;
			doneSensor.shape = new int[]{};
			doneSensor.isState = false;
			doneSensor.resettable = true;
			doneSensor.OnBinding(this);
			AddSensor(doneSensor);
			
			RestartActuator restartActuator = new RestartActuator();
			restartActuator.actionName = "restart";
			restartActuator.IsOperation = true;
			restartActuator.OnBinding(this);
			AddActuator(restartActuator);
			
			NoOpActuator noOpActuator = new NoOpActuator();
			noOpActuator.actionName = "noop";
			noOpActuator.IsOperation = false;
			noOpActuator.OnBinding(this);
			AddActuator(noOpActuator);
		}

		public virtual bool Done {
			get {
				return done;
			}

			set {
				if (value)
					HandleOnDone();
				done = value;
			}
		}

		public int Id {
			get {
				return id;
			}

			set {
				id = value;
			}
		}

		public override void HandleOnResetEvent()
		{
			rewardRegistry = new Dictionary<string, bool>();
			rewardOcurrence = new List<RewardInfo>();
			reward = 0;
			Done = false;
			nSteps = 0;
			SensorsEnabled = true;
		}
		
		public override bool FilterAction(Actuator actuator) {
			if (actuator.actionReward != null) {
				actuator.actionReward.RewardFrom(this);
			}
			return !Done || actuator.IsOperation;
		}

		public virtual void RequestDoneFrom(object rf)
		{
			this.Done = true;
			this.doneSensor.Data = this.Done;
		}


		public virtual void AddReward(float v, RewardFunc from = null, bool causeEpisodeToEnd = false) 
		{
			if (from.isJustAnEvent)
			{
				return;
			}
			RewardInfo info = new RewardInfo(null, v, nSteps, null);
			reward += v;
			if (from != null) 
			{
					info.name = from.Name;
					info.nodePath = from.GetPath();
					if (!rewardRegistry.ContainsKey(from.Name))
					{
						rewardOcurrence.Add(info);
						rewardRegistry[from.Name] = true;
					}
					if (causeEpisodeToEnd) 
					{
						this.RequestDoneFrom(from);	
					}
			}
		}

		public override void AtBeginingOfTheStateUpdate()
		{
			if (nSteps > maxSteps)
			{
				Done = true;
			} 
			if (rewardSensor != null) rewardSensor.Data = reward;
			if (doneSensor != null) doneSensor.Data = Done;
		}
		
		public virtual void HandleOnDone() 
		{
			foreach(Actuator a in actuators)
			{
				a.OnDone();
			}
		}
		
		public override void AtEndOfTheStateUpdate()
		{
			nSteps++;
			if (!Done)
				reward = 0.0f;
		}
	}
}
