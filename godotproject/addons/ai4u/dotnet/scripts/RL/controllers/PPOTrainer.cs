using Godot;
using Microsoft.ML.OnnxRuntime;
using System;
using System.Collections.Generic;
using TorchSharp;

namespace ai4u;

public partial class PPOTrainer : Trainer
{
	private PPO ppoAlg;

	private int inputSize = -1;

	private int numberOfSensors = -1;

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

	private int rewardIdx = -1;


	private long countUpdates = 0;

	/// <summary>
	/// Here you allocate extra resources for your specific training loop.
	/// </summary>
	public override void OnSetup()
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
			if (bagent.Sensors[i].GetKey() == "reward")
			{
				rewardIdx = i;
			}
			for (int j = 0; j < metadata.inputs.Length; j++)
			{
				if (bagent.Sensors[i].GetName() == metadata.inputs[j].name)
				{
					if (metadata.inputs[j].name == null)
						throw new Exception($"Perception key of the sensor {bagent.Sensors[i].GetType()} cannot be null!");
					inputName2Idx[metadata.inputs[j].name] = i;
					inputSize = metadata.inputs[i].shape[0];
					numberOfSensors ++;	
				}
			}
		}

		if (metadata.inputs.Length == 1)
		{
			isSingleInput = true;
		}
		else
		{
			isSingleInput = false;
			throw new System.Exception("Only one input is supported!!!");
		}

		ppoAlg = new PPO(inputSize, modelOutput.shape[0]);
	}	
	
	///<summary>
	/// Here you get agent life cicle callback about episode resetting.
	///</summary>
	public override void OnReset(Agent agent)
	{

		GD.Print("Episode Reward: " + ((BasicAgent)agent).EpisodeReward);
		started = false;
	}

	/// <summary>
	/// This callback method run after agent percept a new state.
	/// </summary>
	public override void StateUpdated()
	{
		var reward = controller.GetStateAsFloat(rewardIdx);
		var (states, actions, rewards, logprobs, values) = CollectData(this, reward);
		var (pl, vl, newLogProb, newValues) = ppoAlg.Update(states, actions, rewards, logprobs, values);


		oldLogProbs = torch.empty_like(newLogProb).copy_(newLogProb);
		oldValues = torch.empty_like(newValues).copy_(newValues);

		countUpdates++;
		if (countUpdates >= long.MaxValue)
		{
			countUpdates = 0;
		} 
		if (countUpdates % 100 == 0)
		{
			GD.Print("Policy Loss: " + pl.item<float>());
			GD.Print("Critic Loss: " + vl.item<float>());
		}
	}
	/// <summary>
	/// This method gets state from sensor named <code>name</code> and returns its value as an array of float-point numbers.
	/// </summary>
	/// <param name="name"></param>
	/// <returns>float[]: sensor value</returns>
	private float[] GetInputAsArray(string name)
	{
		return controller.GetStateAsFloatArray(inputName2Idx[name]);
	}


	public override void EnvironmentMessage()
	{
		
	}

	private float[] GetGrayImageInputAsArray(string name, int width = 61, int height = 61, Image.Format format = Image.Format.L8)
	{
		string content = controller.GetStateAsString(inputName2Idx[name]);
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


	private static torch.Tensor oldLogProbs = null, oldValues = null;

	private static bool started = false;

	static (torch.Tensor, torch.Tensor, torch.Tensor, torch.Tensor, torch.Tensor) CollectData(PPOTrainer trainer, float lastReward)
	{
		torch.Tensor t = null;

		for (int i = 0; i < trainer.metadata.inputs.Length; i++)
		{
			var inputName = trainer.metadata.inputs[i].name;
			var shape = trainer.metadata.inputs[i].shape;
			var dataDim = shape.Length;
			
			if (dataDim  == 1 && trainer.metadata.inputs[i].type == SensorType.sfloatarray)
			{ 
				var values = trainer.GetInputAsArray(inputName);
				
				t = torch.FloatTensor(values);//new DenseTensor<float>( mem, new int[2]{1, shape[0]} );
				t = t.reshape(new long[2]{1, shape[0]});
			}
			else
			{
				throw new System.Exception($"Controller configuration Error: for while, only MLPPolicy is supported!");
			}
			
			/*
			else if (dataDim == 2)
			{

				int width = shape[0];
				int height = shape[1];
				var t = torch.FloatTensor(trainer.GetGrayImageInputAsArray(inputName, width, height));//new DenseTensor<float>( mem, new int[2]{1, shape[0]} );
				t.reshape(new long[3]{1, width, height});
				inputs.Add(t);
			} else if (dataDim == 3)
			{
				int width = shape[1];
				int height = shape[2];
				var t = torch.FloatTensor(trainer.GetGrayImageInputAsArray(inputName, width, height));//new DenseTensor<float>( mem, new int[2]{1, shape[0]} );
				t.reshape(new long[4]{1, shape[0], width, height});
				inputs.Add(t);
			}
			else
			{
				throw new System.Exception($"Controller configuration: unsuported data dimenstion: {shape}. Check agetn's environment configuration by Perception Key {inputName}.");
			}*/
		}
		var y = trainer.ppoAlg.PolicyForward(t);
		float[] result = new float[4];
		y.data<float>().CopyTo(result, 0);

		int action = ArgMax(result);

		//GD.Print("Action " + action);
		trainer.controller.RequestAction(trainer.mainOutput, new int[]{action});
		
		var logProb = torch.log(result);

		var v = trainer.ppoAlg.ValueForward(t);

		if (!started)
		{	
			oldLogProbs = torch.empty_like(logProb).copy_(logProb);
			oldValues = torch.empty_like(v).copy_(v);
			started = true;
		}

		var r = (t, y, torch.ones(1, 1) * lastReward, oldLogProbs, oldValues);
		

		return r;
	}

	private static int ArgMax(float[] values)
	{
		if (values.Length <= 0)
		{
			return -1;
		}
		if (values.Length == 1)
		{
			return 0;
		}
		int maxi = 0;
		for (int i = 1; i < values.Length; i++)
		{
			if (values[i] > values[maxi])
			{
				maxi = i;
			}
		}
		return maxi;
	}
}
