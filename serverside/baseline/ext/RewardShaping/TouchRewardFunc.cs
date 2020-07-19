using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  ai4u.ext
{
    public class TouchRewardFunc : RewardFunc
    {
        public int maxTouch = -1;

        public float painForOverTouch = 0;

        public float painForViolatinPrecondition = 0;

        public float rewardValue = 1.0f;

        public TouchRewardFunc precondition = null;
        public MultTouchPrecondiction multiplePrecondictions = null;
        public bool triggerOnStay = true;
        private int[] counter;
        private bool[] touched;
        private Collider myCollider;
        public bool allowNext;

        void Awake() {
            counter = new int[agents.Length];
            touched = new bool[agents.Length];
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

        public bool wasTouched(RLAgent agent) {
            return touched[agent.Id];
        }

        public override void OnReset(Agent agent) {
            counter = new int[agents.Length];
            touched = new bool[agents.Length];
        }

        public void Check(Collider collider)
        {
            RLAgent agent = collider.gameObject.GetComponent<RLAgent>();
            touched[agent.Id] = true;
            agent.touchListener(this);  

            if (precondition != null) {
                if (!precondition.allowNext || !precondition.wasTouched(agent)){
                    agent.AddReward(-painForViolatinPrecondition, this);
                    return;
                }
            }

            if (multiplePrecondictions != null)
            {
                if (!multiplePrecondictions.allowNext || !multiplePrecondictions.wasTouched(agent))
                {
                    agent.AddReward(-painForViolatinPrecondition, this);
                    return;
                }
            }

            if ( (counter[agent.Id] <
                maxTouch || maxTouch < 0)  )
            {
                counter[agent.Id]++;
                agent.AddReward(rewardValue, this);
            } else if (maxTouch >= 0 && counter[agent.Id] >= maxTouch) {
                agent.AddReward(-painForOverTouch, this);
            }
        }
    }
}
