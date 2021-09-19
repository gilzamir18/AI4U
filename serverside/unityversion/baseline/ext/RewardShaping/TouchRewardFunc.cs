﻿using System.Collections;
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
        public MultTouchPrecondition multiplePreconditions = null;
        public bool triggerOnStay = true;
        private int[] counter;
        private bool[] touched;
        private Collider myCollider;
        public bool allowNext;

        void Awake() {

            if (agents.Length==0) {
                Debug.LogWarning("TouchRewardFunc: no agents added for this event. Game Object: " + gameObject.name);
            }

            counter = new int[agents.Length];
            touched = new bool[agents.Length];
            myCollider = GetComponent<Collider>();
            foreach(Agent agent in agents) {
                if (agent == null) {
                    Debug.LogWarning("TouchRewardFunc: You have not defined the agent that will receive this event. Game Object: " + gameObject.name);
                }
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
            if (agent != null) {
                touched[agent.Id] = true;
                agent.touchListener(this);  

                if (precondition != null) {
                    if (!precondition.allowNext || !precondition.wasTouched(agent)){
                        agent.AddReward(-painForViolatinPrecondition, this);
                        agent.PreconditionFailListener(this, precondition);
                        return;
                    }
                }

                if (multiplePreconditions != null)
                {
                    if (!multiplePreconditions.allowNext || !multiplePreconditions.wasTouched(agent))
                    {
                        agent.AddReward(-painForViolatinPrecondition, this);
                        agent.PreconditionFailListener(this, multiplePreconditions);
                        return;
                    }
                }

                if ( (counter[agent.Id] <
                    maxTouch) || (maxTouch < 0)  )
                {
                    counter[agent.Id]++;
                    agent.AddReward(rewardValue, this);
                } else if (maxTouch >= 0 && counter[agent.Id] >= maxTouch) {
                    agent.AddReward(-painForOverTouch, this);
                }
            }
        }
    }
}
