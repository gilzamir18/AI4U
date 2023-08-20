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
        [Tooltip("'perceptionKey' represents a unique key for an identifiable sensor component, which will be used by the controller to retrieve information from the sensor.")]
        public string perceptionKey;
        [Tooltip("The 'stackedObservation' property represents a collection of observations that have been stacked together in a specific format, where it allows multiple observations to be processed and analyzed as a single input.")]
        public int stackedObservations = 1;
        [Tooltip("If active, the sensor is processed and sent to the controller, otherwise it is as if it does not exist.")]
        public bool isActive = true;
        [Tooltip("The 'resetable' property indicates whether the associated component should be reset at the beginning of each episode, whenever the associated agent is reset.")]
        public bool resetable = true;
        [Tooltip(" The 'isInput' property is a boolean flag that indicates whether the associated component is an agent's input or not.")]
        public bool isInput = false;
        protected SensorType Type;
        protected bool IsState;
        protected int[] Shape;
        protected BasicAgent agent;
        protected bool  normalized = true;
        protected float rangeMin = 0.0f;
        protected float rangeMax = 1.0f;


        public bool Normalized
        {
            get
            {
                return normalized;
            }
        }


        public float RangeMin 
        {
            get
            {
                return rangeMin;
            }
        }

        public float RangeMax 
        {
            get
            {
                return rangeMax;
            }
        }

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