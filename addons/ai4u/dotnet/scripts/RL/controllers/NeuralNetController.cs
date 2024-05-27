using System.Collections;
using System.Collections.Generic;
using Godot;
using ai4u;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Linq;
using System.IO;
using System;
using System.Drawing;
namespace  ai4u;

///<summary> An object of NeuralNetController allows the control of an agent through a 
/// PyTorch model that has been trained with stable-baselines3 and converted to an ONNX model.
/// Only ONNX models that have been converted using BeMaker tools are supported.
/// </summary>
public partial class NeuralNetController : Controller
{
	
	/// <summary>
	/// The path of the ONNX model. 
	/// </summary>
	[Export]
	private string modelPath;

    /// <summary>
    /// The name of the actuator that receives commands from the neural network 
	/// in the ONNX model.
    /// </summary>
    [Export]
	private string mainOutput = "move";
	
	/// <summary>
	/// If true, episode is restarted after ending.
	/// </summary>
	[Export]
	private bool repeat = true;
	
	private bool initialized = false; //indicates if episode has been initialized.
	private ModelMetadata metadata; //Metadata of the input and outputs of the agent decision making. 
	private bool isSingleInput = true; //Flag indicating if agent has single sensor or multiple sensors.
	private Dictionary<string, int> inputName2Idx; //mapping sensor name to sensor index.
	private Dictionary<string, float[]> outputs; //mapping model output name to output value.
	private ModelOutput modelOutput; //output metadata.
	private InferenceSession session; //inference session of the Microsoft.ML framework.
	
	/// <summary>
	/// <see cref="ai4u.Controller.OnSetup"/>
	/// </summary>
	/// <exception cref="System.Exception"></exception>
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
	
	/// <summary>
	/// <see cref="ai4u.Controller.OnReset(Agent)"/>
	/// </summary>
	/// <param name="agent"></param>
	override public void OnReset(Agent agent)
	{
		Inference();
	}

	/// <summary>
	/// <see cref="ai4u.Controller.GetAction"/>
	/// </summary>
	/// <returns></returns>
	/// <exception cref="System.Exception"></exception>
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
				action[i] = (action[i] + 1.0f)/2.0f;
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

	/// <summary>
	/// <see cref="ai4u.Controller.NewStateEvent"/>
	/// </summary>
	override public void NewStateEvent()
	{

	}
	
	/// <summary>
	/// This method gets state from sensor named <code>name</code> and returns its value as an array of float-point numbers.
	/// </summary>
	/// <param name="name"></param>
	/// <returns>float[]: sensor value</returns>
	private float[] GetInputAsArray(string name)
	{
		return GetStateAsFloatArray(inputName2Idx[name]);
	}

	private float[] GetGrayImageInputAsArray(string name, int width = 61, int height = 61, Image.Format format = Image.Format.L8)
	{
		string content = GetStateAsString(inputName2Idx[name]);
		string[] streams = content.Trim().Split(' ');
		float[] data = new float[width * height * streams.Length];
		int k = 0;
		for (int i = 0; i < streams.Length; i++)
		{
			byte[] bytes = System.Convert.FromBase64String(streams[i]);
			
			Image image = new Image();
			image.LoadPngFromBuffer(bytes);
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					data[k] = image.GetPixel(y, x).Luminance;
					k ++;
				}
			}
		}
		return data;
	}

	/// <summary>
	/// This method runs the current ONNX model with current perceptions of the agent. 
	/// </summary>
	/// <exception cref="System.Exception"></exception>
	private void Inference()
	{
		var inputs = new List<NamedOnnxValue>();

		for (int i = 0; i < metadata.inputs.Length; i++)
		{
			var inputName = metadata.inputs[i].name;
			var shape = metadata.inputs[i].shape;
			var dataDim = shape.Length;
			
			if (dataDim  == 1)
			{ 
				System.Memory<float> mem = new System.Memory<float>(GetInputAsArray(inputName));
				DenseTensor<float> t = new DenseTensor<float>( mem, new int[2]{1, shape[0]} );
				inputs.Add(NamedOnnxValue.CreateFromTensor<float>(inputName, t));
			}
			else if (dataDim == 2)
			{

				int width = shape[0];
				int height = shape[1];

				System.Memory<float> mem = new System.Memory<float>(GetGrayImageInputAsArray(inputName, width, height));
				DenseTensor<float> t = new DenseTensor<float>( mem, new int[3]{1, width, height});
				inputs.Add(NamedOnnxValue.CreateFromTensor<float>(inputName, t));
			} else if (dataDim == 3)
			{
				int width = shape[1];
				int height = shape[2];
				System.Memory<float> mem = new System.Memory<float>(GetGrayImageInputAsArray(inputName, width, height));
				DenseTensor<float> t = new DenseTensor<float>( mem, new int[4]{1, shape[0], width, height});
				inputs.Add(NamedOnnxValue.CreateFromTensor<float>(inputName, t));
			}
			else
			{
				throw new System.Exception($"Controller configuration: unsuported data dimenstion: {shape}. Check agetn's environment configuration by Perception Key {inputName}.");
			}
		}


		using (var results = session.Run(inputs))
		{
			foreach(var r in results)
			{
					var output = r.AsEnumerable<float>().ToArray();
					outputs[r.Name] = output;
			}
		}
	}
}

