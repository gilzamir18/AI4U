using System.Collections;
using System.Collections.Generic; 
using UnityEngine;
using ai4u;

namespace ai4u.ext {
    public struct ModelInput
    {
        public string name;
        public int[] shape;
        public Dtype type;
        public bool isImage;
        public ModelInput(string name, int[] s, Dtype dtype=Dtype.float32, bool isImage=false)
        {
            this.name = name;
            this.shape = s;
            this.type = dtype;
            this.isImage = isImage;
        }
    }

    public struct ModelOutput
    {
        public int size;
        public Dtype type;
        public  bool categorical;

        public ModelOutput(int size, Dtype dtype = Dtype.float32, bool cat = true) 
        {
            this.size = size;
            this.type = dtype;
            this.categorical = cat;
        }
    }

    public abstract class EnvironmentTemplate : MonoBehaviour
    {

        public string[] replacesNames;
        public string[] replacesValues;

        protected List<ModelInput> inputs = new List<ModelInput>();
        protected List<ModelOutput> outputs = new List<ModelOutput>();

        public void AddInput(ModelInput input)
        {
            this.inputs.Add(input);
        }

        public void AddOutput(ModelOutput output)
        {
            this.outputs.Add(output);
        }

        public void Reset()
        {
            this.inputs.Clear();
            this.outputs.Clear();
        }

        public abstract string GenCode(EnvironmentGenerator env, Agent agent);      
    }
}