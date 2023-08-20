using System.Collections;
using System.Collections.Generic;
using ai4u;
using Newtonsoft.Json;


namespace ai4u
{
    public class ModelMetadataLoader
    {

        ModelMetadata metadata;

        public ModelMetadataLoader(BasicAgent agent)
        {
            List<ModelInput> inputs = new List<ModelInput>();
            List<ModelOutput> outputs = new List<ModelOutput>();

            foreach(Sensor s in agent.Sensors)
            {
                if (s.isInput)
                {
                    inputs.Add(new ModelInput(s.perceptionKey, s.type, s.shape, s.stackedObservations, s.RangeMin, s.RangeMax));
                }
            }

            foreach(Actuator a in agent.Actuators)
            {
                if (a.isOutput)
                {
                    outputs.Add(new ModelOutput(a.actionName, a.Shape, a.IsContinuous, a.RangeMin, a.RangeMax));
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
            return Newtonsoft.Json.JsonConvert.SerializeObject(metadata);
        }
    }
}
