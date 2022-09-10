using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ai4u;

namespace ai4u
{
    

    /// <summary>DPRLAgent - Dimentional Physical Reinforcement Learning Agent
    /// This class models an agent with physical rigidbody control in a tridimentional world. </summary> 
    [RequireComponent(typeof(ControlRequestor))]
    [RequireComponent(typeof(DoneSensor))]
    [RequireComponent(typeof(RewardSensor))]
    [RequireComponent(typeof(StepSensor))]
    [RequireComponent(typeof(IDSensor))]
    public class BasicAgent : Agent
    {

        /// <summary> Ramdom positions contains all positions where the agent can be placed in the environment. 
        /// All positions are equally likely.</summary>
        public GameObject[] randomPositions;
    
        ///<summary> <code>doneAtNegativeReward</code> ends the simulation whenever the agent receives a negative reward.</summary>
        public bool doneAtNegativeReward = true;
        ///<summary> <code>doneAtPositiveReward</code> ends the simulation whenever the agent receives a positive reward.</summary>
        public bool doneAtPositiveReward = false;
        
        ///<summary>The maximum number of steps per episode.</summary>
        public int MaxStepsPerEpisode = 0;

        public bool childrenSensors = true;

        //Agent's ridid body
        private Rigidbody rBody;

        public Sensor[] sensors;
        public Actuator[]  actuators;

        private Vector3 initialLocalPosition;

        private bool done;
        protected float reward;
        private ControlRequestor controlRequestor;
        private Dictionary<string, bool> firstTouch;

        private Dictionary<string, Sensor> sensorsMap;

        private List<Actuator> actuatorList;
        private List<Sensor> sensorList;

        public int rewardIndex;

        private int numberOfSensors = 0;
        private int numberOfActuators = 0;

        public int Id
        {
            get
            {
                return rewardIndex;
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

        public bool TryGetSensor(string key, out Sensor s)
        {
            return sensorsMap.TryGetValue(key, out s);
        }

        public virtual void RequestDoneFrom(RewardFunc rf) {
            Done = true;
        }

        public override void Setup()
        {
            setupIsDone = false;
            controlRequestor = GetComponent<ControlRequestor>();
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            if (sensors.Length == 0) {
                Debug.LogWarning("Agent without sensors. Add at least one sensor for this agent to be able to perceive the world! GameObject: " + gameObject.name);
            }

            actuatorList = new List<Actuator>();
            sensorList = new List<Sensor>();
            sensorsMap = new Dictionary<string, Sensor>();

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

            if (childrenSensors)
            {
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

            if (actuators.Length == 0) {
                Debug.LogWarning("Agent without actuators. Add at least one actuator for this agent to be able to change the world! GameObject: " + gameObject.name);
            }
            foreach(Actuator a in actuators) {
                if (a == null)
                {
                    Debug.LogWarning("Actuator is null!!! Set a valid actuator in agent's actuators list!");
                }
                actuatorList.Add(a);
                numberOfActuators++;
            }

            rBody = GetComponent<Rigidbody>();
            initialLocalPosition = transform.position;

            int totalNumberOfSensors = sensorList.Count;

            desc = new string[totalNumberOfSensors];
            types = new byte[totalNumberOfSensors];
            values = new string[totalNumberOfSensors];
            controlRequestor.SetAgent(this);

            RequestCommand request = new RequestCommand(3);
            request.SetMessage(0, "__target__", ai4u.Brain.STR, "envcontrol");
            request.SetMessage(1, "max_steps", ai4u.Brain.INT, MaxStepsPerEpisode);
            request.SetMessage(2, "id", ai4u.Brain.STR, ID);

            var cmds = controlRequestor.RequestEnvControl(request);
            if (cmds == null)
            {
                throw new System.Exception("ai4u2unity connection error!");
            }
            setupIsDone = true;
            foreach (Sensor sensor in sensorList)
            {
                sensor.OnSetup(this);
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

        public override void  ApplyAction()
        {
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
                else
                {
                    actuatorList[i].NotifyEndOfEpisode();
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
            ResetPlayer();
        }

        private bool checkFirstTouch(string tag){
            if (firstTouch.ContainsKey(tag)) {
                return false;
            } else {
                firstTouch[tag] = false;
                return true;
            }
        }

        private void ResetPlayer()
        {
            nSteps = 0;
            reward = 0;
            Done = false;
    
            firstTouch = new Dictionary<string, bool>(); 
            
            transform.rotation = Quaternion.identity;
            if (initialLocalPosition == null) {
                initialLocalPosition = transform.localPosition;
            }
            if (rBody != null) {
                rBody.velocity = Vector3.zero;
                rBody.angularVelocity = Vector3.zero;
            }
            if (randomPositions.Length > 0) {
                int idx = (int)Random.Range(0, randomPositions.Length-1 + 0.5f);
                transform.localPosition = randomPositions[idx].transform.localPosition;
            } else {
                transform.localPosition = initialLocalPosition;
            }
            foreach(Actuator a in actuatorList) {
                a.Reset();

            }
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

        public override void UpdateState()
        {
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
            reward = 0;
        }
    }
}
