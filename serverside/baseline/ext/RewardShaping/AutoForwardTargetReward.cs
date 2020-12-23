using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ai4u.ext {
    public class AutoForwardTargetReward: ActionReward 
    {
        public float penalityByAction = 0.0f;
        public GameObject target;
        
        
        private float minDist = 1e17f;

        public void Start() {
            if (target == null) {
                Debug.LogWarning("ForwardTargetReward error: target don't specified!");
            }
            minDist = Vector3.Distance(transform.localPosition, target.transform.localPosition);
        }

        public override void RewardFrom(string actionName, RLAgent agent) {        
            float d = Vector3.Distance(transform.localPosition, target.transform.localPosition);
            
            agent.AddReward(Math.Max(minDist - d, 0) - penalityByAction);
            
            if (minDist > d) {
                d = minDist;
            }
        }

        public override void OnReset(Agent agent){
            minDist = Vector3.Distance(transform.localPosition, target.transform.localPosition);
        }
    }
}
