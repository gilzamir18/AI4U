using System.Collections;
using System.Collections.Generic;
using Godot;
using ai4u;

namespace ai4u.ext {	
	public abstract class RLAgent : Agent
	{
		protected float reward;
		private int id;
		private bool done = false;
		private FloatSensor rewardSensor;
		private BoolSensor doneSensor;
		private int nSteps = 0;
		[Export]
		public int maxSteps = 500;

		public override void OnSetup()
		{
			rewardSensor = new FloatSensor();
			rewardSensor.perceptionKey = "reward";
			rewardSensor.type = SensorType.sfloat;
			rewardSensor.shape = new int[]{};
			rewardSensor.isState = false;
			rewardSensor.resettable = true;
			rewardSensor.OnBinding(this);
			AddSensor(rewardSensor);

			doneSensor = new BoolSensor();
			doneSensor.perceptionKey = "done";
			doneSensor.type = SensorType.sbool;
			doneSensor.shape = new int[]{};
			doneSensor.isState = false;
			doneSensor.resettable = true;
			doneSensor.OnBinding(this);
			AddSensor(doneSensor);
			
			RestartActuator restartActuator = new RestartActuator();
			restartActuator.actionName = "restart";
			restartActuator.IsOperation = true;
			restartActuator.OnBinding(this);
			AddActuator(restartActuator);
			
			NoOpActuator noOpActuator = new NoOpActuator();
			noOpActuator.actionName = "noop";
			noOpActuator.IsOperation = false;
			noOpActuator.OnBinding(this);
			AddActuator(noOpActuator);
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

		public override void HandleOnResetEvent()
		{
			reward = 0;
			done = false;
			nSteps = 0;
		}
		
		public override bool FilterAction(Actuator actuator) {
			if (actuator.actionReward != null) {
				actuator.actionReward.RewardFrom(this);
			}
			return !Done || actuator.IsOperation;
		}

		public virtual void RequestDoneFrom(RewardFunc rf)
		{
			this.Done = true;
			this.doneSensor.Data = this.Done;
			HandleOnDone();
		}

		public virtual void AddReward(float v, RewardFunc from = null, bool causeEpisodeToEnd = false) {
			if (from != null && causeEpisodeToEnd) 
			{
					this.RequestDoneFrom(from);
			}
			reward += v;
		}

		public override void AtBeginingOfTheStateUpdate()
		{
			if (nSteps > maxSteps)
			{
				Done = true;
				HandleOnDone();
			} 
			
			if (rewardSensor != null) rewardSensor.Data = reward;
			if (doneSensor != null) doneSensor.Data = Done;
		}
		
		public virtual void HandleOnDone() 
		{
			
		}
		
		public override void AtEndOfTheStateUpdate()
		{
			reward = 0.0f;
			if (rewardSensor != null) rewardSensor.Data = reward;
		}
	}
}
