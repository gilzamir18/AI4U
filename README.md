# AI4U

AI4U  is a protocol and an Application Programming Interface (API) that allow control Unity and Godot game objects designed with AI4U's agent abstraction. A Unity game object and a Godot node are agents if associated with components of the BasicAgent type. 

The environment and agent creation pipeline is:

* First create a project in the game engine (Unity or Godot).
* Install AI4U in the created project and start modeling the agent's environment using the abstractions of the AI4U Agent Abstraction Framework.
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


A full example of the game Pong using AI4UEE:

![](https://public.by.files.1drv.com/y4mE4z_1xivtrP8mdLnopcJSoad1Vs70jnclJtfQrK5GTBCXjnVVfavVBvTgizC0ytDV4acsbPokboN_tnW8iIppCDHZs1OP1ZJ0_NRh5f2T5DTDSrXSIauYIhPOalXStNutHBQ3StqPYfHcseiwq6kqFQasiuaDN_ozHHnkRkIPDOo3Wn2JTat0XamQo0JxU7jlxYSiUzP4TECDSZDGXWh2KHbKBYOtlXXLhjKQNE5ziw)


The training logs show that:

![Gráfico](https://by3301files.storage.live.com/y4mBFID5H01I_Z5o5VdQ_dAYnAP-eh_MsDKZpWCywqhqx-BMvzHbtD23roz99QqsdmE5BncH0c59wy6OEkVyE7TsblGg-In_CY29MQ81MRzXmrIOwO2Q2XhSy9kcHFSLGneVhOlDB7KYvCsKF0nXYTkWbmihxz_1IeKyBR7qlk_lAFA6dFtbISekGqKNlmFkC110-E6CXpkIqsMYRLzoJKbRjjnpPcziXRnpTU6WkJu7c0?encodeFailures=1&width=384&height=311). 

Pong project is [here](https://github.com/gilzamir18/PhongDemo).


# Game Engine Support

AI4U support Unity or Godot scenes designed using the Framework AI4U Agent Abstraction (AI4UAA). 


AI4U supports scenes designed in Godot and Unity game engines. The AI4UAA Framework provides a set of abstractions for creating agents. The main abstractions are sensors and actuators. An agent uses sensors to obtain information about the environment and about itself and send this information to a decision-making model (a neural network, for example). Actuators represent actions that change the environment and the agent itself. For example, a set of sensors can capture the agent's position and orientation; another set of sensors can capture a target's position. All information is concatenated and sent to an agent controller, which can use a neural network to decide on an action to perform. The action the controller can perform depends on the type of actuator the agent has. For example, a motion actuator can receive a forward motion force and a torque as parameters and then apply these forces to the center of mass of the physical object that represents the agent.

There is an AI4UAA implementation for Unity and one for Godot. Each implementation has its own specifics and limitations. Therefore, AI4U is just the component that connects a Python controller to one of the versions of AI4UAA, either for Unity or for Godot.


[AI4U Unity Edition (AI4UAA for Unity)](https://github.com/gilcoder/AI4UUE)

[AI4U Godot Edition (AI4UAA for Godot)](https://github.com/gilcoder/AI4UGE)