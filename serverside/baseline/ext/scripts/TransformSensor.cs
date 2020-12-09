using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u.ext
{
    public class TransformSensor : Sensor
    {

        public GameObject target;

        public override float[] GetFloatArrayValue()
        {
            return new float[]{target.transform.position.x, target.transform.position.y, target.transform.position.z,
                                target.transform.rotation.eulerAngles.x, target.transform.rotation.eulerAngles.y, target.transform.rotation.eulerAngles.z,
                                target.transform.localScale.x, target.transform.localScale.y, target.transform.localScale.z};
        }
    }
}