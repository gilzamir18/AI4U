using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u.ext
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
        public SensorType type;
        public bool isState;
        public int[] shape;

        protected DPRLAgent agent;

        public void SetAgent(DPRLAgent own)
        {
            agent = own;
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