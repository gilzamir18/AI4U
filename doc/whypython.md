# Why Python?

The standard project flow for an experiment using AI4U is:

1. Model the agent and environment in Godot using the AI4U *dotnet* addon.
2. Write the Python script to train the agent.
3. Write a Python script to test the agent.
4. If the test performance is acceptable, convert the agent model from PyTorch to ONNX and run the model directly in Godot. Otherwise, redo steps 1, 2, and 3.

This flow would be simpler if everything ran within Godot, which may lead many to ask: why doesn't AI4U run training directly in C# or GDScript in Godot? The truth is that DRL (Deep Reinforcement Learning) implementations in Python have been around for a long time and are naturally quite robust. Besides, there's a complete ecosystem of auxiliary libraries for various tasks that can be useful during training. Additionally, Python can be used during training and then discarded.

There is not yet a robust DRL implementation in GDScript. In C#, although we have TorchSharp, there still isn't as comprehensive a DRL implementation as, say, *stable-baselines3*. I am currently implementing a prototype with only the PPO algorithm, but it is still very rudimentary and does not support half of the features found in implementations like stable-baselines3. Therefore, Python still has competitive advantages, and the effort to link Python with C# is extensive. An additional motivation is that replacing Godot's C# module with GDScript communicating with Python would be much easier than implementing all modules directly in GDScript. When I have time, I will implement a full version of AI4U entirely in GDScript communicating with Python. Using Godot extension, it is possible to use a dynamic library to read and execute an ONNX model. If anyone wants to contribute to this, contact me at: [here](gilzamir_gomes@uvanet.br).
