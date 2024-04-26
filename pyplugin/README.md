# PyPlugin for AI4U 
The Python side of the AI4U can be installed using local pip package. To install it, enter the directory pyplugin/ai4upy and run the command:

    pip install -e . 

This command allows you use python to train the AI4U agent living in a godot scene.

# PyPlugin API

PyPlugin provides a basic API that allow you write a training script based in stable-baselines3 and gymnasium to access an environment programmed in Godot (mono edition). Firstly, in your script, import the required modules for that:

```python
import ai4u
import AI4UEnv
import gymnasium as gym
```

So, make a environment using the identifier 'AI4UEnv-v0':


```python
env = gym.make("AI4UEnv-v0")
```

Now, you can run operations in your environment using gymnasium interface. Thus, it's important to reset the environment:

```python
initial_state = env.reset()
```

Also, we can send an action to the environment using the step function:

```python
next_state, reward, done, truncated, info = env.step(action)
```

The shapes of the states and the actions are defined in environment designed in Godot. 

# Godot and PyPlugin Communication
Godot scene and pyplugin communicate by means of the ODP protocol. For that, we use our own communication protocol between Godot and pyplugin. This protocol is based in data shape, and data type conventions. Even we have work to create default parameters working in bascis uses cases, it can be that new demands require change a lot of parameters. Ones of these parameters are host address and the communicattion port. Default value of host is "127.0.0.1", and the default value of port is "8080". We can change this throught config parameters in environment creating:


```python
env = gym.make("AI4UEnv-v0", rid='0', config=dict(server_IP='127.0.0 1', server_port=8080))
```

The  remote id (aka rid) is the identifier of the agent hosted in Godot scene. Also, we can specify a custom controller throught the parameters controller:

```python
env = gym.make("AI4UEnv-v0", controller_class=DonutGymController, rid='0', config=dict(server_IP='127.0.0.1', server_port=8080))
```

In the config dictionary, we can also modify:

- buffer_size: the buffer size used in ODP communication.
- waittime: the time to start communication with the Godot scene.
- timeout: the time to wait for Godot scene messages before closing the connection. If set to zero, the timeout is disabled.
- action_buffer_size: the size of the action buffer. If set to zero, all actions are received and processed.

------------------------------
AI4U - PyPlugin - 2024 - AI4ULab.