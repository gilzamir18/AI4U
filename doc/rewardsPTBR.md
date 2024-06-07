# Recompensas

Há duas formas de adicionar recompensa para um agente. Não tente qualquer outra se não quiser quebrar a lógica da AI4U. 

A primeira forma, é colocando um objeto de evento de recompensa como filho do nó BasicAgent que representa o agente. Por exemplo, você pode criar um nó do tipo TouchRewardFunc, que produzirá uma recompensa toda vez que o agente tocar no objeto alvo (chamado de *Target*). Existem vários eventos de recompensa pronto nos [scripts](../addons/ai4u/dotnet/scripts/RL/events).

A segunda forma de se adicionar recompensas, é por meio da propriedade Rewards do objeto *BasicAgent* do agente. Eis um exemplo de código que usa a propriedade Rewards:

```c#
using Godot;
using System;
using ai4u;

public partial class GameManager : Node
{
	[Export]
	private BasicAgent agent;
	[Export]
	private ArrowPhysicsMoveController2D humanController;
	[Export]
	private CBMoveActuator2D actuator;
	[Export]
	private Label labelTimer;
	[Export]
	private HSlider jumperPower;
	[Export]
	private bool randomizeGravity = false;

	private bool agentOnLeftArea = true;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (jumperPower != null && humanController != null)
		{
			humanController.jumpPower = (float)jumperPower.Value;
			jumperPower.ValueChanged += (value) => humanController.jumpPower = (float)value;
		}

		agent.OnResetStart += OnReset;
		agent.OnStepStart += UpdateDisplay;
	}

	private void UpdateDisplay(BasicAgent agent)
	{
		labelTimer.Text = "Time: " + agent.NSteps;
	}

	public void OnReset(BasicAgent agent)
	{
		if (randomizeGravity)
		{	
			float k = 1.0f;
			if (GD.RandRange(0, 1) == 1)
			{
				k = 0.1f;
			}
			actuator.Gravity = 980 * k;	
		}
	
		labelTimer.Text = "Time: 0";
	}

	public void OnLeftAreaBodyEntered(Node2D node)
	{
		if (node.IsInGroup("AGENT"))
		{
			if (!agentOnLeftArea)
			{
				agent.Rewards.Add(10);
			}
			agentOnLeftArea = true;
		}
	}

	public void OnRightAreaBodyEntered(Node2D node)
	{
		if (node.IsInGroup("AGENT"))
		{
			if (agentOnLeftArea)
			{
				agent.Rewards.Add(10);
			}
			agentOnLeftArea = false;
		}
	}
}
```

. Este exemplo completo pode ser encontrado [aqui](.). Observe que a chamada *agent.Rewards.Add(10)* está adicionando 10 de recompensa ao agente. Neste caso, você pode adicionar qualquer valor do tipo *float* como argumento do método *Add*.  Mas também pode passar um segundo argumento extra, o *requestDone*. Este argumento é do tipo booleano e diz se o episódio deve terminar depois desta recompensa (*requestDone* é *true*) ou não (*requestDone* é *false*, o valor padrão).
