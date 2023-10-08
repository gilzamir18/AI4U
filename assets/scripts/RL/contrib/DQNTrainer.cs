using Godot;
using System;
using ai4u;
using System.Collections.Generic;
using TorchSharp;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;
using System.Threading.Tasks;
using System.Linq;

namespace ai4u.contrib;

/*
Reference: https://pytorch.org/tutorials/intermediate/reinforcement_q_learning.html
*/
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
	[Export]
	private int numEpisodes = 1000;
	
	private ReplayMemory< Transition<float[], int> > replayBuffer;
	private QNet policyNet;
	private QNet targetNet;
	private torch.optim.Optimizer optimizer;
	private float[] initialState = null;
	private int action = -1;
	private int numberOfFields = 0;
	private int inputIdx = -1;
	private RandomNumberGenerator random;
	private int stepsDone = 0;
	
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
		random = new RandomNumberGenerator();
		random.Randomize();
		policyNet = new QNet(stateSize, numberOfActions);
		targetNet = new QNet(stateSize, numberOfActions);
		optimizer = torch.optim.AdamW(policyNet.parameters(), lr:lr, amsgrad:true);
		targetNet.load_state_dict(policyNet.state_dict());
		replayBuffer = new ReplayMemory< Transition<float[], int> >(bufferSize);
	}
	
	public override void OnReset(Agent agent)
	{
		stepsDone = 0;
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
		int curAction = SelectAction(state);

		controller.RequestContinuousAction(actionName, actions[curAction]);
		
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
	
	private float[] SampleAction()
	{
		float[] a = new float[numberOfActions];
		for (int i = 0; i < numberOfActions; i++)
		{
			a[i] = random.Randfn();
		}
		return a;
	}
	
	private int SelectAction(float[] state)
	{
		var sample = random.RandiRange(0, 99)/100.0f;
		var eps_threshold = epsEnd + (epsStart - epsEnd) * Mathf.Exp(-1 * stepsDone/epsDecay);
		stepsDone += 1;
		if (sample > eps_threshold)
		{
			var q = policyNet.call(state);
			return q.argmax(-1).item<int>();
		}
		else
		{
			return random.RandiRange(0, numberOfActions-1);
		}
	}
	
	private async Task OptimizeModel()
	{
		if (this.replayBuffer.Length() < this.batchSize)
		{
			return;
		}
		
		var transitions = replayBuffer.Sample(this.batchSize);
		
		Tensor[] initialStates, actions, nextStatesA, rewards;
		Unzip(transitions, 
				out initialStates,
				out actions,
				out nextStatesA,
				out rewards);
		var device = new Device("cuda");

		// Suponha que 'batch.NextState' seja uma lista de estados
		List<Tensor> nextStates = new List<Tensor>(nextStatesA);
		// Inicialize uma lista para armazenar os valores booleanos
		List<bool> maskValues = new List<bool>();
		// Preencha a lista com valores booleanos indicando se o estado é nulo ou não
		foreach (var state in nextStates)
		{
			maskValues.Add( (state != null).item<bool>() );
		}
		// Converta a lista de bools para um tensor TorchSharp
		var nonFinalMask = BoolTensor( maskValues.ToArray() );
	
		// Inicialize uma lista para armazenar os estados não nulos
		List<Tensor> nonNullStates = new List<Tensor>();

		// Preencha a lista com estados que não são nulos
		foreach (var state in nextStates)
		{
			if ( (state != null).item<bool>() )
			{
				nonNullStates.Add(state);
			}
		}
		// Concatene os estados não nulos em um tensor TorchSharp
		var nonFinalNextStates = torch.cat(nonNullStates.ToArray());
				
		var stateBatch = torch.cat(nextStates);
		var actionBatch = torch.cat(actions);
		var rewardBatch = torch.cat(rewards);
		
		var stateActionValues = policyNet.forward(stateBatch);
		
		stateActionValues = stateActionValues.gather(1, actionBatch);
		
		var nextStateValues = torch.zeros(new long[]{batchSize}, device:device);
		
		Tensor expectedStateActionValues = null;
		using (torch.no_grad())
		{
			nextStateValues[nonFinalMask] = targetNet.forward(nonFinalNextStates).max(1).values[0];
			expectedStateActionValues = (nextStateValues * gamma) + rewardBatch;
		}
		var criterion = torch.nn.SmoothL1Loss();
		var loss = criterion.call(stateActionValues, expectedStateActionValues.unsqueeze(1));
		optimizer.zero_grad();
		loss.backward();
		torch.nn.utils.clip_grad_value_(policyNet.parameters(), 100);
		optimizer.step();
		await Task.Delay(100);
	}
	
	private void Unzip(List<Transition<float[], int>> transitions, 
		out Tensor[] initialStates, 
		out Tensor[] actions,
		out Tensor[] nextStates,
		out Tensor[] rewards
	)
	{	
		int n = transitions.Count;
		initialStates = new Tensor[n];
		actions = new Tensor[n];
		nextStates = new Tensor[n];
		rewards = new Tensor[n];	
		for (int i = 0; i < n; i++)
		{
			var t = transitions[i];
			initialStates[i] = t.InitialState;
			actions[i] = t.Action;
			nextStates[i] = t.NextState;
			rewards[i] = t.Reward;			
		}
	} 
}
