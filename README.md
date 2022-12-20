# AI4U

AI4U  is a protocol and an Application Programming Interface (API) that allow control Unity and Godot game objects designed with AI4U's agent abstraction. A Unity game object and a Godot node are agents if associated with components of the BasicAgent type. 

The environment and agent creation pipeline is:

* First create a project in the game engine (Unity or Godot).
* Install AI4U in the created project and start modeling the agent's. environment using the abstractions of the AI4U Agent Abstraction Framework.
* Connect the environment and the agent with python scripts to, for example, train the neural networks that control the agent.

To control your agent through the Python language, you need to install AI4U. To do this, enter the directory [ai4u](/ai4u) and run the command:

    pip install -e .

After installing AI4U, enter the [example](/examples/scene_samplescene) directory and run the command:

    python appgym_sbtest.py

Then run the SampleScene scene in Unity/Godot and run the game. This script will execute an already trained neural network that will control the capsule with arrow that represents the agent. The agent's objective is to reach the red cube without falling off the platform (green plane in figure bellow).

[See here for full documentation on AI4U](./doc/) 

![IMAGEM](/doc/img/ai4uge_samplescene.png)


In the next image, we show the tensorboard training logs generated when training the SampleScene scene agent. To do this, we use the stable-baselines3 project.

![IMAGE](/doc/img/training_of_godot_sample_scene.png)


# Game Engine Support

AI4U support Unity or Godot scenes designed using the Framework AI4U Agent Abstraction (AI4UAA). 


AI4U supports scenes designed in Godot and Unity game engines. The AI4UAA Framework provides a set of abstractions for creating agents. The main abstractions are sensors and actuators. An agent uses sensors to obtain information about the environment and about itself and send this information to a decision-making model (a neural network, for example). Actuators represent actions that change the environment and the agent itself. For example, a set of sensors can capture the agent's position and orientation; another set of sensors can capture a target's position. All information is concatenated and sent to an agent controller, which can use a neural network to decide on an action to perform. The action the controller can perform depends on the type of actuator the agent has. For example, a motion actuator can receive a forward motion force and a torque as parameters and then apply these forces to the center of mass of the physical object that represents the agent.

There is an AI4UAA implementation for Unity and one for Godot. Each implementation has its own specifics and limitations. Therefore, AI4U is just the component that connects a Python controller to one of the versions of AI4UAA, either for Unity or for Godot.


[AI4U Unity Edition (AI4UAA for Unity)](https://github.com/gilcoder/AI4UUE)

[AI4U Godot Edition (AI4UAA for Godot)](https://github.com/gilcoder/AI4UGE)