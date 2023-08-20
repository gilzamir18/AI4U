using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u
{
    public class GroundStatusSensor : Sensor
    {
        private HistoryStack<float> stack;
        // Start is called before the first frame update
        public LayerMask groundMask;
            public float groundCheckDistance = 1f;

        public override void OnSetup(Agent agent)
        {
            this.type = SensorType.sfloatarray;
            this.shape = new int[1]{1};
            this.agent = (BasicAgent) agent;
            stack = new HistoryStack<float>(1 * stackedObservations);
        }

        public override float[] GetFloatArrayValue()
        {
            bool isGrounded = Physics.Raycast(agent.gameObject.transform.position, Vector3.down, groundCheckDistance, groundMask);
            stack.Push(isGrounded? 1.0f : -1.0f);
            return stack.Values;
        }
    }
}