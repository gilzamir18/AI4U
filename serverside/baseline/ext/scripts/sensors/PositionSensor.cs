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
            return new float[]{target.transform.position.x, target.transform.position.y, target.transform.position.z};
        }
    }
}