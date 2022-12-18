# AI4U Python Edition (AI4UPE) - Developer's Guide

## Introduction
For a long time, Artificial Intelligence (AI) researchers were limited to using test tables based on games that others built, such as the classic game of Chess or Atari games altered for AI experiments. Today's game engines make it possible to build much more complex worlds, but still controllable. And this is done intuitively. AI4U's goal is to connect AI researchers and students with game development technologies so that they can build their own algorithm testing environments. For this, we built an API that connects the tools most used by industry and academia to a programming language that naturally supports the most modern tools for modeling Artificial Intelligence experiments, which is the Python language. But not only that, we provide a modular agent specification framework. This framework allows the design of agents in virtual environments built in sophisticated game engines, such as Unity.

In this guide, we show the architecture of the AI4U component called the AI4U Python Edition, which allows you to control game engine objects through the Python language. But, it goes beyond that, it allows controlling agents built using a modular agent specification framework.

## Architecture

AI4UPE lets you control an agent in Unity and Godot in a similar way. You don't need to have two different scripts, as the communication protocol between the Python code and the game engine is the same, whether it's Unity or Godot. For this, it is important to understand that every agent in the game engine environment has an identifier (ID) that can be addressed via Python, using AI4UPE. For each agent in the game engine environment, an AI4UPE controller must be created that implements the communication protocol with the agent. In Python, this controller is an object that is initialized by the *startasdaemon* method of the *ai4u.appserver* package. The controller interprets the state of the environment perceived by the agent created within the game engine and sends the actions in the format that this agent understands. Note that it is the role of the programmer to adjust the name, type and format of the agent's perceptions (data sent by sensors) and actions (data sent to actuators).

Therefore, AI4U has an architecture summarized in Figure 1.

![Arquitetura da AI4U](/doc/img/ai4ucomps.png)

*Figure 1. AI4U architecture showing its four main components: the function ai4u.appserver.startasdaemon (in short, startasdaemon), an object that inherits from BasicController and is initialized by the function stardaemon, an object of type ControlRequestor associated with an item of game (agent). The ControlRequestor establishes the agent's loop and controls the game item associated with the script of type BasicAgent*.

BasicController is the ai4u.agents.BasicController class and provides the basic interface to control an agent of type BasicAgent. BasicController uses the AI4UPE protocol to abstract the communication between the Python code and the agent created in the game engine.

# Example
In this directory [examples/ai4upe](/examples/), there are example controllers for three scenes. The *scene_samplescene* scene is implemented in both Godot and Unity. The code in **app.py** can control via manual commands the agent represented by the capsule body with arrow shown in Figure 2 (the left side has the agent in Godot and the right side has the agent in Unity).

![Agent](/doc/img/agentgu.png)

Let's implement a manual controller (the user himself sends commands through standard input) for this scene. First we import the modules that contain the components we need.

```
import ai4u
from controller import SimpleController
from ai4u.appserver import startasdaemon
from ai4u import utils
```

