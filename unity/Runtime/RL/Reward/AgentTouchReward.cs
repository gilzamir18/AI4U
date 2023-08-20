using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u
{
    public class AgentTouchReward : RewardFunc
    {
        public List<string> rewardableTags;
        public float reward = 0.01f;

        private BasicAgent agent;
        private float acmReward = 0.0f;

        public override void OnSetup(Agent agent)
        {
            this.agent = (BasicAgent) agent;
        }

        public override void OnUpdate()
        {
            this.agent.AddReward(acmReward, this);
            acmReward = 0.0f;
        }

        void OnCollisionEnter(Collision col)
        {
            if (rewardableTags.Contains(col.gameObject.tag ))
            {
                acmReward += reward;
            }
        }

        void OnTriggerEnter(Collider collider)
        {
            if (rewardableTags.Contains(collider.gameObject.tag))
            {
                acmReward += reward;
            }
        }
    }
}