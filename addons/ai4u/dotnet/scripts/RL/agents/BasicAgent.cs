using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace ai4u
{
    /// <summary>
    /// The BasicAgent class is a partial implementation of an agent that can be used in machine learning environments.
    /// This class inherits from the Agent class and provides a basic framework for the agent's interaction with the environment.
    /// </summary>
    public partial class BasicAgent : Agent
    {
        /// <summary>
        /// Delegate for handling agent episode events.
        /// </summary>
        /// <param name="agent">The agent that triggered the event.</param>
        public delegate void AgentEpisodeHandler(BasicAgent agent);

        /// <summary>
        /// Event triggered before the reset.
        /// </summary>
        public event AgentEpisodeHandler OnResetStart;

        /// <summary>
        /// Event triggered at the end of an episode.
        /// </summary>
        public event AgentEpisodeHandler OnEpisodeEnd;

        /// <summary>
        /// Event triggered at the beginning of an episode.
        /// </summary>
        public event AgentEpisodeHandler OnEpisodeStart;

        /// <summary>
        /// Event triggered at the end of a step.
        /// </summary>
        public event AgentEpisodeHandler OnStepEnd;

        /// <summary>
        /// Event triggered at the beginning of a step.
        /// </summary>
        public event AgentEpisodeHandler OnStepStart;

        /// <summary>
        /// Event triggered at the beginning of state update.
        /// </summary>
        public event AgentEpisodeHandler OnStateUpdateStart;

        /// <summary>
        /// Event triggered at the end of state update.
        /// </summary>
        public event AgentEpisodeHandler OnStateUpdateEnd;

        /// <summary>
        /// Event triggered at the beginning of applying an action.
        /// </summary>
        public event AgentEpisodeHandler OnActionStart;

        /// <summary>
        /// Event triggered at the end of applying an action.
        /// </summary>
        public event AgentEpisodeHandler OnActionEnd;

        /// <summary>
        /// Event triggered when the agent starts.
        /// </summary>
        public event AgentEpisodeHandler OnAgentStart;

        [Export]
        private Node avatarBody;

        [Export]
        private bool remote;

        /// <summary>
        /// Ends the simulation whenever the agent receives a negative reward.
        /// </summary>
        [Export]
        private bool doneAtNegativeReward = true;

        /// <summary>
        /// Ends the simulation whenever the agent receives a positive reward.
        /// </summary>
        [Export]
        public bool doneAtPositiveReward = false;

        /// <summary>
        /// The maximum number of steps per episode.
        /// </summary>
        [Export]
        public int MaxStepsPerEpisode = 0;

        [Export]
        public float rewardScale = 1.0f;

        [Export]
        public bool checkEpisodeTruncated = true;

        [Export]
        internal int initialInputSize = 0;

        /// <summary>
        /// Gets the total reward for the current episode.
        /// </summary>
        public float EpisodeReward => episodeReward;

        private bool truncated;
        private float reward;
        private float lastReward;
        private bool done = true;
        private float episodeReward;

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

		private AgentRewardFunc agentRewardFunc;

        private AgentArraySensor agentArraySensor;

		public AgentRewardFunc Rewards => agentRewardFunc;

        public AgentArraySensor ArraySensor => agentArraySensor;

        /// <summary>
        /// Sets up the agent with the given control requestor.
        /// </summary>
        /// <param name="requestor">The control requestor to use for setup.</param>
        public override void SetupAgent(ControlRequestor requestor)
        {

			if (avatarBody == null)
			{
				//GD.PrintErr("Avatar body is null. Set a RigidBody or CharacterBody instead!");
				avatarBody = GetParent();
			}

            totalNumberOfSensors = 0;
            episodeReward = 0;
            controlRequestor = requestor;
            numberOfSensors = 0;
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            actuatorList = new List<Actuator>();
            rewards = new List<RewardFunc>();
            sensorList = new List<ISensor>();
            sensorsMap = new Dictionary<string, ISensor>();

			agentRewardFunc = new AgentRewardFunc();
        	agentRewardFunc.OnSetup(this);
			rewards.Add(agentRewardFunc);
            CallDeferred("add_child", agentRewardFunc);

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
                    RemoteBrain r = (RemoteBrain)brain;
                    var config = (RemoteConfiguration)node;
                    r.Port = config.port;
                    r.Host = config.host;
                    r.Managed = config.managed;
                    r.ReceiveTimeout = config.receiveTimeout;
                    r.ReceiveBufferSize = config.receiveBufferSize;
                    r.SendBufferSize = config.sendBufferSize;
                }
                else if (!remote && node is Controller)
                {
                    var ctrl = (Controller)node;
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
                    var s = (Sensor)node;
                    sensorList.Add(s);
                    numberOfSensors++;
                }
                else if (node is Actuator)
                {
                    var a = (Actuator)node;
                    actuatorList.Add(a);
                    numberOfActuators++;
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
            
            if (initialInputSize > 0)
            {
                agentArraySensor = new AgentArraySensor();
                agentArraySensor.isInput = true;
                agentArraySensor.SetAgent(this);
                sensorList.Add(agentArraySensor);
                CallDeferred("add_child", agentArraySensor);
                numberOfSensors = 5;
            }
            else
            {
                numberOfSensors = 4;
            }

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

            foreach (ISensor sensor in sensorList)
            {
                if (sensor.IsResetable())
                {
                    AddResetListener(sensor);
                }
                sensor.OnSetup(this);
            }

            foreach (Actuator a in actuatorList)
            {
                a.OnSetup(this);
            }

            metadataLoader = new ModelMetadataLoader(this);
            Metadata = metadataLoader.Metadata;
            string metadatastr = metadataLoader.toJson();

            RequestCommand request = new RequestCommand(5);
            request.SetMessage(0, "__target__", ai4u.Brain.STR, "envcontrol");
            request.SetMessage(1, "max_steps", ai4u.Brain.INT, MaxStepsPerEpisode);
            request.SetMessage(2, "id", ai4u.Brain.STR, ID);
            request.SetMessage(3, "modelmetadata", ai4u.Brain.STR, metadatastr);
            request.SetMessage(4, "config", ai4u.Brain.INT, 1);

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

            var cmds = controlRequestor.RequestEnvControl(this, request);
            if (cmds == null)
            {
                throw new System.Exception("ai4u2unity connection error!");
            }
            setupIsDone = true;
        }

        /// <summary>
        /// Gets the metadata as a JSON string.
        /// </summary>
        /// <returns>A JSON string representing the metadata.</returns>
        public string GetMetadataAsJson()
        {
            return metadataLoader.toJson();
        }

        /// <summary>
        /// Resets the command buffer.
        /// </summary>
        public override void ResetCommandBuffer()
        {
            desc = new string[totalNumberOfSensors + NUMBER_OF_CONTROLINFO];
            types = new byte[totalNumberOfSensors + NUMBER_OF_CONTROLINFO];
            values = new string[totalNumberOfSensors + NUMBER_OF_CONTROLINFO];
        }

        /// <summary>
        /// Resets the reward to zero.
        /// </summary>
        public override void ResetReward()
        {
            reward = 0;
            OnStepStart?.Invoke(this);
        }

        /// <summary>
        /// Gets the avatar body node.
        /// </summary>
        /// <returns>The avatar body node.</returns>
        public Node GetAvatarBody()
        {
            return avatarBody;
        }

        /// <summary>
        /// Updates the reward by calling the update method on each reward function.
        /// </summary>
        public override void UpdateReward()
        {
            int n = rewards.Count;

            for (int i = 0; i < n; i++)
            {
                rewards[i].OnUpdate();
            }
            brain.OnStepReward(nSteps, Reward);
            OnStepEnd?.Invoke(this);
        }

        /// <summary>
        /// Gets the list of actuators.
        /// </summary>
        public List<Actuator> Actuators => actuatorList;

        /// <summary>
        /// Gets the list of sensors.
        /// </summary>
        public List<ISensor> Sensors => sensorList;

        /// <summary>
        /// Gets or sets a value indicating whether the agent is done.
        /// </summary>
        public bool Done
        {
            get => done;
            set => done = value;
        }

        /// <summary>
        /// Gets a value indicating whether the episode was truncated.
        /// </summary>
        public bool Truncated => truncated;

        /// <summary>
        /// Gets the accumulated reward.
        /// </summary>
        [Obsolete]
        public float AcummulatedReward => reward;

        /// <summary>
        /// Gets the last reward received.
        /// </summary>
        public float LastReward => lastReward;

        /// <summary>
        /// Gets the current reward.
        /// </summary>
        public float Reward => reward;

        /// <summary>
        /// Adds a reward to the current reward.
        /// </summary>
        /// <param name="v">The reward value to add.</param>
        /// <param name="from">The reward function that caused the reward.</param>
        internal virtual void AddReward(float v, RewardFunc from = null)
        {
            reward += v;
            lastReward = v;
            episodeReward += v;
            if (doneAtNegativeReward && v < 0)
            {
                Done = true;
            }

            if (doneAtPositiveReward && v > 0)
            {
                Done = true;
            }

            if (from != null && from.causeEpisodeToEnd && v != 0)
            {
                Done = true;
            }
        }

        /// <summary>
        /// Adds a reward to the current reward and optionally ends the episode.
        /// </summary>
        /// <param name="v">The reward value to add.</param>
        /// <param name="causeEpisodeToEnd">Whether the reward should cause the episode to end.</param>
        internal void AddReward(float v, bool causeEpisodeToEnd)
        {
            if (doneAtNegativeReward && v < 0)
            {
                Done = true;
            }

            if (doneAtPositiveReward && v > 0)
            {
                Done = true;
            }

            if (causeEpisodeToEnd)
            {
                Done = true;
            }

            reward += v;
            episodeReward += v;
        }

        /// <summary>
        /// Applies the current action.
        /// </summary>
        public override void ApplyAction()
        {
            OnActionStart?.Invoke(this);

            if (MaxStepsPerEpisode > 0 && nSteps >= MaxStepsPerEpisode)
            {
                Done = true;
            }

            int n = actuatorList.Count;
            for (int i = 0; i < n; i++)
            {
                if (!Done && GetActionName() == actuatorList[i].actionName)
                {
                    actuatorList[i].Act();
                }
            }

            if (!Done)
            {
                OnActionEnd?.Invoke(this);
            }
        }

        /// <summary>
        /// Resets the agent.
        /// </summary>
        public override void AgentReset()
        {
            OnResetStart?.Invoke(this);
            ResetPlayer();
            OnEpisodeStart?.Invoke(this);
            brain.OnReset(this);
            episodeReward = 0;
        }

        /// <summary>
        /// Starts the agent.
        /// </summary>
        public override void AgentStart()
        {
            OnAgentStart?.Invoke(this);
        }

        /// <summary>
        /// Requests to end the episode from the specified reward function.
        /// </summary>
        /// <param name="rf">The reward function requesting the end of the episode.</param>
        public virtual void RequestDoneFrom(RewardFunc rf)
        {
            Done = true;
        }

        /// <summary>
        /// Determines whether the agent is alive.
        /// </summary>
        /// <returns>True if the agent is alive; otherwise, false.</returns>
        public override bool Alive()
        {
            return !Done;
        }

        /// <summary>
        /// Ends the episode.
        /// </summary>
        public override void EndOfEpisode()
        {
            OnEpisodeEnd?.Invoke(this);
        }

        /// <summary>
        /// Updates the agent's state.
        /// </summary>
        public override void UpdateState()
        {
            OnStateUpdateStart?.Invoke(this);
            InitializeDataFromSensor();
            OnStateUpdateEnd?.Invoke(this);
        }

        private void InitializeDataFromSensor()
        {
            int n = sensorList.Count;
            for (int i = 0; i < n; i++)
            {
                ISensor s = sensorList[i];
                switch (s.GetSensorType())
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
                    case SensorType.sintarray:
                        var v = s.GetIntArrayValue();
                        SetStateAsIntArray(i, s.GetKey(), v);
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
                    case SensorType.sstrings:
                        var fv7 = s.GetStringValues();
                        if (fv7 == null)
                        {
                            throw new System.Exception("Error: string array sensor " + s.GetName() + " returning null value!");
                        }
                        SetStateAsStringArray(i, s.GetKey(), fv7);
                        break;
                    default:
                        break;
                }
            }
            SetStateAsBool(n, "__ctrl_paused__", ControlInfo.paused);
            SetStateAsBool(n + 1, "__ctrl_stopped__", ControlInfo.stopped);
            SetStateAsBool(n + 2, "__ctrl_applyingAction__", ControlInfo.applyingAction);
            SetStateAsInt(n + 3, "__ctrl_frameCounter__", ControlInfo.frameCounter);
            SetStateAsInt(n + 4, "__ctrl_skipFrame__", ControlInfo.skipFrame);
            SetStateAsBool(n + 5, "__ctrl_repeatAction__", ControlInfo.repeatAction);
            SetStateAsBool(n + 6, "__ctrl_envMode__", ControlInfo.envmode);
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

        private bool checkFirstTouch(string tag)
        {
            if (firstTouch.ContainsKey(tag))
            {
                return false;
            }
            else
            {
                firstTouch[tag] = false;
                return true;
            }
        }

        public bool TryGetSensor(string key, out ISensor s)
        {
            return sensorsMap.TryGetValue(key, out s);
        }
    }
}
