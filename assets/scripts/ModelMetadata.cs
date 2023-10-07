using System.Collections;
using System.Collections.Generic;
using System;
using ai4u;

namespace ai4u
{
	[Serializable]
	public struct ModelInput
	{
		public string name {get; set;}
		public int[] shape {get; set;}
		public SensorType type {get; set;}
		public float rangeMin {get; set;}
		public float rangeMax {get; set;}

		public ModelInput(string name, SensorType type, int[] shape, int stackedObservations, float rangeMin, float rangeMax)
		{
			this.name = name;
			
			this.shape = new int[shape.Length + 1];
			this.shape[0] = stackedObservations;
			for (int i = 1; i < this.shape.Length; i++)
			{
				this.shape[i] = shape[i-1];
			}
			this.type = type;
			this.rangeMin = rangeMin;
			this.rangeMax = rangeMax;
		}
	}

	[Serializable]
	public struct ModelOutput
	{
		public string name {get; set;}
		public bool isContinuous {get; set;}
		public int[] shape {get; set;}

		public float[] rangeMin {get; set;}
		public float[] rangeMax {get; set;}


		public ModelOutput(string name, int[] shape, bool isContinuous, float[] rangeMin, float[] rangeMax)
		{
			this.name = name;
			this.shape = shape;
			this.isContinuous = isContinuous;
			this.rangeMin = rangeMin;
			this.rangeMax = rangeMax;
		}
	}

	[Serializable]
	public partial class ModelMetadata
	{
		public ModelInput[] inputs {get; set;}
		public ModelOutput[] outputs {get; set;}

		public ModelMetadata(int inputCount, int outputCount)
		{
			inputs = new ModelInput[inputCount];
			outputs = new ModelOutput[outputCount];
		}

		public void SetInput(int idx, ModelInput i)
		{
			this.inputs[idx] = i;
		}

		public ModelInput GetInput(int idx)
		{
			return this.inputs[idx];
		}

		public void SetOutput(int idx, ModelOutput o)
		{
			this.outputs[idx] = o;
		}

		public ModelOutput GetOutput(int idx)
		{
			return this.outputs[idx];
		}

		public int InputCount()
		{
			return inputs.Length;
		}

		public int OutputCount()
		{
			return outputs.Length;
		}
	}
}
