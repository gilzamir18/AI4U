using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u 
{
    public class IDSensor : Sensor
    {
        public override void OnSetup(Agent agent)
        {
            perceptionKey = "id";
            type = SensorType.sstring;
            shape = new int[1]{1};
        }

        public override string GetStringValue()
        {
            return agent.ID;
        }
    }
}