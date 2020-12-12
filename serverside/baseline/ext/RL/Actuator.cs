using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u.ext 
{
    public class Actuator : MonoBehaviour
    {

        public string actionName;
        public bool always;
        public DPRLAgent agent;
        
        public virtual void Act()
        {

        }

        public virtual void Reset()
        {
            
        }
    }
}