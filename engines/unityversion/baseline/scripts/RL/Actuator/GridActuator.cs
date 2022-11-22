using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u
{
    public class GridActuator : Actuator
    {
        public GridBuilder grid;

        public GridActuator()
        {
            shape = new int[]{5};
            isContinuous = false;
        }

        public override void OnSetup(Agent agent)
        {
            agent.AddResetListener(this);
        }

        public override void Act()
        {
            int action = agent.GetActionArgAsInt();
            grid.Move(action);
        }

        public override void OnReset(Agent agent)
        {
        }
    }
}