
# AI4U for Unity

This is the Unity version of the AI4U tool. The Godot version of the AI4U get advantage of the Unity architecture and facilitate agent specification by means of an agent abstraction. In Unity, AI4U provides a alternative approach to modeling Non-Player Characters (NPC). Although, the developer can apply this tool in others situations as the environment modeling as an artificial intelligence test bed. 

Agent abstraction defines an agent living in a environment and interacting with this environment by means of sensors and actuators. So, NPC specification is a kind of agent specification. Agent's components are: sensors, actuators, events, reward functions and brain. Sensors and actuators are the interface between agents and environments. A sensor provides data to an agent's brain, while actuators send actions from agent to environment. A brain is a script that proccessing sensors' data e made a decision (selects an action by time).

We map components of the Unity architecture to agents components. So, agents components are stored as prefabs. A prefab is made component of game objects. A Game Object is a visible or not visible element of the game. Furthermore, the visible elements are statics or dynamics. Shortly, we will publish a full article on this relationship between Unity and agent abstraction. 


# Exemples
There are two demo scenes in AI4UTesting project [here](../../examples/Unity/AI4UTesting.zip):

*   SampleScene
*   DonutScene

## Installation
First, it needs to install *clientside* component. Open the Ubuntu command-line console and enter in directory *clientside/ai4u*. Run the following command:

```
pip3 install -e .
```

Waiting for the installation ends, this may take a while. To install gym and a3c algorithm support, go to directory *clientside/gym* and run the following command:

```
pip3 install -e .
```

Waiting for the installation ends.

Now, run Unity and open project available in *exemples/Unity/AI4UTesting.zip*.  Then open the SampleScene scene (available on *Assets/scenes*) if it is not already loaded.

Then, run *appgym_sb3train.py* script located in *examples/clientnewapi/scene_samplescene/*.

```
python3 appgym_sb3train.py
```

Then press play button in Unity Editor.

If no error message was displayed, then AI4U was installed correctly.

# Requirements

Tested Systems
----------

- Operation System: Windows 11
     * Python >= 3.7.5 (from microsoft store).
     * Unity >= 2021.3.9f1.

Unfortunately, AI4U does not have support for mobile applications.
