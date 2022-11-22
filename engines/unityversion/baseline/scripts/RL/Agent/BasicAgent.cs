using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ai4u;

namespace ai4u
{
    
    /// <summary>DPRLAgent - Dimentional Physical Reinforcement Learning Agent
    /// This class models an agent with physical rigidbody control in a tridimentional world. </summary> 
    [RequireComponent(typeof(DoneSensor))]
    [RequireComponent(typeof(RewardSensor))]
    [RequireComponent(typeof(StepSensor))]
    [RequireComponent(typeof(IDSensor))]
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

        ///<summary> <code>doneAtNegativeReward</code> ends the simulation whenever the agent receives a negative reward.</summary>
        public bool doneAtNegativeReward = true;
        ///<summary> <code>doneAtPositiveReward</code> ends the simulation whenever the agent receives a positive reward.</summary>
        public bool doneAtPositiveReward = false;
        ///<summary>The maximum number of steps per episode.</summary>
        public int MaxStepsPerEpisode = 0;
        public bool childrenSensors = true;
        public Sensor[] sensors;
        public Actuator[]  actuators;
        public List<RewardFunc> rewards;

        //Agent's ridid body
        private bool done;
        protected float reward;
        private Dictionary<string, bool> firstTouch;
        private Dictionary<string, Sensor> sensorsMap;
        private List<Actuator> actuatorList;
        private List<Sensor> sensorList;
        private int numberOfSensors = 0;
        private int numberOfActuators = 0;

        public bool Done
        {
            get
            {
                return done;
            }

            set
            {
                bool pd = done;
                done = value;
                if (!pd && done)
                {
                    EndOfEpisode();
                }
            }
        }

        public override void EndOfEpisode()
        {
            if (endOfEpisodeEvent != null)
            {
                endOfEpisodeEvent(this);
            }
        }

        public bool TryGetSensor(string key, out Sensor s)
        {
            return sensorsMap.TryGetValue(key, out s);
        }

        public virtual void RequestDoneFrom(RewardFunc rf) {
            Done = true;
        }

