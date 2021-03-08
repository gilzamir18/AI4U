using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ai4u.ext;
using ai4u;

namespace ai4u.ext
{

    /// <summary>DPRLAgent - Dimentional Physical Reinforcement Learning Agent
    /// This class models an agent with physical rigidbody control in a tridimentional world. </summary> 
    public class DPRLAgent : RLAgent
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

        //Agent's ridid body
        private Rigidbody rBody;

        public Sensor[] sensors;
        public Actuator[]  actuators;

        //Current episode's number of steps.
        private int nSteps = 0;

        private Vector3 initialLocalPosition;

        private Dictionary<string, Actuator> actuatorsMap;
        private List<Actuator> alwaysActuators = new List<Actuator>();
        private Dictionary<string, Sensor> sensorsMap;

        void Awake() 
        {
            actuatorsMap = new Dictionary<string, Actuator>();
            sensorsMap = new Dictionary<string, Sensor>();
            if (sensors.Length == 0) {
                Debug.LogWarning("Agent without sensors. Add at least one sensor for this agent to be able to perceive the world! GameObject: " + gameObject.name);
            }
            foreach(Sensor s in sensors)
            {
                s.SetAgent(this);

                sensorsMap[s.perceptionKey] = s;
            }
            if (actuators.Length == 0) {
                Debug.LogWarning("Agent without actuators. Add at least one actuator for this agent to be able to change the world! GameObject: " + gameObject.name);
            }
            foreach(Actuator a in actuators) {
                if (a.always) {
                    alwaysActuators.Add(a);
                }
                actuatorsMap[a.actionName] = a;
            }
            rBody = GetComponent<Rigidbody>();
            initialLocalPosition = transform.position;
            ResetPlayer();
        }

        public bool TryGetSensor(string key, out Sensor s)
        {
            return sensorsMap.TryGetValue(key, out s);
        }

        public override void RequestDoneFrom(RewardFunc rf) {
            Done = true;
        }

        public override void StartData()
        {
            int numberOfFields = 2 + sensors.Length;
            desc = new string[numberOfFields];
            types = new byte[numberOfFields];
            values = new string[numberOfFields];
        }

        public override void AddReward(float v, RewardFunc from = null){
            if (doneAtNegativeReward && v < 0) {
                Done = true;
            }

            if (doneAtPositiveReward && v > 0) {
                Done = true;
            }
            base.AddReward(v, from);
        }

        public override void UpdatePhysics()
        {
            if (GetActionName() == "restart") {
                ResetPlayer();
            } else if (GetActionName() != null && actuatorsMap.ContainsKey(GetActionName())) 
            {
                if (!Done) {
                    Actuator a = actuatorsMap[GetActionName()];
                    if (!a.always) {
                        a.Act();
                    }
                }
            } else if (GetActionName() == "ResetReward") {
                reward = 0;
            } else if (GetActionName() == "SetMaxSteps") {
                this.MaxStepsPerEpisode = GetActionArgAsInt();
            }
            if (!Done) {
                int n = alwaysActuators.Count;
                if (n > 0) {
                    for (int i = 0; i < n; i++) {
                        alwaysActuators[i].Act();
                    }
                }
            }
        }

        private void ResetPlayer()
        {
            nSteps = 0;
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
            foreach(Actuator a in actuators) {
                a.Reset();
            }
            NotifyReset();
        }

        public override void UpdateState()
        {

            nSteps ++;

            if (MaxStepsPerEpisode > 0 && nSteps >= MaxStepsPerEpisode) {
                Done = true;
            }

            if (Done) {
                int n = alwaysActuators.Count;
                if (n > 0) {
                    for (int i = 0; i < n; i++) {
                        alwaysActuators[i].NotifyEndOfEpisode();
                    }
                }
            }

            SetStateAsBool(0, "done", Done);
            SetStateAsFloat(1, "reward", Reward);
            for (int i = 0; i < sensors.Length; i++) {
                switch(sensors[i].type)
                {
                    case SensorType.sfloatarray:
                        SetStateAsFloatArray(i+2, sensors[i].perceptionKey, sensors[i].GetFloatArrayValue());
                        break;
                    case SensorType.sfloat:
                        SetStateAsFloat(i+2, sensors[i].perceptionKey, sensors[i].GetFloatValue());
                        break;
                    case SensorType.sint:
                        SetStateAsInt(i+2, sensors[i].perceptionKey, sensors[i].GetIntValue());
                        break;
                    case SensorType.sstring:
                        SetStateAsString(i+2, sensors[i].perceptionKey, sensors[i].GetStringValue());
                        break;
                    case SensorType.sbool:
                        SetStateAsBool(i+2, sensors[i].perceptionKey, sensors[i].GetBoolValue());
                        break;
                    case SensorType.sbytearray:
                        SetStateAsByteArray(i+2, sensors[i].perceptionKey, sensors[i].GetByteArrayValue());
                        break;
                    default:
                        break;
                }
            }
            reward = 0;
        }
    }
}