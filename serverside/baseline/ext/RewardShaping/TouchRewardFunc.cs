using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  ai4u.ext
{
    public class TouchRewardFunc : RewardFunc
    {
        public int maxNumberOfTheRewards = -1;
        public float rewardValue = 1.0f;
        public bool triggerOnStay = true;
        private int[] counter;
        private Collider myCollider;

        void Awake() {
            counter = new int[agents.Length]; 
            myCollider = GetComponent<Collider>();
            foreach(Agent agent in agents) {
                agent.AddResetListener(this);
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

        public override void OnReset(Agent agent) {
            counter = new int[agents.Length]; 
        }

        private void Check(Collider collider)
        {
            RLAgent agent = collider.gameObject.GetComponent<RLAgent>();
            if ( (counter[agent.Id] < maxNumberOfTheRewards || maxNumberOfTheRewards < 0)  )
            {   
                counter[agent.Id]++;
                agent.AddReward(rewardValue, this);
            }
        }
    }
}
