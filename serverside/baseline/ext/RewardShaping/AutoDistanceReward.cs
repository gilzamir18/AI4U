using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u.ext 
{
    public class AutoDistanceReward : RewardFunc
    {

        public float maxRewardByEpisode = 0;
        public float reward = 1.0f;
        public GameObject target;
        public int samples = 10;
        private float[] hist;
        private int head;
        private int len;
        private RLAgent agent;
        private float totalReceivedReward = 0.0f;

        void Start()
        {
            hist = new float[samples];
            head = 0;
            len = 0;
            totalReceivedReward = 0.0f;
            agent = GetComponent<RLAgent>();
            agent.AddResetListener(this);
            if (samples <= 0) {
                Debug.LogWarning("The field samples value must be greater than 0!!!");
            }
        }

        private float GetMean(float[] hist, int n) 
        {
            if (n == 0) {
                return 0.0f;
            }
            float s = 0;
            for (int i = 0; i < n; i++) {
                s += hist[i];
            }
            return s/n;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
                if (agent != null && target != null) 
                {
                    float distance = Vector3.Distance(transform.position, target.transform.position);
                    
                    if (maxRewardByEpisode <= 0 || totalReceivedReward < maxRewardByEpisode)
                    {
                        float mean = GetMean(hist, len);
                        if (distance < mean)
                        {
                            totalReceivedReward += System.Math.Abs(reward);
                            agent.AddReward(reward, this);
                        }
                    }

                    hist[head] = distance;
                    head ++;
                    if (head >= samples) {
                        head = 0;
                    }
                    if (len < samples) 
                    {
                        len ++;
                    }
                } else 
                {
                    Debug.LogWarning("Warning: target is not specified in AutoDistanceRewardFunction");
                }
        }

        public override void OnReset(Agent agent)
        {
            totalReceivedReward = 0.0f;
            head = 0;
            len = 0;
        }
    }

}