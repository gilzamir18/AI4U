using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u 
{
    public class StepSensor : Sensor
    {
        public StepSensor()
        {
            Start();
        }

        void Start()
        {
            perceptionKey = "steps";
            type = SensorType.sint;
            shape = new int[1]{1};
        }

        public override int GetIntValue()
        {
            return agent.NSteps;    
        }
    }
}