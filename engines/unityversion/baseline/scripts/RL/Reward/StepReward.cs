using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u {
    public class StepReward : RewardFunc
    {

        private BasicAgent agent;
        private float sumOfRewards = 0.0f;
        public bool ignoreX = false;
        public bool ignoreY = true;
        public bool ignoreZ = false;
        public int maxRewardByEpisode = 0;
        public float reward = -1;
        public float minimumDistance = 0.2f;

        private Vector3 prevPosition;
        private float fx, fy, fz;
        private Vector3 f;
        // Start is called before the first frame update
        void Start()
        {
            if (ignoreX) {
                fx = 0;
            } else {
                fx = 1;
            } 

            if (ignoreY) {
                fy = 0;
            } else {
                fy = 1;
            }

            if (ignoreZ) {
                fz = 0;
            } else {
                fz = 1;
            }

            f = new Vector3(fx, fy, fz);
            
            prevPosition = transform.localPosition;
            agent = GetComponent<BasicAgent>();
            agent.AddResetListener(this);
            sumOfRewards = 0;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (agent != null) {
                if (maxRewardByEpisode <= 0 || sumOfRewards < maxRewardByEpisode) {
                    Vector3 p = Vector3.Scale(transform.localPosition, f);
                    Vector3 pp = Vector3.Scale(prevPosition, f);
                    float dist = Vector3.Distance(p, pp);
                    if (dist >= minimumDistance) {
                        sumOfRewards += System.Math.Abs(reward);
                        agent.AddReward(reward, this);
                    }
                }
            } else {
                Debug.LogWarning("Warning: an agent was not specified or this game object is not an agent!!!");
            }
            prevPosition = transform.localPosition;
        }

        public override void OnReset(Agent agent)
        {
            prevPosition = transform.localPosition;
            sumOfRewards = 0;
        }
    }
}