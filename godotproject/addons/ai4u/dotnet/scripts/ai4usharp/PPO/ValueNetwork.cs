using System;
using TorchSharp;
using TorchSharp.Modules;
using static TorchSharp.torch.nn;
using static TorchSharp.torch;
using static TorchSharp.torch.nn.functional;

namespace ai4u;

public class ValueNetwork : nn.Module
{
    private Linear fc1;
    private Linear fc2;
    private Linear fc3;

    public ValueNetwork(int stateSize) : base("ValueNetwork")
    {
        fc1 = Linear(stateSize, 128);
        fc2 = Linear(128, 64);
        fc3 = Linear(64, 1);
        RegisterComponents();
    }

    public Tensor Forward(Tensor x)
    {
        x = relu(fc1.forward(x));
        x = relu(fc2.forward(x));
        x = fc3.forward(x);
        return x;
    }
}
