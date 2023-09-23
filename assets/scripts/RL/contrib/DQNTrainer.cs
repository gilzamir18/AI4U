using Godot;
using System;
using ai4u;
using System.Collections.Generic;
using TorchSharp;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;

namespace ai4u.contrib;

public class ReplayMemory<T>
{
	private readonly T[] memory;
	private int head;
	private readonly Random random;
	private bool filled;

	public ReplayMemory(int capacity)
	{
		this.memory = new T[capacity];
		this.random = new Random();
	}

	public void Push(T item)
	{
		memory[head] = item;
		head = (head + 1) % memory.Length;
		if (!filled && head == memory.Length - 1)
		{
			filled = true;
		}
	}

	public List<T> Sample(int batchSize)
	{
		var result = new List<T>();
		var indices = new HashSet<int>();
		int count = filled ? memory.Length : head;
		while (result.Count < batchSize)
		{
			int index = random.Next(count);
			if (indices.Add(index))
			{
				result.Add(memory[index]);
			}
		}
		return result;
	}

	public int Length()
	{
		return filled ? memory.Length : head;
	}
}

internal class Transition<T, U>
{
	public T InitialState {get;set;}
	public U Action {get; set;}
	public T NextState {get; set;}
	public float Reward {get; set;}
	
	public Transition(T initialState,  U action, T nextState, float reward)
	{
		this.InitialState = initialState;
		this.Action = action;
		this.NextState = nextState;
		this.Action = action;
		this.Reward = reward;
	}
}

internal class QNet : Module<Tensor,Tensor>
{
	public QNet(int nbObs, int nbActions)
		: base(nameof(QNet))
	{
		RegisterComponents();
		layer1 = Linear(nbObs, 128);
		layer2 = Linear(128, 128);
		layer3 = Linear(128, nbActions);
	}

	public override Tensor forward(Tensor input)
	{
		using var x1 = layer1.forward(input);
		using var x2 = layer2.forward(x1);
		return layer3.forward(x2);
	}

	private nn.Module<Tensor,Tensor> layer1, layer2, layer3;
}

public partial class DQNTrainer : Trainer
{
	[Export]
	private string inputSensor = "vision";
	
	[Export]
	private string actionName = "move";
	
	[Export]
	private int bufferSize = 10000;
	[Export]
	private int stateSize;
	[Export]
	private int numberOfActions;
	[Export]
	private int batchSize = 128;
	[Export]
	private float gamma = 0.99f;
	[Export]
	private float epsStart = 0.9f;
	[Export]
	private float epsEnd = 0.05f;
	[Export]
	private float epsDecay = 1000;
	[Export]
	private float tau = 0.005f;
	[Export]
	private float lr = 1e-4f;
	
	private ReplayMemory< Transition<float[], int> > replayBuffer;
	private QNet policyNet;
	private QNet targetNet;
	private torch.optim.Optimizer optimizer;
	private float[] initialState = null;
	private int action = -1;
	private int numberOfFields = 0;
	private int inputIdx = -1;
	private float[][] actions = new float[][]{
			new float[]{1, 0, 0, 0}, //forward
			new float[]{-1, 0, 0, 0}, //backward
			new float[]{0, 1, 0, 0}, //turn right
			new float[]{0, -1, 0, 0}, //turn left
			new float[]{0, 0, 1, 0}, //jump
			new float[]{0, 0, 0, 1} //jumpforward
		};
	/// <summary>
	/// Here you allocate extra resources for your specific training loop.
	/// </summary>
	public override void OnSetup()
	{
		policyNet = new QNet(stateSize, numberOfActions);
		targetNet = new QNet(stateSize, numberOfActions);
		optimizer = torch.optim.AdamW(policyNet.parameters(), lr:lr, amsgrad:true);
		targetNet.load_state_dict(policyNet.state_dict());
		replayBuffer = new ReplayMemory< Transition<float[], int> >(bufferSize);
	}
	
	public override void OnReset(Agent agent)
	{
		numberOfFields = controller.GetStateSize();
		for (int i = 0; i < numberOfFields; i++)
		{
			if (controller.GetStateName(i) == inputSensor)
			{
				inputIdx = i;
			}
		}
	}

	/// <summary>
	/// This callback method run after agent percept a new state.
	/// </summary>
	public override void StateUpdated()
	{
		float[] state = controller.GetStateAsFloatArray(inputIdx);
		float[] q = ModelAction(state);
		int argmax = 0;
		for (int i = 1; i < q.Length; i++)
		{
			if (q[argmax] <= q[i])
			{
				argmax = i;
			}
		}
		
		var curAction = argmax;
		controller.RequestContinuousAction(actionName, actions[argmax]);
		
		if (initialState == null)
		{
			initialState = state;
			action = curAction;
		}
		else
		{
			var t = new Transition<float[], int>(initialState, action, state, 
											controller.LastReward);
			replayBuffer.Push(t);
		}
	}
	
	private float[] ModelAction(float[] state)
	{
		return new float[]{1, 0, 0, 0, 0, 0};
	}
	
	private void Training()
	{
			
		
	}
	
}
