# Introduction to AI4U with Stable-Baselines3
AI4U and Stable-Baselines3 (SB3) make a perfect team for working with reinforcement learning in Godot. Using human controllers (ArrayPhysicsMoveController and ArrayPhysicsMoveController2D) for testing, you can model the environment and the agent on the Godot side. Then, you can use the ai4u pyplugin for training the agent. However, some tricks are neccessary.

## Choosing a Policy adequated for your Agent
Stable-Baselines provides a lot of training algorithms. With these algorithms, SB3 offers several policy models. A policy model is a neural network with a specific architecture. For example, MlpPolicy is a policy that implements a Multi-Layer Perceptron neural network. Which architecture should I use? The answer depends on which sensors your agent has. Additionally, it depends on how the agent's sensors are organized.

## The Sensors

Sensors are components that gather information about the agent and the environment. All sensors have an identification specified in their *Perception Key* field. The sensors get environment/agent information and send it to the controller of the agent. The agent has one or more sensors. There are two ways to add a sensor to the agent. The first is by adding values to the ArrayInput property of the agent. The second way is by adding sensor nodes as the agent's children. Both ways can be used together.

### The **Array Input** Sensor

An RLAgent node has a group of properties in the Godot editor named *Array Input*. This is an optional group for the default sensor named **AgentArrayInput**. This sensor facilitates simple agent modeling where the programmer manually adds values to the agent's input. The agent's input is the set of sensors used to send values to the agent's controller. Therefore, the **Array Input** group in the RLAgent inspector allows the sensor *Array Input*. If "Initial Input Size" is greater than 0, you can add values to the agent's input through the command:

```CSharp
agent.ArraySensor.SetValue(idx, value);
```

where `agent` is a reference to the RLAgent node, `idx` is a valid index greater than or equal to 0 and less than the value in the field named *Initial Input Size*. 

#### Sensor Nodes as the Agent's Children

There are various ready-made sensors that you can use for your project by simply adding them as children of the agent. For example, you can use a ray-casting sensor by adding a node of the type **LinearRaycastingSensor** as a child of the agent. Sensors under node *FloatArrayCompositeSensor* will seen as unique sensor with identification defined in *FloatArrayCompositeSensor* node. The sensors returns float arrays. Já sensors of the type *RayCasting* (with the proprety flatten disbaled), *ScreenSensor*, and "Camera3DSensor" returns a bidimentional matrices. 

### Sensor Ranges

All sensors have a range of values. For example, *ScreenSensor* generates images, so its values range from 0 to 255; a normalized *OrientationSensor* ranges from -1 to +1. Some sensors let you define a data range. For example, the LinearRaycastingSensor allows for the specification of the sensor's range.

## Sensors and Stable-Baselines3

Stable-baselines3 (SB3) provides various types of policies. It's necessary to choose the appropriate policy for your sensors. For example, if your agent has only one sensor that returns an array of float numbers, the best policy to use is MlpPolicy. Otherwise, if your agent has two inputs, you should use a MultiInputPolicy. However, if your agent has only one ScreenSensor or a Camera2D/Camera3D sensor, you should use a CnnPolicy. Thus, the policy type depends on how the agent's input is configured.



