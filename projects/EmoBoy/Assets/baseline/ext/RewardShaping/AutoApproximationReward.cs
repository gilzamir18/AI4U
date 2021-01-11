using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u.ext 
{
    public class AutoApproximationReward : RewardFunc
    {

        public float maxRewardByEpisode = 0;
        public float minimumDistance = 1.0f;
        public float reward = 1.0f;
        public bool ignoreX = false;
        public bool ignoreY = true;
        public bool ignoreZ = false;
        public GameObject target;
        public int samples = 10;
        private float[] hist;
        private int head;
        private int len;
        private RLAgent agent;
        private float totalReceivedReward = 0.0f;
        private Vector3 prevPosition;
        private float fx, fy, fz;
        private Vector3 f;

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
            hist = new float[samples];
            head = 0;
            len = 0;
            totalReceivedReward = 0.0f;
            agent = GetComponent<RLAgent>();
            agent.AddResetListener(this);
            prevPosition = transform.localPosition;
            if (samples <= 0) {
                Debug.LogWarning("Warning: the field samples value must be greater than 0!!!");
            }
            if (target == null) {
                Debug.LogWarning("Warning: target is not specified for reward function named AutoApproximationReward.");
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
            if (target != null) 
            {
                Vector3 tp = Vector3.Scale(target.transform.localPosition,f);
                Vector3 p = Vector3.Scale(transform.localPosition, f);
                Vector3 pp = Vector3.Scale(prevPosition, f);

                float distance = Vector3.Distance(p, tp);
                float autoDist = Vector3.Distance(p, pp);
                if (maxRewardByEpisode <= 0 || totalReceivedReward < maxRewardByEpisode)
                {
                    float mean = GetMean(hist, len);
                    if (autoDist >= minimumDistance && distance < mean)
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
            prevPosition = transform.localPosition;
        }

        public override void OnReset(Agent agent)
        {
            totalReceivedReward = 0.0f;
            prevPosition = transform.localPosition;
            head = 0;
            len = 0;
        }
    }

}