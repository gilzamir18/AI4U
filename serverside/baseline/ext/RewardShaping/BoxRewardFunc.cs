using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u.ext
{
    public class BoxRewardFunc : RewardFunc
    {
        public int maxNumberOfTheRewards = 1;
        public float rewardValue = 1.0f;
        public bool checkInside = true;
        public bool triggerOnStay = true;

        public bool triggerOnExit = false;

        private int[] counter;
        private Collider myCollider;

        void Awake() {
            counter = new int[agents.Length]; 
            myCollider = GetComponent<Collider>();
            foreach(Agent agent in agents) {
                agent.AddResetListener(this);
            }
            if (triggerOnStay) {
                triggerOnExit = false;
            }
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
                RLAgent agent = collider.gameObject.GetComponent<RLAgent>();
                agent.boxListener(this);
                counter[agent.Id]++;
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
                RLAgent agent = other.gameObject.GetComponent<RLAgent>();
                agent.boxListener(this);
                counter[agent.Id]++;
                agent.AddReward(rewardValue, this);
            }
        }

        public override void OnReset(Agent agent) {
            counter = new int[agents.Length]; 
        }

        private void Check(Collider collider)
        {
            RLAgent agent = collider.gameObject.GetComponent<RLAgent>();
            agent.boxListener(this);
            if ( counter[agent.Id] < maxNumberOfTheRewards || maxNumberOfTheRewards < 0  )
            {
                if (checkInside)
                {
                    Collider[] colliders = Physics.OverlapBox(myCollider.bounds.center, myCollider.bounds.extents, transform.rotation);

                    int idx = System.Array.IndexOf(colliders, collider);
                    if (idx >= 0)
                    {
                        counter[agent.Id]++;
                        agent.AddReward(rewardValue, this);
                    }
                } else  {
                    counter[agent.Id]++;
                    agent.AddReward(rewardValue, this);
                }
            }
        }
    }
}