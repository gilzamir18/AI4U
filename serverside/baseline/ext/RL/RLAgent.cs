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

        public virtual void touchListener(TouchRewardFunc origin)
        {
            //TODO add behavior here
        }

        public virtual void boxListener(BoxRewardFunc origin)
        {
            //TODO add behavior here
        }

        public virtual void SubReward(float v, RewardFunc from = null) {
            reward -= v;
        }

        public virtual void Act(string actionName) 
        {
            //TODO IMPL
        }

        public virtual string[] GetStateList(){
            //TODO IMPL
            return new string[]{};
        }

        public virtual object[] GetStateValue() {
            return new object[]{};
        }

        public float Reward {
            get {
                return this.reward;
            }
        }
    }
}