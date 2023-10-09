using System.Collections;
using System.Collections.Generic;
using Godot;
using ai4u;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Linq;

namespace  ai4u;

public partial class NeuralNetController : Controller
{
	
	[Export]
	private string modelPath;
	
	[Export]
	private string mainOutput = "move";
	
	[Export]
	private bool repeat = true;
	
	private bool initialized = false;
	private ModelMetadata metadata;
	private bool isSingleInput = true;
	private Dictionary<string, int> inputName2Idx;
	private Dictionary<string, float[]> outputs;
	private ModelOutput modelOutput;
	private InferenceSession session;
	

	override public void OnSetup()
	{
		BasicAgent bagent = (BasicAgent) agent;
		inputName2Idx = new Dictionary<string, int>();
		outputs = new Dictionary<string, float[]>();
		metadata = agent.Metadata;
		for (int o = 0; o < metadata.outputs.Length; o++)
		{
			var output = metadata.outputs[o];
			outputs[output.name] = new float[output.shape[0]];
			if (output.name == mainOutput)
			{
				modelOutput = output;
			}
		}
		
		for (int i = 0; i < bagent.Sensors.Count; i++)
		{
			for (int j = 0; j < metadata.inputs.Length; j++)
			{
				if (bagent.Sensors[i].GetName() == metadata.inputs[j].name)
				{
					inputName2Idx[metadata.inputs[j].name] = i;
				}
			}
		}
		if (metadata.inputs.Length > 1)
		{
			isSingleInput = false;
		}
		else if (metadata.inputs.Length == 1)
		{
			isSingleInput = true;
		}
		else
		{
			throw new System.Exception("Neural model with less than one input");
		}
		
		session = new InferenceSession(modelPath);
	}
	
	override public void OnReset(Agent agent)
	{
		Inference();
	}

	override public string GetAction()
	{
		if (GetStateAsString(0) == "envcontrol")
		{
			if (GetStateAsString(1).Contains("restart"))
			{
				return ai4u.Utils.ParseAction("__restart__");
			}			
		}
		if (initialized && !((BasicAgent)agent).Done )
		{
			if (mainOutput == null)
			{
				throw new System.Exception("No valid main output was specified!");
			}
			Inference();
			float[] action = outputs[mainOutput];
			for (int i = 0; i < action.Length; i++)
			{
				var d = (modelOutput.rangeMax[i]-modelOutput.rangeMin[i]);
				action[i] = (action[i] + 1)/2;
				action[i] = modelOutput.rangeMin[i] + action[i] * d;
			}
			return ai4u.Utils.ParseAction(mainOutput, action);
		}
		else 
		{
			if (initialized)
			{
				if (repeat)
				{
					initialized = true;
					return ai4u.Utils.ParseAction("__restart__");
				}
				return ai4u.Utils.ParseAction("__noop__");
			}
			else
			{
				initialized = true;
				return ai4u.Utils.ParseAction("__restart__");
			}
		}
	}

	override public void NewStateEvent()
	{
		if (isSingleInput)
		{
			var name = metadata.inputs[0].name;
			GetInputAsArray(name);
		}
	}
	
	private float[] GetInputAsArray(string name)
	{
		return GetStateAsFloatArray(inputName2Idx[name]);
	}

	private void Inference()
	{
		var inputs = new List<NamedOnnxValue>();
		/*var random = new RandomNumberGenerator();
		random.Randomize();*/
		
		for (int i = 0; i < metadata.inputs.Length; i++)
		{
			var inputName = metadata.inputs[i].name;
			//GD.Print(inputName);
			var shape = metadata.inputs[i].shape;
			var dataDim = shape.Length - 1;
			DenseTensor<float> t;
			if (dataDim  >= 1)
			{ 
				System.Memory<float> mem = new System.Memory<float>(GetInputAsArray(inputName));
				/*float[] dt = new float[shape[0] * shape[1]];
				for (int k = 0; k < shape[0] * shape[1]; k++)
				{
					dt[k] = random.Randfn();
				}
				System.Memory<float> mem = new System.Memory<float>( dt );*/
				t = new DenseTensor<float>( mem, shape );
			}
			else
			{
				throw new System.Exception($"Controller configuration: unsuported data dimenstion: {shape}. Check agetn's environment configuration by Perception Key {inputName}.");
			}
			inputs.Add(NamedOnnxValue.CreateFromTensor<float>(inputName, t));
		}


		using (var results = session.Run(inputs))
		{
			foreach(var r in results)
			{
					var output = r.AsEnumerable<float>().ToArray();
					outputs[r.Name] = output;
					/*
					GD.Print("----------------------------------");
					for (int k = 0; k < output.Length; k++)
					{
						GD.Print(output[k]);
					}
					GD.Print("----------------------------------");*/
			}
		}
	}
}

