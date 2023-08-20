using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u
{
    public class PositionSensor : Sensor
    {

        public GameObject target;
        private HistoryStack<float> stack;
    
        public float xMin = 0;
        public float xMax = 500;

        public float yMin = 0;
        public float yMax = 500;

        public float zMin = 0;
        public float zMax = 500;

        private float Preprocessing(float v, float min, float max)
        {
            return (v - min)/(max-min);
        }

        public override void OnSetup(Agent agent)
        {
            this.type = SensorType.sfloatarray;
            this.shape = new int[1]{3};
            this.agent = (BasicAgent) agent;
            stack = new HistoryStack<float>(3 * stackedObservations);
        }

        public override void OnReset(Agent agent)
        {
            stack = new HistoryStack<float>(3 * stackedObservations);
            GetFloatArrayValue();
        }

        public override float[] GetFloatArrayValue()
        {
            stack.Push(Preprocessing(target.transform.localPosition.x, xMin, xMax));
            stack.Push(Preprocessing(target.transform.localPosition.y, yMin, yMax));
            stack.Push(Preprocessing(target.transform.localPosition.z, zMin, zMax));
            return stack.Values;
        }
    }
}
