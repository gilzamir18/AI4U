using System.Collections;
using System.Collections.Generic;
using Godot;

namespace ai4u.ext {
	public class RLAgent : Agent
	{
		protected float reward;
		private int id;
		private bool done = false;

		public override  Node GetBody() {
			return null;
		}
		
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
		}

		public virtual void RequestDoneFrom(RewardFunc rf) {

		}

		public virtual void AddReward(float v, RewardFunc from = null) {
			if (from != null && from.causeEpisodeToEnd) {
				this.RequestDoneFrom(from);
			}
			reward += v;
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
