using System.Collections;
using System.Collections.Generic;
using ai4u.math;
using Godot;

namespace ai4u 
{
	public partial class FloatArrayCompositeSensor : Sensor
	{
		private List<Sensor> sensors;
		private int size = 0;
		private HistoryStack<float> stack;

		private float[] lastValues = null;

		public float[] LastValues
		{
			get
			{
				return lastValues;
			}
		}


        [ExportCategory("Data Range")]
        [Export]
		private float _rangeMin = -1;
		[Export]
		private float _rangeMax = 1;
		[ExportGroup("Data Scale")]
		[Export]
		private bool normalize = false;
		[Export]
        private float minValue = -1;
		[Export]
        private float maxValue = 1;

		public float[] UnnormalizedValues => unnormalizedData;

		private float[] unnormalizedData;

		public override void OnSetup(Agent agent)
		{
			rangeMin = _rangeMin;
			rangeMax = _rangeMax;
			sensors = new List<Sensor>();
			this.type = SensorType.sfloatarray;
			var children = GetChildren();
			for (int i = 0; i < children.Count; i++) 
			{
				if (children[i] is Sensor)
				{
					Sensor s = (Sensor)children[i];
					if (s != null && s.IsActive())
					{
						sensors.Add(s);
						s.OnSetup(agent);

						var prod = 1;
						foreach(var d in s.shape)
						{
							prod *= d;
						}
						size += prod;
						
					}
				}
			}
			this.shape = new int[1]{size * stackedObservations};
			this.stack = new HistoryStack<float>(this.shape[0]);
		}

		public override void OnReset(Agent agent)
		{
			this.stack = new HistoryStack<float>(this.shape[0]);
			int n = sensors.Count;
			for (int i = 0; i < n; i++)
			{
				if (sensors[i].IsResetable())
				{
					sensors[i].OnReset(agent);
				}
			}

            unnormalizedData = GetFloatArrayValue();

            if (normalize)
            {
                lastValues = new float[this.unnormalizedData.Length];
                for (int i = 0; i < this.unnormalizedData.Length; i++)
                {
                    lastValues[i] = (this.unnormalizedData[i] - minValue) / (maxValue - minValue);
                }
            }
            else
            {
                lastValues = this.unnormalizedData;
            }
        }

		public override float[] GetFloatArrayValue()
		{
			int n = sensors.Count;
			for (int i = 0; i < n; i++)
			{
				Sensor s = sensors[i];
				if (s.type == SensorType.sfloatarray)
				{
					float[] a = s.GetFloatArrayValue();
					for (int j = 0; j < a.Length; j++)
					{
						stack.Push(a[j]);
					}
				}
				else if (s.type == SensorType.sfloat)
				{
					stack.Push(s.GetFloatValue());
				} else if (s.type == SensorType.sint)
				{
					stack.Push(s.GetIntValue());
				} else if (s.type == SensorType.sbool)
				{
					stack.Push(s.GetBoolValue() ? rangeMax: rangeMin);
				}
			}
			this.unnormalizedData = stack.Values;
			if (normalize)
			{
				lastValues = new float[this.unnormalizedData.Length];
				for (int i = 0; i < this.unnormalizedData.Length; i++)
				{
					lastValues[i] = (this.unnormalizedData[i] - minValue)/(maxValue-minValue);
				}
			}
			else
			{
				lastValues = this.unnormalizedData;
			}
			return lastValues;
		}
	}
}
