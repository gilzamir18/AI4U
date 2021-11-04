using Godot;
using System;
using System.Collections.Generic;
using ai4u;

namespace ai4u.ext.npcs
{
	public class EnergySensor : Sensor
	{
		[Export]
		public int maxEnergy = 1000;
		[Export]
		public int energyDecay = 0;
		
		private int currentEnergy = 0;
		
		[Export]
		public Godot.Collections.Array<NodePath> actionPath;
		
		public int CurrentEnergy
		{
			get
			{
				return currentEnergy;
			}
		}
		
		public override int GetIntValue()
		{
			if (energyDecay > 0)
			{
				this.currentEnergy -= energyDecay;
			}
			
			if (this.currentEnergy <= 0)
			{
				(this.agent as RLAgent).RequestDoneFrom(this);
			}
			return this.currentEnergy;
		} 

		public override void OnBinding(Agent agent)
		{
			this.agent = agent;
			type = SensorType.sint;
			currentEnergy = maxEnergy;
			
			if (actionPath != null)
			{
				foreach(NodePath path in actionPath)
				{
						Actuator a = (GetNode(path) as Actuator);
						a.Subscribe(this);
				}
			}
		}

		public override void OnAction(Actuator a)
		{
			currentEnergy += (int)a.ActionValue;	
		}
		
		public override void OnReset(Agent a)
		{
			this.currentEnergy = maxEnergy;
		}
	}
}
