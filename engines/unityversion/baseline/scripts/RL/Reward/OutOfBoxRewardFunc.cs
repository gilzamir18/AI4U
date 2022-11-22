using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u
{
    public class OutOfBoxRewardFunc : RewardFunc
    {
        public float rewardValue = 1.0f;
        public bool checkInside = false;
        public GameObject target;

        private float acmReward = 0.0f;
        private Collider myCollider;
        private BasicAgent agent;

        public override void OnSetup(Agent agent) 
        {
            this.agent = (BasicAgent) agent;
            if (target == null)
            {
                target = gameObject;
            }
            myCollider = target.GetComponent<Collider>();
            agent.AddResetListener(this);
            acmReward = 0;
        }


        public override void OnUpdate()
        {
            Check(agent.GetComponent<Collider>());     
            if (acmReward != 0)
            {
                agent.AddReward(acmReward, this);
                acmReward = 0;
            }
        }

        public override void OnReset(Agent agent) {
            acmReward = 0;
        }

        private void Check(Collider collider)
        {
            Collider[] colliders = Physics.OverlapBox(myCollider.bounds.center, myCollider.bounds.extents, transform.rotation);

            int idx = System.Array.IndexOf(colliders, collider);
            if (!checkInside)
            {
                if (idx < 0)
                {
                        acmReward += rewardValue;
                }
            }
            else
            {
                if (idx >= 0)
                {
                        acmReward += rewardValue;
                }
            }
        }
    }
}
