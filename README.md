# What is UnityRemotePlugin?
UnityRemotePlugin is a way to connect your Unity application to Python code. Therefore, it's possible with UnityRemote connecting your games and virtual reality applications to vast libraries set written in Python. For example, there are many deep reinforcement learning algorithms written in Python. Here we provide some examples with the A3C (Asynchronous Advantage Actor-Critic) algorithm. Moreover, you can write your code in Python to control game aspects better controlled in Python.

# How to use the UnityRemotePlugin?
UnityRemotePlugin has two components. The first component is the *server-side* code written in C#. The second component is the *client-side* code written in Python.

We recommend that you have some version of Unity 2019 and start by looking at the examples available in the directories *examples*, and *unityplugin/UnityRemoteExample*. Start by example *examples/CubeAgent* for the first look in UnityRemotePlugin. Also, see the documentation available in directory *doc*. See the complete documentation in Table 1.

Table 1: Documentation.

| Tutorial        |                                    Link                                          |
|-----------------|---------------------------------------------------------------------------------------|
| API Overview    |  [doc/overeview.md](doc/overview.md)                                           |
| UnityRemote with OpenAI Gym      | [doc/unityremotegym.md](doc/unityremotegym.md)                         |
| A3C Implementation     |  [doc/a3cintro.md](doc/a3cintro.md)                        |
| GMPROC (multiprocessing) | [GMPROC](clientside/unityremote/unityremote/gmproc/README.md)

## Installation

First, you need to install *clientside* component. Open the Ubuntu command-line console and enter in directory *clientside/unityremote*. Run the following command:

```
pip3 install -e .
```

Waiting for the installation ends, this may take a while. If you want to install gym and a3c algorithm support, go to directory *clientside/gym* and run the following command:

```
pip3 install -e .
```

Waiting for the installation ends.

Now, run Unity and open project available in *unityplugin/UnityRemoteExample*.  Then open the BallRoller scene (available on *Assets/scenes*) if it is not already loaded.

![Menu File --> Open Scene ](doc/images/openscene.PNG)


![Menu File --> Open Scene ](doc/images/scenesmarked.PNG)

Then, press the play button and run the random_ballroller.py script located in *clientside/examples/ballroller*.

![Pressing play button](doc/images/ballrollerplay.PNG)

```
python3 random_ballroller.py
```

![Running example ballroller](doc/images/ballrollerexec.PNG)

Output logs:
![See output logs ](doc/images/ballrollerlog.PNG)

If no error message was displayed, then UnityRemotePlugin was installed correctly.

# OS Support and Software Compatibility
All examples available in UnityRemotePlugin Ubuntu 19.10. We used Python 3.7.5 available in this Ubuntu version. Unity used was the version 2019.3.0f6 (64 bits for GNU/Linux).

Unfortunately, UnityRemotePlugin does not have support for mobile applications.

# <span style="color:red"> Warning </span>
UnityRemote is in the alpha development stage. Therefore, your API is not yet consistent as we would wish. But we are evolving!
