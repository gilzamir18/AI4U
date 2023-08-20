using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u 
{
    public class Actuator : MonoBehaviour, IAgentResetListener
    {

        public string actionName;
        public BasicAgent agent;
        public bool isOutput = true;
        private bool actionDone = false;
        protected int[] shape;
        protected bool isContinuous;
		protected float[] rangeMin;
		protected float[] rangeMax;

		public float[] RangeMin
		{
			get 
			{
				return rangeMin;
			}
		}

		public float[] RangeMax 
		{
			get
			{
				return rangeMax;
			}
		}

        public int[] Shape
        {
            get
            {
                return shape;
            }
        }

        public bool IsContinuous
        {
            get
            {
                return isContinuous;
            }
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

        public virtual void OnSetup(Agent agent)
        {
            
        }

        public virtual void OnReset(Agent agent) {

        } 
    }
}