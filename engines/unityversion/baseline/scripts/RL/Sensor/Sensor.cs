using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u
{
    public enum SensorType {
        sfloat,
        sstring,
        sbool,
        sint,
        sbytearray,
        sfloatarray,
        sintarray
    }

    public class Sensor : MonoBehaviour, IAgentResetListener
    {
        public string perceptionKey;
        public int stackedObservations = 1;
        [Tooltip("If active, the sensor is processed and sent to the controller, otherwise it is as if it does not exist.")]
        public bool isActive = true;
        [Tooltip("Determines whether observation data must be transformed to stay within a certain range before being sent to the controller.")]
        public bool  normalized = true;
        public bool resetable = true;

        protected SensorType Type;
        protected bool IsState;
        protected int[] Shape;
        [Tooltip("The maximum number of observations to be appended to this sensor.")]
        protected BasicAgent agent;
        

        public SensorType type
        {
            get 
            {
                return Type;
            }

            set
            {
                Type = value;
            }
        }

        public bool isState
        {
            get
            {
                return IsState;
            }

            set
            {
                IsState = value;
            }

        }

        public int[] shape 
        {
            get
            {
                return Shape;
            }

            set
            {
                Shape = value;
            }
        }

        public void SetAgent(BasicAgent own)
        {
            agent = own;
        }

        public virtual void OnSetup(Agent agent)
        {
        }

        public virtual float GetFloatValue() {
            return 0;
        }

        public virtual string GetStringValue() {
            return string.Empty;
        }

        public virtual bool GetBoolValue() {
            return false;
        }

        public virtual byte[] GetByteArrayValue() {
            return null;
        }

        public virtual int GetIntValue() {
            return 0;
        }

        public virtual int[] GetIntArrayValue() {
            return null;
        }

        public virtual float[] GetFloatArrayValue() {
            return null;
        }

        public virtual void OnReset(Agent agent) {

        }
    }
}