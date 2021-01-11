using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u.ext
{
    public class ForwardSensor : Sensor
    {

        public GameObject target;

        public ForwardSensor()
        {
        }

        public override float[] GetFloatArrayValue()
        {
            return new float[]{target.transform.forward.x, target.transform.forward.y, target.transform.forward.z};
        }
    }
}
