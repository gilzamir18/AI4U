using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ai4u;

namespace ai4u.ext {
    public class RewardFunc : MonoBehaviour, IAgentResetListener
    {
        public RLAgent[] agents;
    
        public virtual void OnReset(Agent agent) {

        } 
    }
}