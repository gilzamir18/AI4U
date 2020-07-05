using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using unityremote;

namespace unityremote.ext {
    public class RewardFunc : MonoBehaviour, IAgentResetListener
    {
        public RLAgent[] agents;
    
        public virtual void OnReset(Agent agent) {

        } 
    }
}