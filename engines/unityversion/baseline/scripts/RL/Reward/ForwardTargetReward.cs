 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ai4u {
    public class ForwardTargetReward: ActionReward 
    {
        public float penalityByAction = 0.0f;
        public GameObject target;

        
        private BasicAgent agent;
        private float minDist = -1.0f;

        public void Start() {
            if (target == null) {
                Debug.LogWarning("ForwardTargetReward error: target don't specified!");
            }
            agent = GetComponent<BasicAgent>();
            agent.AddResetListener(this);
            minDist = -1;
        }

        public override void RewardFrom(string actionName, BasicAgent agent) {        
            float d = Vector3.Distance(transform.localPosition, target.transform.localPosition);
            
            if (minDist >= 0)
                agent.AddReward(Math.Max(minDist - d, 0) - penalityByAction);
            
            if (minDist < 0 || minDist > d) {
                minDist = d;
            }
        }

        public override void OnReset(Agent agent){
            minDist = -1;
        }
    }
}
