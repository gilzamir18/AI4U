using System.Collections;
using System.Collections.Generic;
using ai4u;
using System.Text;


namespace ai4u
{
	public partial class ModelMetadataLoader
	{

		ModelMetadata metadata;

		public ModelMetadataLoader(RLAgent agent)
		{
			List<ModelInput> inputs = new List<ModelInput>();
			List<ModelOutput> outputs = new List<ModelOutput>();

			foreach(ISensor s in agent.Sensors)
			{
				if (s.IsInput())
				{
					inputs.Add(new ModelInput(s.GetName(), s.GetSensorType(), s.GetShape(), s.GetStackedObservations(), s.GetRangeMin(), s.GetRangeMax(), s.GetDataType()));
				}
			}

			foreach(Actuator a in agent.Actuators)
			{
				if (a.isOutput)
				{
					outputs.Add(new ModelOutput(a.actionName, a.Shape3D, a.IsContinuous, a.RangeMin, a.RangeMax));
				}
			}

			metadata = new ModelMetadata(inputs.Count, outputs.Count);
			for (int i = 0; i < inputs.Count; i++)
			{
				metadata.SetInput(i, inputs[i]);
			}

			for (int i = 0; i < outputs.Count; i++)
			{
				metadata.SetOutput(i, outputs[i]);
			}
		}


		public ModelMetadata Metadata
		{
			get
			{
				return metadata;
			}
		}

		public string toJson()
		{
			return System.Text.Json.JsonSerializer.Serialize(metadata);
		}
	}
}
