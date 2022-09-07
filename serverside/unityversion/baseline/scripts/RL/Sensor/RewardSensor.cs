using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u
{
    public class RewardSensor : Sensor
    {
        public RewardSensor()
        {
            Start();
        }

        void Start()
        {
            perceptionKey = "reward";
            type = SensorType.sfloat;
            shape = new int[0];
        }

        public override float GetFloatValue()
        {
            return agent.Reward;
        }
    }
}
