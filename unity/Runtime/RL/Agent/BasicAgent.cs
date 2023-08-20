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
        public List<RewardFunc> rewards;
        public GameObject body;
        
        //Agent's ridid body
        private bool done;
        protected float reward;
        private Dictionary<string, bool> firstTouch;
        private Dictionary<string, Sensor> sensorsMap;
        private List<Actuator> actuatorList;
        private List<Sensor> sensorList;

        private int numberOfSensors = 0;
        private int numberOfActuators = 0;
        private ModelMetadataLoader metadataLoader;

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

        public void RegisterRewardFunc(RewardFunc f)
        {
            if (rewards == null)
            {
                rewards = new List<RewardFunc>();
            }
            rewards.Add(f);
        }

        public bool UnregisterRewardFunc(RewardFunc f)
        {
            return rewards.Remove(f);
        }

        public override void Setup()
        {
            if (body == null)
            {
                body = gameObject;
            }
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
            doneSensor.isInput = false;
            doneSensor.SetAgent(this);
            sensorList.Add(doneSensor);
            sensorsMap[doneSensor.perceptionKey] = doneSensor;

            RewardSensor rewardSensor = GetComponent<RewardSensor>();
            rewardSensor.isInput = false;
            rewardSensor.SetAgent(this);
            sensorList.Add(rewardSensor);
            sensorsMap[rewardSensor.perceptionKey] = rewardSensor;

            IDSensor idSensor = GetComponent<IDSensor>();
            idSensor.isInput = false;
            idSensor.SetAgent(this);
            sensorList.Add(idSensor);
            sensorsMap[idSensor.perceptionKey] = idSensor;

            StepSensor stepSensor = GetComponent<StepSensor>();
            stepSensor.isInput = false;
            stepSensor.SetAgent(this);
            sensorList.Add(stepSensor);
            sensorsMap[stepSensor.perceptionKey] = stepSensor;
            numberOfSensors = 4;

            for (int i = 0; i < transform.childCount; i++) 
            {
                GameObject obj = transform.GetChild(i).gameObject;
                Sensor s = obj.GetComponent<Sensor>();
                if (s != null && s.isActive)
                {
                    sensorList.Add(s);
                    numberOfSensors++;
                    sensorsMap[s.perceptionKey] = s;
                }
                else
                {
                    if (obj.name == "sensors")
                    {
                        for (int j = 0; j < obj.transform.childCount; j++)
                        {
                            GameObject sobj = obj.transform.GetChild(j).gameObject;
                            Sensor s2 = sobj.GetComponent<Sensor>();
                            if (s2 != null)
                            {
                                sensorList.Add(s2);
                                numberOfSensors++;
                                sensorsMap[s2.perceptionKey] = s2;
                            }
                        }
                    }
                }

                Actuator a = obj.GetComponent<Actuator>();
                if (a != null)
                {
                    actuatorList.Add(a);
                    numberOfActuators++;
                }
                else
                {
                    if (obj.name == "actuators")
                    {
                        for (int j = 0; j < obj.transform.childCount; j++)
                        {
                            GameObject aobj = obj.transform.GetChild(j).gameObject;
                            Actuator a2 = aobj.GetComponent<Actuator>();
                            if (a2 != null)
                            {
                                actuatorList.Add(a2);
                                numberOfActuators++;
                            }
                        }
                    }
                }

                RewardFunc r = obj.GetComponent<RewardFunc>();
                if ( r != null)
                {
                    rewards.Add(r);
                }
                else
                {
                    if (obj.name == "rewards")
                    {
                        for (int j = 0; j < obj.transform.childCount; j++)
                        {
                            GameObject robj = obj.transform.GetChild(j).gameObject;
                            RewardFunc r2 = robj.GetComponent<RewardFunc>();
                            if (r2 != null)
                            {
                                rewards.Add(r2);
                            }
                        }
                    }
                }
            }

            if (sensorList.Count == 0) {
                Debug.LogWarning("Agent without sensors. Add at least one sensor for this agent to be able to perceive the world! GameObject: " + gameObject.name);
            }

            if (actuatorList.Count == 0) {
                Debug.LogWarning("Agent without actuators. Add at least one actuator for this agent to be able to change the world! GameObject: " + gameObject.name);
            }

            int totalNumberOfSensors = sensorList.Count;

            desc = new string[totalNumberOfSensors];
            types = new byte[totalNumberOfSensors];
            values = new string[totalNumberOfSensors];
            controlRequestor.SetAgent(this);
        
            foreach (RewardFunc r in rewards)
            {
                r.OnSetup(this);
            }

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
                throw new System.Exception("ai4u2unity connection error on agent id: " + ID);
            }
    
            setupIsDone = true;
        }

        public List<Actuator> Actuators
        {
            get 
            {
                return actuatorList;
            }
        }

        public List<Sensor> Sensors 
        {
            get
            {
                return sensorList;
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
