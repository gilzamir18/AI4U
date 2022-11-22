using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u {
    public class OrientationSensor : Sensor
    {
        public GameObject target;
        public GameObject reference = null;
        public float maxDistance = 100;

        private HistoryStack<float> history;

        public override void OnSetup(Agent agent) {

            this.agent = (BasicAgent)agent;

            if (reference == null) {
                reference = agent.gameObject;
            }

            type = SensorType.sfloatarray;
            shape = new int[1]{2};
            history = new HistoryStack<float>(shape[0]*stackedObservations);
        }

        public override void OnReset(Agent aget)
        {
            history = new HistoryStack<float>(shape[0]*stackedObservations);
        }
        
        public override float[] GetFloatArrayValue()
        {
            if (target == null){
                Debug.LogWarning("OrientationSensor error: target don't specified! Game Object: " + gameObject.name);
            }
        
            Vector3 f = reference.transform.forward;
            //Debug.Log("f = " + f.x  + ", " + f.y + ", " + f.z);
            Vector3 d = target.transform.position - reference.transform.position;
            //Debug.Log("d = " + d.x + ", " + d.y + ", " + d.z);
            float c = Vector3.Dot(f, d.normalized);
            //Debug.Log("c == " + c);
            history.Push(c);
            if (normalized)
            {
                history.Push(d.magnitude/maxDistance);
            } 
            else
            {
                history.Push(d.magnitude);
            }
            return history.Values;
        }
    }
}