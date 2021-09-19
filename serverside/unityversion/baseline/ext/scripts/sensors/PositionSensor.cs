using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u.ext
{
    public class PositionSensor : Sensor
    {

        public GameObject target;

        public PositionSensor()
        {
        }

        public override float[] GetFloatArrayValue()
        {
            return new float[]{target.transform.localPosition.x, target.transform.localPosition.y, target.transform.localPosition.z};
        }
    }
}