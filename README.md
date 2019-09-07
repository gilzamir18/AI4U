# What is UnityRemotePlugin?
UnityRemotePlugin is a way to connect your Unity developed application to Python code. Therefore, it's possible with UnityRemote connecting your games and virtual reality applications to vast libraries set written in Python. For example, there are many deep reinforcement learning algorithms written in Python. Moreover, you can write your code and logic in Python to control game aspects better controlled in Python.

# How to use the UnityRemotePlugin?
UnityRemotePlugin has two components. The first component is the *server side* code written in C#. The second component is the *client side* code written in Python (*version 3.1.6*).

We recommend that you have some version of Unity 2019 and start by looking at the examples available in the directories *unityplugin/UnityRemoteExample* and *clientside/examples*. Start by example *beginner* for the first look in UnityRemotePlugin. Also see the documentions available in directory doc.

## Installation

Firstly, it is need to install *clientside* component. Open the Ubuntu command-line console and enter in directory *clientside/unityremote*. Run the following command:

```
pip3 install -e .
```

Waiting the installation ends, this may take a while. If you want to install gym and a3c algorithm support, go to directory *clientside/gym* and run the following command:

```
pip3 install -e .
```

Waiting the installation ends.

Now, run Unity and open project available in *unityplugin/UnityRemoteExample*.  Then open the BallRoller scene (available on *Assets/scenes*) if it is not already loaded.

![Menu File --> Open Scene ](doc/images/openscene.PNG)


![Menu File --> Open Scene ](doc/images/scenesmarked.PNG)

Then, press the play button and then run the random_ballroller.py script located in *clientside/examples/ballroller*.

![Pressing play button](doc/images/ballrollerplay.PNG)

```
python3 random_ballroller.py
```

![Running example ballroller](doc/images/ballrollerexec.PNG)

Output logs:
![See output logs ](doc/images/ballrollerlog.PNG)

If no error message was displayed, then UnityRemotePlugin was installed correctly.

# OS Support and Software Compatibility
All examples available in UnityRemotePlugin repo were tested only in Windows 10 Pro (version 1903) with WSL activated. Ubuntu 18.0.4 image was used in WSL. We used Python 3.6.8 available in this Ubuntu version. Unity used was the version 2019.2.1f1 (64 bits for windows).

Unfortunately, for now, UnityRemotePlugin does not have support for mobile applications.

# <span style="color:red"> Warning </span>
For now, UnityRemote is in alpha development stage. Therefore, your API is not yet consistent as we would wish. But we are evolving!
