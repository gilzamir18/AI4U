using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u 
{
    public class Actuator : MonoBehaviour, IAgentResetListener
    {

        public string actionName;
        public BasicAgent agent;
        private bool actionDone = false;

        public bool ActionDone{
            get {
                return actionDone;
            }

            set {
                actionDone = value;
            }
        }
        
        public virtual void Act()
        {

        }

        public virtual void OnSetup(Agent agent)
        {
            
        }

        public virtual void OnReset(Agent agent) {

        } 
    }
}