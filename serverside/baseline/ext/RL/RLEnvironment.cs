using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ai4u;
using ai4u.ext;


namespace ai4u.ext {
    public class RLEnvironment : RLAgent
    {
        // Start is called before the first frame update
        
        public RLAgent[] agents;

        void Awake() {
            this.numberOfFields = agents.Length * this.numberOfFields;
        }

        public override void ApplyAction()
        {
            string actionname = GetActionName();
            string[] tokens = actionname.Split(':');
            int agent = int.Parse(tokens[0]);
            if (agent >= agents.Length)
            {
                agents[agent].Act(actionname);
                string[] states = agents[agent].GetStateList();
                object[] values = agents[agent].GetStateValue();
                for (int j = 0; j < values.Length; j++) {
                    int k = agent*values.Length + j;
                    if (values[j] is int){
                        SetStateAsInt(k, agent + ":" + states[j], (int)values[j]);
                    } if (values[j] is byte[]) {
                        SetStateAsByteArray(k, agent + ":" + states[j], (byte[])values[j]);
                    } else if (values[j] is float[]) {
                        SetStateAsFloatArray(k, agent + ":" + states[j], (float[])values[j]);
                    } else if (values[j] is float) {
                        SetStateAsFloat(k, agent + ":" + states[j], (float)values[j]);
                    } else if (values[j] is string) {
                        SetStateAsString(k, agent + ":" + states[j], (string)values[j]);
                    } else if (values[j] is bool) {
                        SetStateAsBool(k, agent + ":" + states[j], (bool)values[j]);
                    }
                }
            } else {
                throw new System.Exception("AI4UError: Agent not found: " + agent);
            }
        }
    }
}