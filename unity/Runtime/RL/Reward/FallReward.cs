using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u
{
    public class FallReward : RewardFunc
    {

        public float threshold = 0.0f;
        public bool stopRewardingAfterFall = true;
        public float rewardValue = -1.0f;

        private BasicAgent agent;
        private bool fall = false;

        public override void OnSetup(Agent agent)
        {
            fall = false;
            this.agent = (BasicAgent) agent;
            this.agent.AddResetListener(this);
        }

        // Update is called once per frame
        void Update()
        {
                if (agent != null) 
                {
                    if (transform.localPosition.y < threshold) 
                    {
                        if (!stopRewardingAfterFall) {
                            agent.AddReward(rewardValue, this);
                        } else if (!fall) {
                            agent.AddReward(rewardValue, this);
                        }
                        fall = true;
                    }
                }
        }

        public override void OnReset(Agent agent) {
            fall = false;
        }
    }

}