using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u.ext 
{
    public class AutoFallReward : RewardFunc
    {

        public float threshold = 0.0f;
        public bool stopRewardingAfterFall = true;
        public float rewardValue = -1.0f;

        private RLAgent agent;
        private bool fall = false;
        

        void Start()
        {
            fall = false;
            agent = GetComponent<RLAgent>();
            agent.AddResetListener(this);
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