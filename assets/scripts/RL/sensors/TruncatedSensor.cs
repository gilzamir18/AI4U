using Godot;
using System;

namespace ai4u;

public partial class TruncatedSensor : AbstractSensor
{
		public TruncatedSensor()
		{
			SetKey("truncated");
			SetIsResetable(true);
			SetIsActive(true);
			SetIsInput(false);
			SetStackedObservations(1);
			SetSensorType(SensorType.sbool);
			SetShape(new int[]{GetStackedObservations()});
		}

		public override void OnSetup(Agent agent)
		{
			SetAgent((BasicAgent) agent);
		}

		public override bool GetBoolValue()
		{
			return GetAgent().Truncated;
		}
}
