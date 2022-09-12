using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u
{
    public class MinDistanceReward : RewardFunc
    {
        public GameObject target;
        public float successReward = 0.01f;
        public float stepReward = 0.001f;
        private BasicAgent agent;
        private float minDistance;
        
        public override void OnSetup(Agent agent)
        {
            this.agent = (BasicAgent)agent;
            this.agent.AddResetListener(this);
            minDistance = System.Single.PositiveInfinity;
        }

        // Update is called once per frame
        public override void OnUpdate()
        {
            Vector3 d = this.agent.gameObject.transform.position - target.transform.position;
            float dist = d.magnitude;
            if (dist < minDistance)
            {
                minDistance = dist;
                //Debug.Log("SUCE: " + successReward);
                this.agent.AddReward(successReward, this);
            } else
            {
                this.agent.AddReward(-stepReward, this);
            }
        }

        public override void OnReset(Agent agent)
        { 
            this.agent = (BasicAgent) agent;
            minDistance = System.Single.PositiveInfinity;
        }
    }
}