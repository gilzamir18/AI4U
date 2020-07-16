![Running example ballroller](doc/images/ai4u1.PNG)

# What is AI4U?
AI4U is a neat and easy way to connect a Unity application to Python code. Therefore, it is possible with AI4U plugin connecting games and virtual reality applications to vast libraries set written in Python. Moreover, One provides integration with the latest algorithms implemented by OpenAI, e.g., PPO and PPO2. So, people interested in AI4U can use more recent advances given by OpenAI researchers.

## Features

- Some examples.
- Support for multiple environment configurations and training scenarios
- Flexible SDK that can be integrated into your game or custom Unity scene
- API agnostic, but one provides support to all algorithms implemented by [stable-basiles](https://github.com/hill-a/stable-baselines).
- Integrated A3C implementation.
- Tools for facilated no-markovian decision making.
- AI4U can be integrated to Imitation Learning through Behavioral Cloning or
  Generative Adversarial Imitation Learning present on [stable-baslines](https://github.com/hill-a/stable-baselines).
- Train robust agents using environment randomization.
- Flexible agent control with On Demand Decision Making.
- Train using multiple concurrent Unity environment instances.
- Unity environment partial control from Python.
- Wrap Unity learning environments as a [gym][doc/ai4ugym.md](doc/ai4ugym.md).


# How to use the AI4U?
AI4U has two components:
    * the *server-side* code written in C#. The second is the
    * *client-side* code written in Python.

For using AI4U, we recommend to have some version of Unity 2019 and start by looking at the examples available in the repository [AI4UExamples](https://github.com/gilcoder/AI4U). Start by example [AI4UExamples/CubeAgent](https://github.com/gilcoder/AI4UExamples/CubeAgent) for the first look in AI4U. Also, see the documentation available in directory *doc*. See the complete documentation in Table 1.

Table 1: Documentation.

| Tutorial        |                                    Link                                          |
|-----------------|---------------------------------------------------------------------------------------|
| API Overview    |  [doc/README.md](doc/README.md)                                           |
| AI4U with OpenAI Gym      | [doc/ai4ugym.md](doc/ai4ugym.md)                         |
| A3C Implementation     |  [doc/a3cintro.md](doc/a3cintro.md)                        |
| GMPROC (multiprocessing) | [GMPROC](clientside/ai4u/ai4u/gmproc/README.md)

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

Now, run Unity and open project available in *exemples/BallRoller*.  Then open the BallRoller scene (available on *Assets/scenes*) if it is not already loaded.

![Menu File --> Open Scene ](doc/images/openscene.PNG)


![Menu File --> Open Scene ](doc/images/scenesmarked.PNG)

Then, press the play button and run the random_ballroller.py script located in *examples/BallRoller/Client*.

![Pressing play button](doc/images/ballrollerplay.PNG)

```
python3 random_ballroller.py
```

![Running example ballroller](doc/images/ballrollerexec.PNG)

Output logs:
![See output logs ](doc/images/ballrollerlog.PNG)

If no error message was displayed, then AI4U was installed correctly.

# Requirements

Tested Systems
----------

- Operation System: Ubuntu 18.04, and 19.10.
     * Python >= 3.7.5.
     * Unity >= 2019.4 (64 bits for GNU/Linux).

- Operation System: Ubuntu >= 20.04
    * Anaconda Python "3.7" version
    * Unity >= 2019.4 (64 bits for GNU/Linux).

- Operation System: Windows 10
    * Anaconda Python "3.7" version.
    * Unity >= 2019.4 (64 bits for Windows)

Unfortunately, AI4U does not have support for mobile applications.

