using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u
{
    public class BoxRewardFunc : RewardFunc
    {
        public int maxNumberOfTheRewards = 1;
        public float rewardValue = 1.0f;
        public bool checkInside = true;
        public bool triggerOnStay = true;
        public GameObject target;

        public bool triggerOnExit = false;
        private float acmReward = 0.0f;
        private int counter;
        private Collider myCollider;

        private BasicAgent agent;

        public override void OnSetup(Agent agent) 
        {
            this.agent = (BasicAgent) agent;
            counter = 0; 
            if (target == null)
            {
                target = gameObject;
            }
            myCollider = target.GetComponent<Collider>();
            agent.AddResetListener(this);
            if (triggerOnStay) {
                triggerOnExit = false;
            }
            acmReward = 0;
        }

        void OnTriggerStay(Collider collider) {
            if (triggerOnStay) {
                Check(collider);
            }
        }

        void OnTriggerEnter(Collider collider) {
            if (!triggerOnStay && !triggerOnExit) {
                Check(collider);
            }
        }


        void OnTriggerExit(Collider collider) {
            if (triggerOnExit) {
                agent.boxListener(this);
                counter++;
                agent.AddReward(rewardValue, this);
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


        void OnCollisionExit(Collision other) {
            if (triggerOnExit) {
                agent.boxListener(this);
                counter++;
                agent.AddReward(rewardValue, this);
            }
        }

        public override void OnUpdate()
        {
            if (acmReward != 0)
            {
                agent.AddReward(acmReward, this);
                acmReward = 0;
            }
        }

        public override void OnReset(Agent agent) {
            counter = 0; 
            acmReward = 0;
        }

        private void Check(Collider collider)
        {
            agent.boxListener(this);
            if ( counter < maxNumberOfTheRewards || maxNumberOfTheRewards < 0  )
            {
                if (checkInside)
                {
                    Collider[] colliders = Physics.OverlapBox(myCollider.bounds.center, myCollider.bounds.extents, transform.rotation);

                    int idx = System.Array.IndexOf(colliders, collider);
                    if (idx >= 0)
                    {
                        counter++;
                        acmReward += rewardValue;
                    }
                } else  {
                    counter++;
                    acmReward += rewardValue;
                }
            }
        }
    }
}