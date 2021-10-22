# AI4U for Godot
This is the Godot version of the AI4U tool. The Godot version of the AI4U get advantage of the Godot architecture and facilitate agent specification by means of an agent abstraction. In Godot, AI4U provides a alternative approach to modeling Non-Player Characters (NPC). Although, the developer can apply this tool in others situations as the environment modeling as an artificial intelligence test bed. 

Agent abstraction defines an agent living in a environment and interacting with this environment by means of sensors and actuators. So, NPC specification is a kind of agent specification. Agent's components are: sensors, actuators, events, reward functions and brain. Sensors and actuators are the interface between agents and environments. A sensor provides data to an agent's brain, while actuators send actions from agent to environment. A brain is a script that proccessing sensors' data e made a decision (selects an action by time).

We map components of the Godot architecture to agents components. So, agents components are stored as subscenes. A subscene is asubtree of the nodes. A node is a visible or not visible element of the game. Furthermore, the visible elements are statics or dynamics. Shortly, we will publish a full article on this relationship between Godot and agent abstraction. 


# 3D-Platformer
The Godot version of the AI4U is availale as a basic 3D platformer made in Godot (adapted from https://github.com/Janders1800/3D-Platformer). Thus, you can download this version and get a fuctional example on the hand. However, AI4U for Godot can be installed on any game. For this, you must make a copy of ai4u file in your project. While we don't produce more robust documentation, we recommend that you download this directory and import the project into it for Godot. So study the code, see the connection between the components and experiment.

## Controls
The basic 3D plataformer has a simple controller with WASD movements. For Jump action, press the key U.


## Training
You can train the agent by exchanging the component called LocalBrain for a RemoteBrain component. Then you must compile and run the scene and then run the training_scripts/sacagent.py script.

# Requirements

*	Godot C# version 3.3 or upper.
*	Python 3.6 or upper if you use python machine learning frameworks.	 