﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u 
{
    public class Actuator : MonoBehaviour, IAgentResetListener
    {

        public string actionName;
        public BasicAgent agent;
        public ActionReward actionReward;
        private bool actionDone = false;

        public virtual void NotifyEndOfEpisode() 
        {

        }

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

        public virtual void Reset()
        {
            
        }

        public virtual void OnReset(Agent agent) {

        } 
    }
}