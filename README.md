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

# Prerequisites
AI4U and AI4UUE have been tested on three different operating systems: Windows 11, Windows 10, and Ubuntu 20.04. AI4U support Python 3.8, 3.9, and 3.10 versions.

If you are using the stable-baselines3 framework for deep reinforcement learning, AI4U only supports up to version 1.8.0. Also, we only support gym version <= 0.25.2. The commands to install the proper version of these modules are:

``` bash
pip install gym<=0.25.2
pip install stable-baselines3<=1.8.0
```
