using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u.ext {

    public class RLAgent : Agent
    {
        protected float reward;
        private int id;

        public int Id {
            get {
                return id;
            }

            set {
                id = value;
            }
        }

        public void ResetReward() {
            reward = 0;
        }

        public virtual void AddReward(float v, RewardFunc from = null) {
            reward += v;
        }

        public virtual void SubReward(float v, RewardFunc from = null) {
            reward -= v;
        }

        public float Reward {
            get {
                return this.reward;
            }
        }
    }
}