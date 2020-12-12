using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u.ext
{
    public class ManhattanDistanceSensor : Sensor
    {
            public GameObject target;
            public override float[] GetFloatArrayValue()
            {
                return new float[]{target.transform.position.x - agent.transform.position.x,
                                    target.transform.position.y - agent.transform.position.y,
                                    target.transform.position.z - agent.transform.position.z};
            }
    }
}