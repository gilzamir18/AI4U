using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u
{
    public class RewardSensor : Sensor
    {
        public override void OnSetup(Agent agent)
        {
            perceptionKey = "reward";
            type = SensorType.sfloat;
            shape = new int[1]{1};
        }

        public override float GetFloatValue()
        {
            return agent.Reward;
        }
    }
}
