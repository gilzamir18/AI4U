using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ai4u
{
	public partial class BasicAgent : Agent
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
		public event AgentEpisodeHandler agentRestartEvent;	

		[Export]
		private NodePath avatarPath;
		private Node avatarBody;

		[Export]
		private bool remote;

		///<summary> <code>doneAtNegativeReward</code> ends the simulation whenever the agent receives a negative reward.</summary>		
		[Export]
		private bool doneAtNegativeReward = true;
		
		///<summary> <code>doneAtPositiveReward</code> ends the simulation whenever the agent receives a positive reward.</summary>
		[Export]
		public bool doneAtPositiveReward = false;
		
		///<summary>The maximum number of steps per episode.</summary>
		[Export]
		public int MaxStepsPerEpisode = 0;

		[Export]
		public float rewardScale = 1.0f;

		[Export]
		public bool checkEpisodeTruncated = true;
		
		private bool truncated;
		private float reward;
		private float lastReward;
		private bool done;
		
		private List<RewardFunc> rewards;

		private Dictionary<string, bool> firstTouch;
		private Dictionary<string, ISensor> sensorsMap;
		private List<Actuator> actuatorList;
		private List<ISensor> sensorList;
		private int numberOfSensors = 0;
		private int numberOfActuators = 0;
		private ModelMetadataLoader metadataLoader;
		private int NUMBER_OF_CONTROLINFO = 7;
		
		private int totalNumberOfSensors = 0;
		public override void SetupAgent(ControlRequestor requestor)
		{	
			totalNumberOfSensors = 0;
			controlRequestor = requestor;
			numberOfSensors = 0;
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

			avatarBody = GetNode<Node>(avatarPath);

			actuatorList = new List<Actuator>();
			rewards = new List<RewardFunc>();
			sensorList = new List<ISensor>();
			sensorsMap = new Dictionary<string, ISensor>();

			if (remote)
			{
				RemoteBrain r = new RemoteBrain();
				SetBrain(r);
			}

			var children = GetChildren();
			foreach (Node node in children)
			{
				if (remote && node is RemoteConfiguration && brain != null)
				{
					RemoteBrain r = (RemoteBrain) brain;
					var config = (RemoteConfiguration) node;
					r.Port = config.port;
					r.Host = config.host;
					r.Managed = config.managed;
					r.ReceiveTimeout = config.receiveTimeout;
					r.ReceiveBufferSize = config.receiveBufferSize;
					r.SendBufferSize = config.sendBufferSize;
				}
				else if (!remote && node is Controller)
				{
					var ctrl = (Controller) node;
					SetBrain(new LocalBrain(ctrl));
					
				}
				else if (node is ControllerConfiguration)
				{
					var controllerConfig = (ControllerConfiguration)node; 
					ControlInfo.skipFrame = controllerConfig.skipFrame;
					ControlInfo.repeatAction = controllerConfig.repeatAction;
				} 
				else if (node is RewardFunc)
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
			
			
			if (brain != null)
			{
				brain.Setup(this);
			}
			else
			{
				if (remote)
				{
					throw new System.Exception($"Remote agent without a remote brain. Add a valid remote brain for the agent {ID}");
				}
				else
				{
					throw new System.Exception($"Local agent without a Controller child. Add child Controller node for the agent {ID}");
				}
			}

			DoneSensor doneSensor = new DoneSensor();
			doneSensor.isInput = false;
			doneSensor.SetAgent(this);
			sensorList.Add(doneSensor);
			CallDeferred("add_child", doneSensor);

			RewardSensor rewardSensor = new RewardSensor();
			rewardSensor.isInput = false;
			rewardSensor.SetAgent(this);
			sensorList.Add(rewardSensor);
			CallDeferred("add_child", rewardSensor);

			IDSensor idSensor = new IDSensor();
			idSensor.isInput = false;
			idSensor.SetAgent(this);
			sensorList.Add(idSensor);
			CallDeferred("add_child", idSensor);

			StepSensor stepSensor = new StepSensor();
			stepSensor.isInput = false;
			stepSensor.SetAgent(this);
			sensorList.Add(stepSensor);
			CallDeferred("add_child", stepSensor);
			numberOfSensors = 4;

			if (checkEpisodeTruncated)
			{
				TruncatedSensor truncatedSensor = new TruncatedSensor();
				truncatedSensor.SetIsInput(false);
				truncatedSensor.SetAgent(this);
				sensorList.Add(truncatedSensor);
				sensorsMap[truncatedSensor.GetKey()] = truncatedSensor;
				numberOfSensors += 1;
			}


			totalNumberOfSensors = sensorList.Count;

			desc = new string[totalNumberOfSensors + NUMBER_OF_CONTROLINFO];
			types = new byte[totalNumberOfSensors + NUMBER_OF_CONTROLINFO];
			values = new string[totalNumberOfSensors + NUMBER_OF_CONTROLINFO];
			
			foreach (RewardFunc r in rewards)
			{
				r.OnSetup(this);
			}
			
			foreach (ISensor sensor in sensorList)
			{
				if (sensor.IsResetable())
				{
					AddResetListener(sensor);
				}
				sensor.OnSetup(this);
			}

			foreach(Actuator a in actuatorList)
			{
				a.OnSetup(this);
			}

			metadataLoader = new ModelMetadataLoader(this);
			string metadatastr = metadataLoader.toJson();

			RequestCommand request = new RequestCommand(5);
			request.SetMessage(0, "__target__", ai4u.Brain.STR, "envcontrol");
			request.SetMessage(1, "max_steps", ai4u.Brain.INT, MaxStepsPerEpisode);
			request.SetMessage(2, "id", ai4u.Brain.STR, ID);
			request.SetMessage(3, "modelmetadata", ai4u.Brain.STR, metadatastr);
			request.SetMessage(4, "config", ai4u.Brain.INT, 1);

			var cmds = controlRequestor.RequestEnvControl(this, request);
			
			if (cmds == null)
			{
				throw new System.Exception("ai4u2unity connection error!");
			}
			setupIsDone = true;
		}

		public override void ResetCommandBuffer()
		{
			desc = new string[totalNumberOfSensors + NUMBER_OF_CONTROLINFO];
			types = new byte[totalNumberOfSensors + NUMBER_OF_CONTROLINFO];
			values = new string[totalNumberOfSensors + NUMBER_OF_CONTROLINFO];
		}

		public override void ResetReward()
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
		
		public Node Body
		{
			get
			{
				return avatarBody;
			}
		}
		
		public override void UpdateReward()
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
		
		public List<Actuator> Actuators
		{
			get 
			{
				return actuatorList;
			}
		}

		public List<ISensor> Sensors 
		{
			get
			{
				return sensorList;
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
		
		public bool Truncated
		{
			get
			{
				return truncated;
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

		public override void AgentReset() 
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
			brain.OnReset(this);
		}

		public override void AgentRestart()
		{
			if (agentRestartEvent != null)
			{
				agentRestartEvent(this);
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

			InitializeDataFromSensor();


			if (endOfUpdateStateEvent != null)
			{
				endOfUpdateStateEvent(this);
			}
		}
		
		private void InitializeDataFromSensor()
		{
			int n = sensorList.Count;
			for (int i = 0; i < n; i++) {
				ISensor s = sensorList[i];
				switch(s.GetSensorType())
				{
					case SensorType.sfloatarray:
						var fv = s.GetFloatArrayValue();
						if (fv == null)
						{
							throw new System.Exception("Error: array of float sensor " + s.GetName() + " returning null value!");
						}
						SetStateAsFloatArray(i, s.GetKey(), fv);
						break;
					case SensorType.sfloat:
						var fv2 = s.GetFloatValue();
						SetStateAsFloat(i, s.GetKey(), fv2);
						break;
					case SensorType.sint:
						var fv3 = s.GetIntValue();
						SetStateAsInt(i, s.GetKey(), fv3);
						break;
					case SensorType.sstring:
						var fv4 = s.GetStringValue();
						if (fv4 == null)
						{
							throw new System.Exception("Error: string sensor " + s.GetName() + " returning null value!");
						}
						SetStateAsString(i, s.GetKey(), fv4);
						break;
					case SensorType.sbool:
						var fv5 = s.GetBoolValue();
						SetStateAsBool(i, s.GetKey(), fv5);
						break;
					case SensorType.sbytearray:
						var fv6 = s.GetByteArrayValue();
						if (fv6 == null)
						{
							throw new System.Exception("Error: byte array sensor " + s.GetName() + " returning null value!");
						}
						SetStateAsByteArray(i, s.GetKey(), fv6);
						break;
					default:
						break;
				}
			}
			SetStateAsBool(n, "__ctrl_paused__", ControlInfo.paused);
			SetStateAsBool(n+1, "__ctrl_stopped__", ControlInfo.stopped);
			SetStateAsBool(n+2, "__ctrl_applyingAction__", ControlInfo.applyingAction);
			SetStateAsInt(n+3, "__ctrl_frameCounter__", ControlInfo.frameCounter);
			SetStateAsInt(n+4, "__ctrl_skipFrame__", ControlInfo.skipFrame);
			SetStateAsBool(n+5, "__ctrl_repeatAction__", ControlInfo.repeatAction);
			SetStateAsBool(n+6, "__ctrl_envMode__", ControlInfo.envmode);
		}
		
		private void ResetPlayer()
		{
			nSteps = 0;
			reward = 0;
			truncated = false;
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
		
		private bool TryGetSensor(string key, out ISensor s)
		{
			return sensorsMap.TryGetValue(key, out s);
		}
	}
}
