# AI4U

AI4U  is a protocol and an Application Programming Interface (API) that allow control Unity game objects designed with AI4U's agent abstraction framework. A Unity game object is an agent if associated with BasicAgent component. 

The environment and agent creation pipeline is:

* First create a project in the Unity.
* Install AI4U in the created project and start modeling the agent's environment using the abstractions of the AI4U Agent Abstraction Framework.
* Connect the environment and the agent with python scripts to train the neural networks that control the agent.

To control your agent through the Python language, you need to install AI4U. To do this, enter the directory [ai4u](/ai4u) and run the command:

    pip install -e .

After installing AI4U, enter the [example](/examples/scene_samplescene) directory and run the command:

    python appgym_sbtest.py

Then run the SampleScene scene in Unity and play the game. This script will execute an already trained neural network that will control the capsule with arrow that represents the agent. The agent's goal is to reach the red cube without falling off the platform (green plane in figure bellow).

# AI4U Features
* AI4U is Stable-baselines3 friendly: it has a default configuration to support stable-baseline3 (SB3) features. We keep track of SB3 features and implement an appropriate compatibility layer.

* AI4U supports many HuggingFace features. With this, you can provide characters with natural language capabilities. We provide a default sensor for main NLP models and a set of prompts to create characters with various peculiar personas.

* AI4U supports ONNX models. As such, you can make your apps without python binding after agent training. For this, it is possible to load trained ONNX models in your Unity application.

* AI4U supports 2D and 3D scenarios in Unity. We provide various made components that facilitate the development of agent-oriented games.

* AI4U supports both classic game artificial intelligence (such as the A* algorithm) and new ways of doing pathfinding (such as reinforcement learning).


# Prerequisites
AI4U and AI4UUE have been tested on three different operating systems: Windows 11, Windows 10, and Ubuntu 20.04. AI4U support Python 3.8, 3.9, and 3.10 versions.

If you are using the stable-baselines3 framework for deep reinforcement learning, AI4U only supports up to version 1.8.0. Also, we only support gym version <= 0.25.2. The commands to install the proper version of these modules are:

``` bash
pip install gym<=0.25.2
pip install stable-baselines3<=1.8.0
```
