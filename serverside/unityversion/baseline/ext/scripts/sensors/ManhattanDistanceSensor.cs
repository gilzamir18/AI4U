using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u.ext
{
    public class ManhattanDistanceSensor : Sensor
    {
            public GameObject target;
            public GameObject reference;

            void Start() {
                if (reference == null) {
                    reference = agent.gameObject;
                }
            }


            public override float[] GetFloatArrayValue()
            {
                return new float[]{target.transform.localPosition.x - reference.transform.localPosition.x,
                                    target.transform.localPosition.y - reference.transform.localPosition.y,
                                    target.transform.localPosition.z - reference.transform.localPosition.z};
            }
    }
}