using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ai4u;

namespace ai4u {
    public class RewardFunc : MonoBehaviour, IAgentResetListener
    {
        public BasicAgent[] agents;

        public bool causeEpisodeToEnd = false;
    
        public virtual void OnReset(Agent agent) {

        } 
    }
}