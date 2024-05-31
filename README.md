# AI4U - Artificial Inteligence For You

AI4U tool AI4U is a agent framework for modeling virtual reality and game environment. This repo we keep the reference implementation for Godot Game Engine.

The Godot version of the AI4U get advantage of the Godot architecture and facilitate agent specification by means of an agent abstraction. In Godot, AI4U provides a alternative approach to modeling Non-Player Characters (NPC). Although, developers can apply this tool in others situations, for example, the modeling environment for artificial intelligence experiments.

Agent abstraction defines an agent living in a environment and interacting with this environment by means of sensors and actuators. So, NPC specification is a kind of agent specification. Agent's components are: sensors, actuators, events, reward functions and brain. Sensors and actuators are the interface between agents and environments. A sensor provides data to an agent's brain, while actuators send actions from agent to environment. A brain is a script that proccessing sensors' data e made a decision (selects an action by time).

# Use ai4upy for Training
The Python side of the AI4U can be installed using local pip package. To install it, enter the directory pyplugin/ai4upy/ai4u and run the command:

    pip install -e . 

Additionally, install the libraries related to training. To do this, navigate to the `pyplugin` directory and run the following command:

    pip install -r sb3_requirements.txt

This command allows you to train and run an agent within a Godot scene.

# Tutorials

* [Introduction](doc/introduction.md)


# Requirements
* Godot 4.2.2 Mono Version.
* Python 3.12.
* Micrsoft.ML.OnnxRuntime.
* Gynasium.
* Tested in Windows 11 and Ubuntu 24.04.

# Demo Projects
[Here](https://github.com/gilzamir18/ai4u_demo_projects) we will include AI4U demo projects that you can use for experimentation.


# Maintainers
AI4U is currently maintained by Gilzamir Gomes (gilzamir_gomes@uvanet.br), Creto A. Vidal (cvidal@dc.ufc.br), Joaquim B. Cavalcante-Neto (joaquimb@dc.ufc.br) and Yuri Nogueira (yuri@dc.ufc.br).

Important Note: We do not do technical support, nor consulting and don't answer personal questions per email.
How To Contribute
To any interested in making the AI4U better, there is still some documentation that needs to be done. If you want to contribute, please read CONTRIBUTING.md guide first.

Acknowledgments
AI4U was created in the CRab (Computer Graphics, Virtual Reality and Animations) Labs at UFC (Universidade Federal do Ceará).
