using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u
{
    public class SequentialActuator : Actuator
    {
        public List<Actuator> actuators;

        public override void Act()
        {
            foreach(Actuator a in actuators)
            {
                a.Act();
            }
        }

        public override void OnSetup(Agent agent)
        {
            foreach(Actuator a in actuators)
            {
                a.OnSetup(agent);
            }
        }
    }
}