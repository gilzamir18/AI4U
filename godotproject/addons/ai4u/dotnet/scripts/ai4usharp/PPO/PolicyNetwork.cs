using System;
using TorchSharp;
using TorchSharp.Modules;
using static TorchSharp.torch.nn;


namespace ai4u;

public class PolicyNetwork : Module
{
	private Linear fc1;
    private Linear fc2;
    private Linear fc3;

    public PolicyNetwork(int stateSize, int actionSize) : base("PolicyNetwork")
    {
        fc1 = Linear(stateSize, 128);
        fc2 = Linear(128, 64);
        fc3 = Linear(64, actionSize);
        RegisterComponents();
    }

    public torch.Tensor Forward(torch.Tensor x)
    {
        x = torch.nn.functional.relu(fc1.forward(x));
        x = torch.nn.functional.relu(fc2.forward(x));
        x = torch.softmax(fc3.forward(x), 1);
        return x;
    }
}
