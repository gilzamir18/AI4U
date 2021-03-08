using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u.ext {
    public class TargetDiscrepancySensor : Sensor
    {
        public GameObject target;
        public GameObject reference = null;

        void Start() {

            if (agent == null) {
                Debug.LogWarning("TargetDiscrepancySensor error: agent dont't specified. Game object: " + gameObject.name);
            }

            if (reference == null) {
                reference = agent.gameObject;
            }
        }
        
        public override float[] GetFloatArrayValue()
        {
            if (target == null){
                Debug.LogWarning("TargetDiscrepancySensor error: target don't specified! Game Object: " + gameObject.name);
            }
        
            Vector3 f = reference.transform.forward;
            //Debug.Log("f = " + f.x  + ", " + f.y + ", " + f.z);
            Vector3 d = target.transform.position - reference.transform.position;
            d = Vector3.Normalize(d);
            //Debug.Log("d = " + d.x + ", " + d.y + ", " + d.z);
            float c = Vector3.Dot(f, d);
            //Debug.Log("c == " + c);
            return new float[]{c, target.transform.position.y - reference.transform.position.y};
        }
    }
}