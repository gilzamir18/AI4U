using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ai4u;

namespace ai4u {
    public class RewardFunc : MonoBehaviour, IAgentResetListener
    {
        public bool causeEpisodeToEnd = false;
        
        public virtual void OnSetup(Agent agent)
        {

        }

        public virtual void OnUpdate()
        {

        }

        public virtual void OnReset(Agent agent) {

        }
    }
}