The *SimpleController* component inherits from ai4u.agents.*BasicController* and implements a specific form of communication with the agents of the scenes of the project [Donut in Unity](https://github.com/gilcoder/AI4UUE/) and of the project [AI4GTesting by Godot](https://github.com/gilcoder/AI4UGE/examples/). The *utils* component, among other features, provides the import_getch function, which can be used in place of standard input in Python.

After importing the necessary modules, you must instantiate the controller and specify the ID of the controlled agent:

```
# in this case there is only one identify, as there is only one agent.
ids = ["0"]

# There is a controller for each agent.
# We use the classes.
controllers_classes =  [SimpleController]

#The method *startasdaemon* creates an instance of the controller and initilize it in a separated thred.

controller = startasdaemon(ids, controllers_classes)[0]
```

The *startasdaemon* method receives the list of agent identifiers and a list of corresponding controllers and then instantiates the controller in a *thread* of type *daemon* and returns the list of instantiated controller objects. In the last line of code, we get the only returned controller on the same line. With this object, we can send commands to the agent modeled in the game engine and that runs in an instance of the game we created:

```
# Reset environment's episode.
state = controller.request_reset()
```

In this example, the controller command *request_reset* requests the restart of the agent's environment, generating a new episode. The agent's lifetime is divided into episodes. The episode only starts after the *request_reset* command has been executed.

We can also send stocks to the agent. The set of actions that we can send depends on how the agent was modeled, that is, on the sensors and actuators that the researcher added to the agent. Because of this, the SimpleController class had to be coded specifically for the *SampleScene* scene of the Donut sample project (Unity) or the AI4UTesting sample project (Godot). In this scene, the agent has a capsule body as shown in Figure 2 and can move around on a flat platform. The agent has to reach the cube avoiding leaving the platform. If the agent leaves the platform, he falls and dies. This agent supports an action named "move" that receives four real numbers in a vector *[fb, lr, j, jf]*, whose values represent:

* *fb* a value in the real interval [-1, 1], if the value is positive it performs a forward movement with a speed proportional to the magnitude of *fb*; if negative, move backwards proportional to *fb*.
* *lr* amount of degrees the agent can rotate (if positive, rotates to the agent's left, if positive, rotates to the agent's right).
* *j* intensity of the upward jump (if it is less than or equal to zero, the agent does not jump).
* *jf* forward jump intensity (if less than or equal to zero, the agent does not jump).

To send such a command, just execute the *request_step* method of the *controller*:

```
state, reward, done, info = controller.request_step(action)
```

where *action* is a vector of four real numbers as just described; The *request_step* method returns a tuple with four elements by default:

* state: the current state observed by the agent;
* reward: the sum of rewards received during the agent's decision cycle;
* done: if the episode ended after the last action; and
* info: extra information sent by the environment itself, usually the same as *state*.

The [app.py](/examples/scene_samplescene/app.py) file goes further and adds code for user interaction. To run this example, first run the script using the Python language:

```
state, reward, done, info = controller.request_step(action)
```

where *action* is a vector of four real numbers as just described; The *request_step* method returns a tuple with four elements by default:

* state: the current state observed by the agent;
* reward: the sum of rewards received during the agent's decision cycle;
* done: if the episode ended after the last action; and
* info: extra information sent by the environment itself, usually the same as *state*.


The [app.py](/examples/scene_samplescene/app.py) file goes further and adds code for user interaction. To run this example, first run the script using the Python language:

    python app.py

Then run the scene in your preferred game engine (Godot or Unity). Note that you run the scene only after running the *script* that uses AI4UPE, otherwise the scene will be closed due to lack of communication with AI4UPE.

# Creating your own controller
A controller inherits is a class that implements the functions of ai4u.agents.BasicController. These are *callback* functions that, when executed, indicate special events that have occurred in the scene environment.

* **handleNewEpisode(self, info)**: this function is executed when a new episode starts.
* **handleEndOfEpisode(self, info)**: This function is executed after an episode ends.
* **handleConfiguration(self, id, max_step)**: this function is executed when the agent connects to the controller and indicates the agent identification (parameter **id**) and the maximum amount of time steps that the episode supports (parameter **max_step**). If *max_step <= 0*, the episode has no time limit.
* **reset_behavior(self, info)**: this function is called whenever it is necessary to return a new state to a decision-making model (a neural network, for example). It will be the return of this function that the *reset* method of the **Gym** environment of AI4U will return.
* **step_behavior(self, action)**: this function will be called whenever the *step* method of the AI4U Gym environment is executed with an action indicated by the **action** parameter. We have to translate this structure into **action** to the appropriate name and argument list for the active actuator type of the agent in the game engine scene. For that, for each case of **action**, you must specify the name of the action (*self.actionName*) and the list of arguments that the action needs to receive in *self.actionArgs*.

To create a controller for a new scene, it is necessary to map the perceptions that the scene sends to the controller and the actions that the controller has to send to the agent to execute in the scene's environment. For this, the base class *ai4u.agents.BasicController* provides pre-defined attributes that allow specifying the name of the action to be executed, action arguments, action domain and observations domain (states). -if you specify the values of the *action_space* and *observation_space* attributes, which must be a *gym.spaces* object, as in the following example:

![Controller Spaces](/doc/img/controller_spaces.png)

When the *step_behavior* function is executed, it means that the agent has already defined an action and passed this action with a specific format given in the function's *action* parameter. For example, *action* can be a vector *(d, b, l, r)*, where *d* is the amount of steering turn in degrees, *b* is the amount of braking, l is a binary value which indicates the *status* of the left turn signal (1 indicates if the left turn signal is on and 0 if it is off) and *r* is also a binary value that indicates the *status* of the right turn signal. Let's say that this action must be associated with an actuator of a car agent in the game named **move**. Thus, the *actionName* attribute must be modified to "move" and the *actionArgs* attribute must be modified to contain a list *[d, b, l, r]*, as shown in the following figure:

![request_step.png](/doc/img/request_step.png)

The [Donut scene controller](/examples/scene_donut/controller.py) (available in both Unity and Godot AI4UAAF samples) is a more complex case of controller. Instead of creating a controller from scratch, use this example to get started.

# Limitations

AI4U does not yet natively support communication between many agents. But multiple agents running simultaneously are supported. More documentation on this aspect will be made available soon.

