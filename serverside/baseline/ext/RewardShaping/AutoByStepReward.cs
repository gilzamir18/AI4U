using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u.ext {
    public class AutoByStepReward : RewardFunc
    {

        private RLAgent agent;
        private float sumOfRewards = 0.0f;
        
        public int maxRewardByEpisode = 0;
        public float reward = -1;
        
        // Start is called before the first frame update
        void Start()
        {
            agent = GetComponent<RLAgent>();
            agent.AddResetListener(this);
            sumOfRewards = 0;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (agent != null) {
                if (maxRewardByEpisode <= 0 || sumOfRewards < maxRewardByEpisode) {
                    sumOfRewards += System.Math.Abs(reward);
                    agent.AddReward(reward, this);
                }
            } else {
                Debug.LogWarning("Warning: an agent was not specified or this game object is not an agent!!!");
            }
        }

        public override void OnReset(Agent agent)
        {
            sumOfRewards = 0;
        }
    }
}