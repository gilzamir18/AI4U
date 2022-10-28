using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ai4u
{
	public class BasicAgent : Agent
	{
		public delegate void AgentEpisodeHandler(BasicAgent agent);
		public event AgentEpisodeHandler beforeTheResetEvent;
		public event AgentEpisodeHandler endOfEpisodeEvent;
		public event AgentEpisodeHandler beginOfEpisodeEvent;
		public event AgentEpisodeHandler endOfStepEvent;
		public event AgentEpisodeHandler beginOfStepEvent;
		public event AgentEpisodeHandler beginOfUpdateStateEvent;
		public event AgentEpisodeHandler endOfUpdateStateEvent;
		public event AgentEpisodeHandler beginOfApplyActionEvent;
		public event AgentEpisodeHandler endOfApplyActionEvent; 		

		[Export]
		private NodePath avatarPath;
		private Node avatarBody;

		///<summary> <code>doneAtNegativeReward</code> ends the simulation whenever the agent receives a negative reward.</summary>		
		[Export]
		private bool doneAtNegativeReward = true;
		
		///<summary> <code>doneAtPositiveReward</code> ends the simulation whenever the agent receives a positive reward.</summary>
		[Export]
		public bool doneAtPositiveReward = false;
		
		///<summary>The maximum number of steps per episode.</summary>
		[Export]
		public int MaxStepsPerEpisode = 0;

		private float reward;
		private float lastReward;
		private bool done;
		
		private List<RewardFunc> rewards;

		private Dictionary<string, bool> firstTouch;
		private Dictionary<string, Sensor> sensorsMap;
		private List<Actuator> actuatorList;
		private List<Sensor> sensorList;
		private int numberOfSensors = 0;
		private int numberOfActuators = 0;
		
		public override void Setup()
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

			avatarBody = GetNode<Node>(avatarPath);

			actuatorList = new List<Actuator>();
			rewards = new List<RewardFunc>();
			sensorList = new List<Sensor>();
			sensorsMap = new Dictionary<string, Sensor>();

			var children = GetChildren();
			foreach (Node node in children)
			{
				if (node is RewardFunc)
				{
					RewardFunc rf = (RewardFunc)node;
					rf.OnSetup(this);
					rewards.Add(rf);
				}
				else if (node is Sensor)
				{
					var s = (Sensor) node;
					sensorList.Add(s);
					numberOfSensors++;
				}
				else if (node is Actuator)
				{
					var a = (Actuator) node;
					actuatorList.Add(a);
					numberOfActuators++;
				}
			}
			

			DoneSensor doneSensor = new DoneSensor();
			doneSensor.SetAgent(this);
			sensorList.Add(doneSensor);
			CallDeferred("add_child", doneSensor);

			RewardSensor rewardSensor = new RewardSensor();
			rewardSensor.SetAgent(this);
			sensorList.Add(rewardSensor);
			CallDeferred("add_child", rewardSensor);

			IDSensor idSensor = new IDSensor();
			idSensor.SetAgent(this);
			sensorList.Add(idSensor);
			CallDeferred("add_child", idSensor);

			StepSensor stepSensor = new StepSensor();
			stepSensor.SetAgent(this);
			sensorList.Add(stepSensor);
			CallDeferred("add_child", stepSensor);

			numberOfSensors += 4;

			desc = new string[numberOfSensors];
			types = new byte[numberOfSensors];
			values = new string[numberOfSensors];
			
			RequestCommand request = new RequestCommand(3);
			request.SetMessage(0, "__target__", ai4u.Brain.STR, "envcontrol");
			request.SetMessage(1, "max_steps", ai4u.Brain.INT, MaxStepsPerEpisode);
			request.SetMessage(2, "id", ai4u.Brain.STR, ID);

			var cmds = brain.ControlRequestor.RequestEnvControl(request);
			if (cmds == null)
			{
				throw new System.Exception("ai4u2unity connection error!");
			}
			setupIsDone = true;
			
			foreach (Sensor sensor in sensorList)
			{
				if (sensor.Resetable)
				{
					AddResetListener(sensor);
				}
				sensor.OnSetup(this);
				sensorsMap[sensor.PerceptionKey] = idSensor;
			}

			foreach(Actuator a in actuatorList)
			{
				a.OnSetup(this);
			}
		}
		
		public void ResetReward()
		{
			reward = 0;
			if (beginOfStepEvent != null)
			{
				beginOfStepEvent(this);
			}
		}
		
		public Node GetAvatarBody()
		{
			return avatarBody;
		}
		
		public void UpdateReward()
		{
			int n = rewards.Count;

			for (int i = 0; i < n; i++)
			{
				rewards[i].OnUpdate();
			}
			if (endOfStepEvent != null)
			{
				endOfStepEvent(this);
			}
		}
		
		
		public bool Done
		{
			get
			{
				return done;
			}
			
			set
			{
				done = value;
			}
		}
		
		public float AcummulatedReward
		{
			get
			{
				return this.reward;
			}
		}
		
		public float LastReward
		{
			get
			{
				return this.lastReward;
			}
		}
		
		public float Reward
		{
			get
			{
				return this.reward;
			}	
		}
		
		public virtual void AddReward(float v, RewardFunc from = null){
			if (doneAtNegativeReward && v < 0) {
				Done = true;
			}

			if (doneAtPositiveReward && v > 0) {
				Done = true;
			}

			if (from != null)
			{
				if (from.causeEpisodeToEnd && v != 0)
				{
					Done = true;
				}
			}
			reward += v;
			lastReward = v;
		}
		
		public void AddReward(float v, bool causeEpisodeToEnd){
			if (doneAtNegativeReward && v < 0) {
				Done = true;
			}

			if (doneAtPositiveReward && v > 0) {
				Done = true;
			}

			if (causeEpisodeToEnd)
			{
				Done = true;
			}

			reward += v;
		}

		public override void  ApplyAction()
		{
			if (beginOfApplyActionEvent != null)
			{
				beginOfApplyActionEvent(this);
			}
			if (MaxStepsPerEpisode > 0 && nSteps >= MaxStepsPerEpisode) {
				Done = true;
			}
			int n = actuatorList.Count;
			for (int i = 0; i < n; i++)
			{
				if (!Done)
				{
					if (GetActionName() == actuatorList[i].actionName)
					{
						actuatorList[i].Act();
					}
				}
			}
			if (!Done)
			{
				if (endOfApplyActionEvent != null)
				{
					endOfApplyActionEvent(this);
				}
			}
		}

		public override void Reset() 
		{
			if (beforeTheResetEvent != null)
			{
				beforeTheResetEvent(this);
			}
			ResetPlayer();
			if (beginOfEpisodeEvent != null)
			{
				beginOfEpisodeEvent(this);
			}
		}
		
		public virtual void RequestDoneFrom(RewardFunc rf) {
			Done = true;
		}

		public override bool Alive()
		{
			return !Done;
		}

		public override void EndOfEpisode()
		{
			if (endOfEpisodeEvent != null)
			{
				endOfEpisodeEvent(this);
			}
		}

		public override void UpdateState()
		{
			if (beginOfUpdateStateEvent != null)
			{
				beginOfUpdateStateEvent(this);
			}

			int n = sensorList.Count;
			for (int i = 0; i < n; i++) {
				Sensor s = sensorList[i];
				switch(s.type)
				{
					case SensorType.sfloatarray:
						var fv = s.GetFloatArrayValue();
						if (fv == null)
						{
							GD.Print("Error: array of float sensor " + s.Name + " returning null value!");
						}
						SetStateAsFloatArray(i, s.PerceptionKey, fv);
						break;
					case SensorType.sfloat:
						var fv2 = s.GetFloatValue();
						SetStateAsFloat(i, s.PerceptionKey, fv2);
						break;
					case SensorType.sint:
						var fv3 = s.GetIntValue();
						SetStateAsInt(i, s.PerceptionKey, fv3);
						break;
					case SensorType.sstring:
						var fv4 = s.GetStringValue();
						if (fv4 == null)
						{
							GD.Print("Error: string sensor " + s.Name + " returning null value!");
						}
						SetStateAsString(i, s.PerceptionKey, fv4);
						break;
					case SensorType.sbool:
						var fv5 = s.GetBoolValue();
						SetStateAsBool(i, s.PerceptionKey, fv5);
						break;
					case SensorType.sbytearray:
						var fv6 = s.GetByteArrayValue();
						if (fv6 == null)
						{
							GD.Print("Error: byte array sensor " + s.Name + " returning null value!");
						}
						SetStateAsByteArray(i, s.PerceptionKey, fv6);
						break;
					default:
						break;
				}
			}


			if (endOfUpdateStateEvent != null)
			{
				endOfUpdateStateEvent(this);
			}
		}

		private void ResetPlayer()
		{
			nSteps = 0;
			reward = 0;
			Done = false;
			firstTouch = new Dictionary<string, bool>(); 
			UpdateState();
			NotifyReset();
		}
		
		private bool checkFirstTouch(string tag){
			if (firstTouch.ContainsKey(tag)) {
				return false;
			} else {
				firstTouch[tag] = false;
				return true;
			}
		}
		
		private bool TryGetSensor(string key, out Sensor s)
		{
			return sensorsMap.TryGetValue(key, out s);
		}
	}
}
