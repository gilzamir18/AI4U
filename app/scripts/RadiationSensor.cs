using System.Collections;
using System.Collections.Generic;
using Godot;
using ai4u;

public partial class RadiationSensor : Sensor
{
	[Export]
	public NodePath radiationPath;
	private RadiationSource radiationSource;

	private HistoryStack<float> stack;
	private float acmReward = 0.0f;

	public override void OnSetup(Agent agent)
	{
		radiationSource = GetNode(radiationPath) as RadiationSource;
		this.agent = (BasicAgent)agent;
		this.agent.endOfStepEvent += HandleEndOfStep;
		shape = new int[1]{1};
		type = SensorType.sfloatarray;
		stack = new HistoryStack<float>(shape[0]*stackedObservations);
		agent.AddResetListener(this);
	}

	private void HandleEndOfStep(BasicAgent agent)
	{
		agent.AddReward(acmReward);
		acmReward = 0.0f;
	}

	public override void OnReset(Agent agent)
	{
		stack = new HistoryStack<float>(shape[0]*stackedObservations);
		acmReward = 0.0f;
	}

	public override float[] GetFloatArrayValue()
	{
		float v = radiationSource.IntensityTo(this.agent.GetAvatarBody() as RigidBody3D);
		acmReward -= v;
		stack.Push(v);
		return stack.Values;
	}
}

