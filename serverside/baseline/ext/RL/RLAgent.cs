using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u.ext {

    public class RLAgent : Agent
    {
        protected float reward;
        private int id;
        private Dictionary<string, bool> firstTouch;
        private bool done = false;

        public virtual bool Done {
            get {
                return done;
            }

            set {
                done = value;
            }
        }

        public int Id {
            get {
                return id;
            }

            set {
                id = value;
            }
        }

        public override void HandleOnResetEvent(){
            reward = 0;
            done = false;
            firstTouch = new Dictionary<string, bool>();            
        }

        public virtual void RequestDoneFrom(RewardFunc rf) {

        }

        private bool checkFirstTouch(string tag){
            if (firstTouch.ContainsKey(tag)) {
                return false;
            } else {
                firstTouch[tag] = false;
                return true;
            }
        }

        public virtual void PreconditionFailListener(RewardFunc func, RewardFunc precondiction) { 
            if (Done) return;
            if (func is TouchRewardFunc) {
                if (!checkFirstTouch(func.gameObject.tag) && !((TouchRewardFunc)func).allowNext)
                {
                    this.RequestDoneFrom(func);
                }
            }
        }

        public virtual void AddReward(float v, RewardFunc from = null) {
            if (from != null && from.causeEpisodeToEnd) {
                this.RequestDoneFrom(from);
            }
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