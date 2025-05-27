# AI4U - Artificial Inteligence For You

AI4U tool AI4U is a agent framework for modeling virtual reality and game environment. This repo we keep the reference implementation for Godot Game Engine.

The Godot version of the AI4U get advantage of the Godot architecture and facilitate agent specification by means of an agent abstraction. In Godot, AI4U provides a alternative approach to modeling Non-Player Characters (NPC). Although, developers can apply this tool in others situations, for example, the modeling environment for artificial intelligence experiments.

Agent abstraction defines an agent living in a environment and interacting with this environment by means of sensors and actuators. So, NPC specification is a kind of agent specification. Agent's components are: sensors, actuators, events, reward functions and brain. Sensors and actuators are the interface between agents and environments. A sensor provides data to an agent's brain, while actuators send actions from agent to environment. A brain is a script that proccessing sensors' data e made a decision (selects an action by time).

# AI4U Components

There are two components required to run AI4U in a new project: Python scripts and C# scripts.

## Install Stable Python Scripts
To install the stable version of AI4U, run the command:

```bash
pip install ai4u
```

## Install Stable C# Scripts
Next, download the latest AI4U release [here](https://raw.githubusercontent.com/gilzamir18/AI4U/main/packages/ai4u.zip). Unzip this file and place it in the main directory of your Godot project.

## Install the Latest Python Scripts
The Python component of AI4U can be installed using a local pip package. To install it, navigate to the directory `pyplugin/ai4upy/ai4u` and run the command:

```bash
pip install -e .
```

This command fetches the latest modifications from the AI4U repository.

## Install the Latest C# Scripts
Clone the repository and copy the directory `addons/ai4u` to your project.

# Tutorials
* [Introduction](doc/introduction.md)
* [Don't Break the Game](doc/dontbreakthegame.md)
* [Events](doc/events.md)
* [AI4U and Stable-Baselines3](doc/stable_baselines3guide.md)
* [Using ONNX](doc/introductionwithonnxmodels.md)
* [Why Python?](doc/whypython.md)


# Requirements
* Godot 4.2.2 Mono Version.
* Python 3.12.
* Micrsoft.ML.OnnxRuntime.
* Gynasium.
* Tested in Windows 11 and Ubuntu 24.04.

The minimum recommended hardware for AI4U includes a GeForce 1050ti (4GB VRAM), 8GB of RAM, and at least 20GB of SSD storage. Memory requirements can significantly increase when using complex sensor inputs (like images) or employing algorithms such as Soft-Actor-Critic (SAC) and DQN. For more demanding applications, such as using SAC with an image sensor, a computer equipped with at least 24GB of RAM and a high-end GPU is essential. In gaming scenarios, we advise using modest sensor configurations, for example, by moderately utilizing RayCasting.

## Linux Performance Issue with Occluded Windows & Godot Projects

Recent Linux versions may throttle or pause applications when their windows are occluded (minimized, covered, or on an inactive desktop). This impacts background processes like simulations in Godot-based AI4U projects.

To ensure consistent performance for such tasks:

* Configure App Window to Start Minimized:
    * Path: In Godot's Project > Project Settings... > Display > Window, find the Mode property.
    * Action: Set Mode to Minimized.
    * Reason: This can prevent the OS from deprioritizing the application due to its window not being visible.
* Disable V-Sync (Vertical Synchronization):
    * Path: In Godot's Project > Project Settings... > Display > Window, find Vsync Mode or Use Vsync.
    * Action: Set Vsync Mode to Disabled (or uncheck "Use Vsync").
    * Reason: V-Sync is irrelevant for background tasks and can cause issues with occluded windows. Disabling it allows the simulation to run at maximum speed, unconstrained by display refresh rates.

These changes help AI4U simulations run reliably in the background on Linux.

# Demo Projects
[Here](https://github.com/gilzamir18/ai4u_demo_projects) we will include AI4U demo projects that you can use for experimentation.

# Maintainers
AI4U is currently maintained by Gilzamir Gomes (gilzamir_gomes@uvanet.br), Creto A. Vidal (cvidal@dc.ufc.br), Joaquim B. Cavalcante-Neto (joaquimb@dc.ufc.br) and Yuri Nogueira (yuri@dc.ufc.br).

Important Note: We do not do technical support, nor consulting and don't answer personal questions per email.
How To Contribute
To any interested in making the AI4U better, there is still some documentation that needs to be done. If you want to contribute, please read CONTRIBUTING.md guide first.

Acknowledgments
AI4U was created in the CRab (Computer Graphics, Virtual Reality and Animations) Labs at UFC (Universidade Federal do Ceará).
