using System;
using System.Collections.Generic;
using TorchSharp;
using TorchSharp.Modules;
using static TorchSharp.torch.nn;
using static TorchSharp.torch;
using static TorchSharp.torch.nn.functional;

namespace ai4u;

public class PPO
{
    private PolicyNetwork policyNet;
    private ValueNetwork valueNet;
    private Adam optimizerPolicy;
    private Adam optimizerValue;
    private float clipParam = 0.2f;
    private float gamma = 0.99f;
    private float lambda = 0.95f;


	public Tensor PolicyForward(Tensor x)
	{
		return policyNet.Forward(x);
	}

	public Tensor ValueForward(Tensor x)
	{
		return valueNet.Forward(x);
	}

    public PPO(int stateSize, int actionSize)
    {
        policyNet = new PolicyNetwork(stateSize, actionSize);
        valueNet = new ValueNetwork(stateSize);
        optimizerPolicy = torch.optim.Adam(policyNet.parameters(), lr: 1e-3);
        optimizerValue = torch.optim.Adam(valueNet.parameters(), lr: 1e-3);
    }

    public (Tensor, Tensor, Tensor, Tensor) Update(Tensor states, Tensor actions, Tensor rewards, Tensor oldLogProbs, Tensor values)
    {
        // Calcular vantagens e targets
        var advantages = CalculateAdvantages(rewards, values);
        var targets = rewards + gamma * values;

        // Atualizar rede de políticas
        optimizerPolicy.zero_grad();
        var newLogProbs = torch.log(policyNet.Forward(states));
        var ratio = torch.exp(newLogProbs - oldLogProbs);
        var surr1 = ratio * advantages;
        var surr2 = torch.clamp(ratio, 1 - clipParam, 1 + clipParam) * advantages;
        var policyLoss = -torch.min(surr1, surr2).mean();
        policyLoss.backward();
        optimizerPolicy.step();

        // Atualizar rede de valor
        optimizerValue.zero_grad();
        var newValues = valueNet.Forward(states);
        var valueLoss = (newValues - targets).pow(2).mean();
        valueLoss.backward();
        optimizerValue.step();
		return (policyLoss, valueLoss, newLogProbs.detach(), newValues.detach());
    }

    private Tensor CalculateAdvantages(Tensor rewards, Tensor values)
    {
        var advantages = torch.zeros_like(rewards);
        float advantage = 0;
        for (long i = rewards.shape[0] - 1; i >= 0; i--)
        {
            advantage = rewards[i].item<float>() + gamma * advantage - values[i].item<float>();
            advantages[i] = advantage;
        }
        return advantages;
    }
}
