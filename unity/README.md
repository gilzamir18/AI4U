
# AI4U for Unity
### **Unfortunately, AI4U has been discontinued and is in maintenance mode. For a modern agent-oriented game modeling framework with support for reinforcement learning and natural language processing, see the BeMaker project (https://github.com/gilzamir18/Bemaker).**
This is the Unity version of the AI4U Agent Abstraction Framework (AI4UAAF). AI4UAAF provides a alternative approach to modeling Non-Player Characters (NPC).

Agent abstraction defines an agent living in a environment and interacting with this environment by means of sensors and actuators. So, NPC specification is a kind of agent specification. Agent's components are: sensors, actuators, events, reward functions and brain. Sensors and actuators are the interface between agents and environments. A sensor provides data to an agent's brain, while actuators send actions from agent to environment. A brain is a script that proccessing sensors' data e made a decision (selects an action by time).

We map components of the Unity architecture to agents components. So, agents components are stored as prefabs. A prefab is made component of game objects. A Game Object is a visible or not visible element of the game. Furthermore, the visible elements are statics or dynamics. Shortly, we will publish a full article on this relationship between Unity and agent abstraction.

# AI4UUE and MLAgents
IA4UUE is not the same as MLAgents. For sure AI4UUE has less features than MLAgents. But our idea is to provide a more flexible and modular way of developing and reusing code. AI4UUE doesn't promise to be any easier to configure than MLAgents, but it is easier to extend by creating reusable sensors, actuators and reward functions.

Another difference of AI4UUE from MLagents is that we facilitate the use of third-party training algorithms. AI4UUE's integration with environments that use the OpenAI Gym protocol is built in by default into the programming interface, allowing the programmer to choose any AI Python framework that is already working with environments that follow the Gym standard. We use stable-baselines3 in the AI4U sample scripts to demonstrate the use of the stable-baselines3 framework, but any other machine learning API could be used. In addition, the programmer can easily integrate his own machine learning solution into AI4UUE.

We are still working on getting high-performance and more sophisticated demos of our framework. Soon we will also add support for using neural networks completely within Unity, without relying on Python scripts, as is currently the case with MLAgents.

# Compatibility

AI4UAAF for Unity was tested in Ubuntu 22.04, PopOS 22.04 and Windows 11. In Ubuntu and PopOS, better experience was installing Unity Hub beta and **Unity Editor 2022.2.1f1**. The best scenario is to use Unity and AI4U in Windows environment (10 or 11).


# Setup

AI4UAAF is a Unity package that can be installed locally. Create a new 3D project in Unity. From the Window menu, select the "Package Manager" option. In the Package Manager window, click on the "add package from disk" option. The following image shows the entire process of installing the package and opening a ready-to-train scene.

[![IMAGE ALT TEXT HERE](https://img.youtube.com/vi/O8rYrfaTSFI/0.jpg)](https://youtu.be/O8rYrfaTSFI)
