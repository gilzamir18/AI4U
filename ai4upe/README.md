# AI4U Python Edition

AI4U Python Edition (AI4UPE) is a protocol and an Application Programming Interface (API) that allow control Unity and Godot itens designed with AI4U's agent abstraction. A Unity game object and a Godot node are agents if associated with components of the BasicAgent type. See the developer guide to understand how to turn a game item into an agent: [DevGuide](/doc/).

To control your agent through the Python language, you need to install AI4UPE. To do this, enter the directory [ai4upe](/ai4upe/ai4u) and run the command:

    pip install -e .

After installing AI4UPE, enter the [example](/examples/ai4upe/scene_samplescene) directory and run the command:

    python appgym_sbtest.py

Then run the SampleScene scene in Godot and run the game. This script will execute an already trained neural network that will control the capsule with arrow that represents the agent. The agent's objective is to reach the red cube without falling off the platform (green plane in figure bellow).

[See here for full documentation on AI4UPE](./doc/) 

![IMAGEM](/ai4upe/doc/img/ai4uge_samplescene.png)