        public override void Setup()
        {
            if (controlRequestor == null)
            {
                throw new System.Exception("ControlRequestor is mandatory to BasicAgent! Set a ControlRequestor component for this agent.");
            }
            setupIsDone = false;
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            actuatorList = new List<Actuator>();
            sensorList = new List<Sensor>();
            sensorsMap = new Dictionary<string, Sensor>();
            if (rewards == null)
            {
                rewards = new List<RewardFunc>();
            }

            DoneSensor doneSensor = GetComponent<DoneSensor>();
            doneSensor.SetAgent(this);
            sensorList.Add(doneSensor);
            sensorsMap[doneSensor.perceptionKey] = doneSensor;

            RewardSensor rewardSensor = GetComponent<RewardSensor>();
            rewardSensor.SetAgent(this);
            sensorList.Add(rewardSensor);
            sensorsMap[rewardSensor.perceptionKey] = rewardSensor;

            IDSensor idSensor = GetComponent<IDSensor>();
            idSensor.SetAgent(this);
            sensorList.Add(idSensor);
            sensorsMap[idSensor.perceptionKey] = idSensor;

            StepSensor stepSensor = GetComponent<StepSensor>();
            stepSensor.SetAgent(this);
            sensorList.Add(stepSensor);
            sensorsMap[stepSensor.perceptionKey] = stepSensor;
            numberOfSensors = 4;

            for (int i = 0; i < transform.childCount; i++) 
            {
                GameObject obj = transform.GetChild(i).gameObject;
                
                if (childrenSensors)
                {
                    Sensor s = obj.GetComponent<Sensor>();
                    if (s != null && s.isActive)
                    {
                        sensorList.Add(s);
                        numberOfSensors++;
                        sensorsMap[s.perceptionKey] = s;
                    }
                }

                RewardFunc r = obj.GetComponent<RewardFunc>();
                if ( r != null)
                {
                    rewards.Add(r);
                }
            }

            foreach(RewardFunc r in rewards)
            {
                if (r == null)
                {
                    Debug.LogWarning("You add a null reward func for agent " + ID + "! Fix it!");
                }
                else
                {
                    r.OnSetup(this);
                }
            }

            foreach(Sensor s in sensors)
            {
                if (s == null)
                {
                    Debug.LogWarning("Sensor is null!!! Set a valid sensor in agent's sensor list!");
                } else if (s.isActive)
                {
                    s.SetAgent(this);
                    sensorList.Add(s);
                    sensorsMap[s.perceptionKey] = s;
                }
            }

            if (sensorList.Count == 0) {
                Debug.LogWarning("Agent without sensors. Add at least one sensor for this agent to be able to perceive the world! GameObject: " + gameObject.name);
            }
            
            foreach(Actuator a in actuators) {
                if (a == null)
                {
                    Debug.LogWarning("Actuator is null!!! Set a valid actuator in agent's actuators list!");
                }
                actuatorList.Add(a);
                numberOfActuators++;
            }

            if (actuatorList.Count == 0) {
                Debug.LogWarning("Agent without actuators. Add at least one actuator for this agent to be able to change the world! GameObject: " + gameObject.name);
            }

            int totalNumberOfSensors = sensorList.Count;

            desc = new string[totalNumberOfSensors];
            types = new byte[totalNumberOfSensors];
            values = new string[totalNumberOfSensors];
            controlRequestor.SetAgent(this);

            RequestCommand request = new RequestCommand(3);
            request.SetMessage(0, "__target__", ai4u.Brain.STR, "envcontrol");
            request.SetMessage(1, "max_steps", ai4u.Brain.INT, MaxStepsPerEpisode);
            request.SetMessage(2, "id", ai4u.Brain.STR, ID);

            var cmds = controlRequestor.RequestEnvControl(this, request);
            if (cmds == null)
            {
                throw new System.Exception("ai4u2unity connection error!");
            }
            setupIsDone = true;

            foreach (Sensor sensor in sensorList)
            {
                if (sensor.resetable)
                {
                    AddResetListener(sensor);
                }
                sensor.OnSetup(this);
            }

            foreach(Actuator a in actuatorList)
            {
                a.OnSetup(this);
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
                if (from.causeEpisodeToEnd)
                {
                    Done = true;
                }
            }
            reward += v;
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

        public virtual void PreconditionFailListener(RewardFunc func, RewardFunc precondiction) { 
            if (Done) return;
            if (func is TouchRewardFunc) {
                if (!checkFirstTouch(func.gameObject.tag) && !((TouchRewardFunc)func).allowNext)
                {
                    this.RequestDoneFrom(func);
                }
            }
        }

        public virtual void touchListener(TouchRewardFunc origin)
        {
            //TODO add behavior here
        }

        public virtual void boxListener(BoxRewardFunc origin)
        {
            //TODO add behavior here
        }

        public override bool Alive()
        {
            return !Done;
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

        private bool checkFirstTouch(string tag){
            if (firstTouch.ContainsKey(tag)) {
                return false;
            } else {
                firstTouch[tag] = false;
                return true;
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

        private void ResetPlayer()
        {
            nSteps = 0;
            reward = 0;
            Done = false;
            firstTouch = new Dictionary<string, bool>(); 
            
                    
            UpdateState();
            NotifyReset();
        }

        public float Reward
        {
            get
            {
                return reward;
            }
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
                            Debug.LogWarning("Error: array of float sensor " + s.name + " returning null value!");
                        }
                        SetStateAsFloatArray(i, s.perceptionKey, fv);
                        break;
                    case SensorType.sfloat:
                        var fv2 = s.GetFloatValue();
                        SetStateAsFloat(i, s.perceptionKey, fv2);
                        break;
                    case SensorType.sint:
                        var fv3 = s.GetIntValue();
                        SetStateAsInt(i, s.perceptionKey, fv3);
                        break;
                    case SensorType.sstring:
                        var fv4 = s.GetStringValue();
                        if (fv4 == null)
                        {
                            Debug.LogWarning("Error: string sensor " + s.name + " returning null value!");
                        }
                        SetStateAsString(i, s.perceptionKey, fv4);
                        break;
                    case SensorType.sbool:
                        var fv5 = s.GetBoolValue();
                        SetStateAsBool(i, s.perceptionKey, fv5);
                        break;
                    case SensorType.sbytearray:
                        var fv6 = s.GetByteArrayValue();
                        if (fv6 == null)
                        {
                            Debug.LogWarning("Error: byte array sensor " + s.name + " returning null value!");
                        }
                        SetStateAsByteArray(i, s.perceptionKey, fv6);
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
    }
}
