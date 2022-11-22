using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u {
    public class ActionSensor : Sensor
    {
        public string actionName;
        public int actionSize;    
        private HistoryStack<float> history;
        private float[] previewsAction;

        public override void OnSetup(Agent agent) {
            this.agent = (BasicAgent)agent;

            type = SensorType.sfloatarray;
            shape = new int[1]{actionSize};
            previewsAction = new float[shape[0]];
            history = new HistoryStack<float>(shape[0]*stackedObservations);
        }

        public override void OnReset(Agent aget)
        {
            history = new HistoryStack<float>(shape[0]*stackedObservations);
            previewsAction = new float[shape[0]];
        }
        
        public override float[] GetFloatArrayValue()
        {             
            if (agent.GetActionName()==actionName)
            {
                previewsAction = agent.GetActionArgAsFloatArray();
                for (int i = 0; i < actionSize; i++)
                {
                    history.Push(previewsAction[i]);
                }
            }

            return history.Values;
        }
    }
}