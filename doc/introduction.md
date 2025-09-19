# Introduction

AI4U (Artificial Intelligence for You) is an open-source tool that provides an abstraction for modeling behaviors in Godot based on Artificial Intelligence. The agent abstraction is used to allow for the sharing of solutions. Each component of an agent can be produced, refined, shared, and reused. An agent's components are: sensors, actuators, reward functions, a controllable item, and a controller.

The agent perceives the game world through sensors and acts upon this world through actuators. The mapping between sensors and actuators is performed by a controller. Based on the history of observations, the controller decides which action to execute. These actions change the agent's controllable object or the surrounding environment.

This tutorial is based entirely on Godot version 4.4.1 (.NET version). The [official Godot documentation on using C\#](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_basics.html) is a good starting point for a complete understanding of this tutorial.

# How to Implement an Agent in Godot

In Godot, an agent is composed of a virtual body, sensors, actuators, and a controller. A virtual body can be a RigidBody2D, CharacterBody2D, RigidBody3D, CharacterBody3D, Node2D, Node3D, or any other object that the agent has control over.

> However, for a controllable object to be useful, it is necessary that actuators and sensors can be created specifically for it. For now, these are the implemented sensors and actuators and the types of bodies they support:

| Actuator/Sensor/Controller | Role | Type |
|---|---|---|
| CBMoveActuator | Actuator | CharacterBody3D |
| RBMoveActuator | Actuator | RigidBody3D |
| RBMoveActuator2D | Actuator | RigidBody2D |
| RBRespawnActuator\* | Actuator | RigidBody3D |
| CBRespawnActuator\* | Actuator | CharacterBody3D |
| DiscreteActuator | Actuator | Node2D/Node3D |
| Camera3DSensor\*\*\* | Sensor | Node3D |
| ActionSensor | Sensor | Node2D/Node3D |
| LinearRayRaycasting\*\*\* | Sensor | Node3D |
| OrientationSensor\*\*\* | Sensor | Node3D |
| RewardSensor | Sensor | Node2D/Node3D |
| ScreenSensor | Sensor | Node2D/Node3D |
| GroundStatusSensor\*\*\* | Sensor | Node3D |
| ArrowPhysicsMoveController\*\*\* | Controller | Node3D |
| DiscreteArrowController | Controller | Node2D/Node3D |

\* These actuators are only used to position the agent somewhere in the scene at the start of an episode.

\*\* Can be any Node2D or Node3D object or any of their subtypes. The object must support the **SetAction(action)** message, which takes the **action** argument, an integer greater than or equal to zero.

\*\*\* These sensors can be applied to any supported Node3D (in this case, RigidBody3D and CharacterBody3D).

-----

## Installing AI4U

The best way to install AI4U in your project is by downloading the [AI4U repository](https://github.com/gilzamir18/AI4U) to your computer.

Then, create a new C\# project in Godot (leave the default project creation options).

Copy the **addons** directory from the AI4U repository into your project. This is enough for Godot to recognize the classes you need to model an agent. But, to train this agent using reinforcement learning, you need to install the [*AI4U/pyplugin* component](https://github.com/gilzamir18/ai4u). This plugin allows you to connect your agent in Godot with a Python framework, which enables you to train the agent. AI4U and the *pyplugin* were specially designed to communicate properly with the [stable-baselines3 framework](https://github.com/DLR-RM/stable-baselines3).

Once you have installed AI4U, pyplugin, and **stable-baselines3**, continue reading this tutorial.

-----

# A Basic Example

In Godot, an agent's structure is a tree of nodes whose root is the controllable object (the agent's body). This structure has minor variations, depending on the controllable object.

## Creating a Rigid Body Agent

Now, let's modify your project to create a rigid body agent. Create a project named [BoxChase](https://github.com/gilzamir18/ai4u_demo_projects/tree/main/BoxChase). You need to create a scene where our agent will live and interact with objects. For now, let's create a very simple scene. To do this, choose the **3D Scene** option, as shown in *Figure 1*. Use the shortcut CTRL+S to save the scene, as shown in *Figure 2*.

*Figure 1. The scene creation options are shown in the upper left panel.*

*Figure 2. Saving a scene.*

By now, I assume you have already copied the *addons* directory from the AI4U repository into your project. If so, your project should look like what's shown in *Figure 3*.

*Figure 3. Project with AI4U assets.*

-----

## Creating the Environment

Now let's create the scene. The scene is composed of a flat floor with a box and an agent (represented by a capsule) on the floor. The agent can move in the scene and its goal is to get closer to and touch the box. The agent gets a reward every time it gets closer to the box and a higher reward when it touches it. The episode ends when the agent touches the box, so the touch should generate a high reward. The agent is penalized when it falls off the plane that represents the ground. In this case, the episode also ends.

We will model the ground as a plane. To do this, create a **StaticBody3D** object as a child of Node3D (the root node of the scene). A **StaticBody3D** object must also have a **CollisionShape3D** or **CollisionPolygon3D** object as a child. We will use **CollisionShape3D**. And then, in the **Shape** resource of this object, create a **Shape** of type **BoxShape3D**, with the **Size** property parameters modified to *x=10m*, *y=0.1m*, and *z=10m*. Also, add a **MeshInstance3D** object as a child of the **StaticBody3D**, which represents the visual mesh of the scene's floor. Change the x, y, and z parameters of the **Scale** property to *x=10*, *y=0.1*, and *z=10*. *Figure 4* shows these configurations.

*Figure 4. Hierarchy of the StaticBody3D Object and configuration of its mesh.*

Now let's create a box that represents our target, the object to be touched. The process for creating this box is very similar to the floor, except at the root we place a **RigidBody3D** object, as shown in *Figure 5*. The target's **CollisionShape3D** object must have a **BoxShape3D** shape and a **MeshInstance3D** with a **BoxMesh**. We can leave these target objects with their default properties, except for the RigidBody3D, which we move to the position indicated in *Figure 5*.

*Figure 5. Hierarchy of the RigidBody3D Object and configuration of its Transform component.*

These objects will be fixed in the scene. Change the name of the **StaticBody3D** node to **Floor**; and in the **RigidBody3D**, change the name to **Target**. Thus, our scene looks like what's shown in *Figure 6*.

*Figure 6. Renaming the scene components.*

## Agent Structure

Now let's create an agent that learns to approach and touch the target without falling off the plane. To do this, create a **RigidBody3D** node and add a **CollisionShape3D** and a **MeshInstance3D** as we did for the target, except that the Shape and the Mesh must be of type **CapsuleShape3D** and **MeshInstance3D** (with Mesh configured to be a capsule), respectively. Position the capsule to be on the plane. Also, change the colors of the plane, the target, and the agent. This can be done by changing the **Surface Material Override** field of each object's **MeshInstance3D**. The partial agent structure is shown in *Figure 7*.

*Figure 7. Partial version of the scene.*

Note that the agent as a 3D object should have a forward direction. In Godot, we use the z-axis to indicate a 3D object's forward direction. However, this axis is not visible, and when the agent is moving in the scene, we won't know which side is its front, as the capsule is symmetrical. To solve this, in *ai4u\_prefabs3d/3DModel*, select the Arrow.dae model and drag it as a child of **RigidBody3D**. Change the dimensions and orientation of the arrow to look like what's shown in *Figure 8*.

*Figure 8. Partial version of the scene with the agent's front face indicated by a red arrow.*

We have completed the agent's physical body and appearance, but we haven't done much about how the agent perceives the environment and acts upon this world. But before we do that, let's change the **RigidBody3D** node's name to **AgentBody**, so we know it's the agent.

The agent needs a main module called **RLAgent**, capable of coordinating its sensors and actuators through a controller.

To create an **RLAgent** node, create a child node of the **AgentBody** node and change its name to **Agent**. In this node's **script** property, place the **RLAgent** script (use the **Quick Load** property to do this). As soon as you add this script, nothing will change in the **Inspector**. Godot will only show the properties of the added component after a new compilation. Therefore, set the current scene as the main scene of the project. Then, click **build** to compile the project for the first time. Then, run CTRL+F5. A black screen will open because there are no cameras or lights in the scene. But this first compilation was just for Godot to recognize the properties of the **RLAgent** object.

> For the compilation to work correctly, you need to create a C\# solution. To do this, in the Project menu, select Tools\>\>C\#\>\>"Create C\# Solution". That's it, your project can now be compiled correctly.

After compiling the project for the first time, the **RLAgent** script properties will be shown in the Godot **inspector**. Configure these properties as shown in *Figure 9*.

*Figure 9. Agent settings. The **Remote** option indicates that the agent will communicate with a remote controller (a Python script). When remote is unchecked, you must use either the NeuralNetController (to load a neural network directly into Godot) or an ArrayPhysicsMoveActuator combined with RBMoveActuator or RBMoveActuator2D (to control the agent directly via the arrow keys). The last option is excellent for testing the game's physics before training the agent. The **Max Steps Per Episode** option indicates the maximum number of time steps per episode.*

*Figure 10* shows the current project structure.

*Figure 10. Current project structure.*

Our agent is still very simple, without sensors and actuators. It is necessary to add actuators and sensors to the agent. We will use an **RBMoveActuator** type actuator and a **Camera3DSensor** type sensor. Add an **RBMoveActuator** type actuator as a child of the **Agent** node with the settings shown in *Figure 11*. Take the opportunity to create the node structure shown in *Figure 11*. To create the child nodes of **Agent**, right-click on this node and select the option that leads to the creation of a new node. Select the generic **Node** option and change the name of this node to one of the names shown in *Figure 11*, below **Agent**. Then, under...

> Note: Notice that whenever we create an AI4U object, we create a generic **Node** type node. And then we add an AI4U script, such as **RLAgent** and **RBMoveActuator**. From now on, to simplify the writing, we will just say: create an **RLAgent** type node.

*Figure 11. Agent actuator.*

In *Figure 12*, I show the node named **Vision** containing a **Camera3DSensor** type script.

*Figure 12. Properties of the agent's Vision sensor, which will produce a sequence of 3 grayscale images with a resolution of 61x61 pixels each.*

The **Vision** sensor is of the type that allows the agent to view the world from the point of view of a **Camera3D** type camera placed in the scene. We place this camera as a child of the **AgentBody** node. That way, whenever the agent rotates, the camera rotates the agent's view. Note that the **Camera3DSensor**'s **Camera** property points to an object named **AgentCam**, which is a **Camera3D** object, as shown in *Figure 13*.

*Figure 13. The agent's camera.*

Finally, at each simulation step, the vision sensor will produce a sequence of 3 grayscale images with a resolution of 61x61 pixels each. But for the agent to learn effectively, it needs to perceive its own actions and have direct information about its spatial orientation in relation to the target (the cube to be chased). To do this, we will add two more sensors grouped into a single **FloatArrayCompositeSensor** type sensor. Inside this node, we create the nodes that represent the two extra sensors we need, that is, the **OrientationSensor** node (**OrientationSensor** type) and the **ActionSensor** node (**ActionSensor** type). In *Figure 14*, we show the configurations of all these nodes.

*Figure 14. Configurations of the **ArraySensor** on the right. Configuration of the **OrientationSensor** node in the upper right corner. And configuration of the **ActionSensor** in the lower left corner.*

> We can stack several observations in sequence using the **Stacked Observations** property. This can make the model more effective, but it also increases the amount of computation needed to train the model. For the agent's behavior to learn, we need to make the agent's observation more informative without greatly increasing its complexity. To do this, we have developed a set of sensors suitable for making the agent's perceptions more informative. In reinforcement learning jargon, we say that the agent's observations represent a Markovian state for the task to be learned. That's why we use two sensors: **OrientationSensor** and **ActionSensor**. As shown, we created them as a child of a **FloatArrayCompositeSensor** node, configured as shown in *Figure 14*. A **composite** type sensor groups multiple sensors, making them appear to the training algorithm as a single sensor.

Check the values of the sensors and actuators and configure them according to the images already shown.

Now we have an agent capable of moving and perceiving the world. But this agent still has no goal. We will add goals to the agent through reward functions. Add two more **Node** objects as children of the **Agent** node: one of type **MinDistReward** and the other of type **TouchRewardFunc**. The first gives a reward whenever the agent performs an action that brings it closer to the goal. The second gives a reward when the agent touches the target. In *Figure 15*, it is shown how the **MinDist** function was configured. In *Figure 16*, it is shown how the **TouchReward** function was configured.

*Figure 15. Properties of the MinDist reward function.*

*Figure 16. Properties of the TouchReward reward function.*

It is necessary to penalize the agent if it falls off the platform. To do this, as shown in *Figure 17*, we add a fall reward function (**FallRewardFunc**).

*Figure 17. Reward generated when the Agent falls below a certain level indicated by the Threshold field.*

We may want the agent to appear in the scene in various random positions. To do this, we will add an extra actuator to the agent. This actuator only runs before the start of an episode. Create a node of type **RBRespawnActuator**, as shown in *Figure 18*. Note that this node contains the **Respawn Option Path** property, which points to a sub-tree of the scene that contains several **Node3D** with positions for the agent to be reborn. Place some positions that cover the scene area.

*Figure 18. The agent's Respawn component.*

The AI4U training system uses network communication that is transparent to the user whenever possible. The user runs the Python training script, using the AI4U's Gymnasium interface, and runs the Godot game produced with AI4U or the game directly in the Editor. I prefer to use the editor directly during the experiment design phase. However, sometimes customizations are needed. Our agent sends an image with a spatial resolution of 61x61, which results in 29768, which is larger than the default network buffer size. To solve this problem, we add, as a child of the **Agent** node, a **RemoteConfig** node (associated with the **RemoteConfiguration** script), as shown in *Figure 19*.

*Figure 19. Configurations for the agent's network communication with the Python script.*

The agent's control loop does not necessarily have to coincide with the Godot physical simulation loop. We can determine that the agent will capture observations only every 4 physical steps and determine that in the frames without action, the last one selected by the agent is repeated. These configurations are already the defaults, but you can change them by adding a **ControllerConfiguration** type node as a child of the **Agent** node, as shown in *Figure 20*.

*Figure 20. Agent control loop settings.*

There are still two optional nodes, but they are present in this environment: one of type **ArrowPhysicsMoveController** (named *HumanController* in our example), which allows us to handle agents with **RigidBody3D** or with **CharacterBody3D** with the keyboard; and the other of type **NeuralNetController** (named *NeuralNetController* in our example), which allows the agent to be controlled by a neural network loaded from an ONNX file. In other tutorials, I explain these nodes in detail. The configurations of these nodes used in this experiment are shown in *Figure 21*. Although optional, the **ArrowPhysicsMoveController** node is excellent for testing the environment and the agent before performing a costly and time-consuming training.

*Figure 21. Configurations of the optional nodes in our scene.*

We are close to training our agent. We need to create the object that creates the control loop for all agents in the environment. To do this, leave the **Add Control Requestor** option checked in the **RLAgent** node. Another option is to leave this option unchecked and create a **ControlRequestor** type node, adding it as a child of the scene's root. This node can be anywhere in the scene and in it you must explicitly specify which agents will be run in the environment. In *Figure 22*, an example of a **ControlRequestor** that could be used in this demonstration is shown. Creating a specific node for **ControlRequestor** is only necessary if you have more than one agent in your scene.

*Figure 22. Agent loop configurations.*

Now we can train our agent. To do this, create a Python file (say, trainer.py) and copy the following content into it:

```python
import ai4u
from ai4u.controllers import BasicGymController
import AI4UEnv
import gymnasium as gym
import numpy as np
from stable_baselines3 import SAC
from stable_baselines3.sac import MultiInputPolicy

env = gym.make("AI4UEnv-v0")

model = SAC(MultiInputPolicy, env, verbose=1, tensorboard_log="SAC")
print("Training....")
model.learn(total_timesteps=30000, log_interval=4,  tb_log_name='SAC')
model.save("ai4u_model")
print("Trained...")
del model # remove to demonstrate saving and loading
print("Train finished!!!")
```

Ensure that all necessary modules are installed (bemaker, pytorch, stable-baselines3, and gymnasium). Now, run the Python program:

```sh
$> python trainer.py
```

Observe the agent's behavior at the beginning of the training; it moves strangely on the plane and not as if it were moving. To correct this, close the Godot game window. Now, go to the agent's **AgentBody** node and modify the **Axis Lock** property as shown in *Figure 23*.

*Figure 23. Axis Lock configuration of the agent's AgentBody.*

> Final Adjustments: In the **RigidBody3D** of all objects that can collide, enable the **Contact Monitor** property. Also, change the **Max Contacts Reported** property to a value greater than zero (I used 10000). These changes are essential for the proper functioning of the **TouchRewardFunc**. For greater physics calculation precision, enable the **Continuos CD** property of the agent's **RigidBody3D**. Specifically for the target's **RigidBody3D** (**Target** object), enable the **Freeze** and **Lock Rotation** properties. Specifically for the agent's **RigidBody3D**, change the **Damp** property, under the **Angular** tab, to 5. This property is essential for obtaining a smooth rotation of the agent's body.

After running the training script, you should observe something like what's shown in *Figure 24*.

*Figure 24. Agent training.*

After approximately 16 thousand steps of the agent's neural network update, the result is as shown in *Figure 25*.

*Figure 25. Several steps after the start of agent training.*

After the agent is trained, an *ai4u\_bemaker.zip* file (or another name you defined in the training script) is generated. This file contains the neural network model that knows how to control the agent to perform a task. We can run this model using a Python loop or by using the model directly in Godot itself. To use Python, create a test file (say run.py) with the following content:

```python
import gymnasium as gym
import numpy as np
from stable_baselines3 import SAC
from stable_baselines3.sac import MultiInputPolicy
import ai4u
from ai4u.controllers import BasicGymController
#from ai4u.onnxutils import sac_export_to
import AI4UEnv
import gymnasium as gym

env = gym.make("AI4UEnv-v0")

print('''
bemaker Client Controller
=======================
This example controll a movable character in game.
''')
model = SAC.load("ai4u_model")

#sac_export_to("ai4u_model", metadata=env.controller.metadataobj)

obs, info = env.reset()

reward_sum = 0
while True:
    action, _states = model.predict(obs, deterministic=True)
    obs, reward, done, truncated, info = env.step(action)
    reward_sum += reward
    env.render()
    if done:
      print("Testing Reward: ", reward_sum)
      reward_sum = 0
      obs, info = env.reset()
```

Now run the file with the following command:

```sh
$> python run.py
```

Run the scene and see the result.

> Change the **RLAgent** execution speed by changing the **Control Requestor Time Scale** value to 1. Or change the **Default Time Scale** property to 1 (one), if you added a **ControlRequestor** node.

In the document [Introduction to ONNX Model](https://www.google.com/search?q=introductionwithonnxmodels.md), I show how to run the model directly in Godot, without needing to use Python.

*Figure 26. Agent after being trained.*

Note that in the directory where you ran the training script, a directory named SAC was created. This directory creates TensorBoard log files for you to check the training progress in graphs. In the Linux/Windows terminal, access the SAC directory and type the command:

```
python -m tensorboard.main --logdir .
```

or

```
python -m tensorboard.main --port 6007 --logdir .
```

Then, open the link suggested by TensorBoard in a browser (usually *http://localhost:6007*). The results can be as shown in *Figure 27*.

*Figure 27. Final training result.*

Learn how to convert the model to run directly in Godot: [tutorial](https://www.google.com/search?q=introductionwithonnxmodels.md).
