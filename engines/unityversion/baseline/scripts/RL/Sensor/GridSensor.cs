using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u
{
    public class GridSensor : Sensor
    {
        public GridBuilder grid;

        private HistoryStack<float> history;

        public override void OnSetup(Agent agent)
        {
            history = new HistoryStack<float>(grid.W * grid.H * stackedObservations);
            this.type = SensorType.sfloatarray;
            this.shape = new int[]{grid.W * grid.H * stackedObservations};
            agent.AddResetListener(this);
        }

        public override float[] GetFloatArrayValue()
        {
            for (int i = 0; i < grid.W; i++)
            {
                for (int j = 0; j < grid.H; j++)
                {
                    history.Push(grid.GetObjectType(i, j));
                }
            }
            return history.Values;
        }

        public override void OnReset(Agent agent)
        {
            history = new HistoryStack<float>(grid.W * grid.H * stackedObservations);
            if (agent.Brain.containsCommandField("goalDist"))
            {
                int d = agent.GetFieldArgAsInt("goalDist");
                grid.ResetGrid(d);
            }
            else
            {
                grid.ResetGrid();
            }

        }
    }
}