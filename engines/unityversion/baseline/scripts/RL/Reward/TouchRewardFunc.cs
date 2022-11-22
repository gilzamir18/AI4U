using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  ai4u
{
    public class TouchRewardFunc : RewardFunc
    {
        public int maxTouch = -1;
        public float painForOverTouch = 0;
        public float painForViolatinPrecondition = 0;
        public float rewardValue = 1.0f;
        public TouchRewardFunc precondition = null;
        public MultTouchPrecondition multiplePreconditions = null;
        public bool triggerOnStay = true;
        public bool allowNext;
        public GameObject target;

        private int counter;
        private bool touched;
        private Collider myCollider;
        private float acmReward = 0.0f;
        private BasicAgent agent;

        public override void OnSetup(Agent agent) {
            this.agent = (BasicAgent) agent;
            counter = 0;
            touched = false;
            acmReward = 0;
            if (target == null)
            {
                target = gameObject;
            }
            myCollider = target.GetComponent<Collider>();
            agent.AddResetListener(this);
        }


        public override void OnUpdate() 
        {
            if (acmReward != 0)
            {
                agent.AddReward(acmReward, this);
                acmReward = 0;
            }
        }

        void OnTriggerStay(Collider collider) {
            if (triggerOnStay) {
                Check(collider);
            }
        }

        void OnTriggerEnter(Collider collider) {
            if (!triggerOnStay) {
                Check(collider);
            }
        }

        void OnCollisionStay(Collision other) {
            if (triggerOnStay) {
                Check(other.collider);
            }         
        }

        void OnCollisionEnter(Collision other) {
            if (!triggerOnStay) {
                Check(other.collider);
            }
        }

        public bool wasTouched(BasicAgent agent) {
            return touched;
        }

        public override void OnReset(Agent agent) {
            counter = 0;
            touched = false;
            acmReward = 0;
        }

        public void Check(Collider collider)
        {
            if (agent.gameObject.GetComponent<Collider>() == collider) {
                touched = true;
                agent.touchListener(this);  

                if (precondition != null) {
                    if (!precondition.allowNext || !precondition.wasTouched(agent)){
                        agent.AddReward(-painForViolatinPrecondition, this);
                        agent.PreconditionFailListener(this, precondition);
                        return;
                    }
                }

                if (multiplePreconditions != null)
                {
                    if (!multiplePreconditions.allowNext || !multiplePreconditions.wasTouched(agent))
                    {
                        agent.AddReward(-painForViolatinPrecondition, this);
                        agent.PreconditionFailListener(this, multiplePreconditions);
                        return;
                    }
                }

                if ( (counter < maxTouch) || (maxTouch < 0)  )
                {
                    counter++;
                    acmReward += rewardValue;
                } else if (maxTouch >= 0 && counter >= maxTouch) {
                    acmReward -= painForOverTouch;
                }
            }
        }
    }
}
