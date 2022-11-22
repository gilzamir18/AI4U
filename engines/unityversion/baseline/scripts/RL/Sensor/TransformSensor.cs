using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u
{
    public class TransformSensor : Sensor
    {
        public GameObject target;

        private HistoryStack<float> stack;

        public override void OnSetup(Agent agent)
        {
            perceptionKey = "transform";
            type = SensorType.sfloatarray;
            shape = new int[1]{9};
            stack = new HistoryStack<float>(shape[0] * stackedObservations);
        }

        public override void OnReset(Agent agent)
        {
            stack = new HistoryStack<float>(shape[0] * stackedObservations);
        }

        public override float[] GetFloatArrayValue()
        {
            this.stack.Values = new float[]{target.transform.localPosition.x, target.transform.localPosition.y, target.transform.localPosition.z,
                                target.transform.rotation.eulerAngles.x, target.transform.rotation.eulerAngles.y, target.transform.rotation.eulerAngles.z,
                                target.transform.localScale.x, target.transform.localScale.y, target.transform.localScale.z};
            return this.stack.Values;
        }
    }
}
