# AI4U - Artificial Intelligence For You

Welcome to **AI4U**, an agent framework for modeling virtual reality and game environments. This repository contains the reference implementation for the Godot Game Engine.

AI4U leverages Godot's architecture to simplify agent specification through an agent abstraction. It provides an alternative approach to modeling Non-Player Characters (NPCs), but can also be used for other scenarios, such as environments for artificial intelligence experiments.

## Agent Abstraction

An agent in AI4U lives and interacts with its environment using sensors and actuators. NPCs are a specific type of agent. The main agent components are:

- **Sensors**: Provide data to the agent's brain.
- **Actuators**: Send actions from the agent to the environment.
- **Events**
- **Reward Functions**
- **Brain**: A script that processes sensor data and selects actions.

## AI4U Components

To use AI4U in a new project, you need:

- **Python scripts**
- **C# scripts**

### Install Stable Python Scripts

```bash
pip install ai4u
```

### Install Stable C# Scripts

Download the latest AI4U release [here](https://raw.githubusercontent.com/gilzamir18/AI4U/main/packages/ai4u.zip), unzip it, and place it in your Godot project directory. For ONNX model support, also copy the [`ai4u_onnx`](https://raw.githubusercontent.com/gilzamir18/AI4U/main/packages/ai4u_onnx.zip) package.

### Prerequisites for ONNX Models in Godot

In your Godot project folder (with a C# solution), install:

```bash
dotnet add package Microsoft.ML
dotnet add package Microsoft.ML.OnnxRuntime
```

If you don't have a solution, create one in Godot via Project > Tools > C# > "Create C# Solution".

### Install Latest Python Scripts

Navigate to `pyplugin/ai4upy/ai4u` and run:

```bash
pip install -e .
```

### Install Latest C# Scripts

Clone the repository and copy `addons/ai4u` to your project.

## Tutorials

- [Introduction](doc/introduction.md)
- [Don't Break the Game](doc/dontbreakthegame.md)
- [Events](doc/events.md)
- [AI4U and Stable-Baselines3](doc/stable_baselines3guide.md)
- [Using ONNX](doc/introductionwithonnxmodels.md)
- [Why Python?](doc/whypython.md)

## Requirements

- Godot 4.2.2, 4.3, or 4.4.1 (.NET version)
- Python 3.12 or newer
- Microsoft.ML.OnnxRuntime
- Gynasium
- Tested on Windows 11, Ubuntu 24.04, and PopOS 24.04 (CosmicDE)

**Minimum hardware:** GeForce 1050ti (4GB VRAM), 8GB RAM, 20GB SSD. For complex tasks (e.g., SAC with image sensors), 24GB RAM and a high-end GPU are recommended. For games, use modest sensor configurations (e.g., moderate RayCasting).

## Linux Performance Tips for Godot Projects

Recent Linux versions may throttle or pause apps with occluded windows. For reliable background simulations:

- **Start App Window Minimized:**  
    Godot Project > Project Settings > Display > Window > Mode: set to Minimized.
- **Disable V-Sync:**  
    Project Settings > Display > Window > Vsync Mode: Disabled.

These settings help AI4U simulations run consistently in the background.

## Demo Projects

Find demo projects [here](https://github.com/gilzamir18/ai4u_demo_projects) for experimentation.

## Maintainers

- Gilzamir Gomes (gilzamir_gomes@uvanet.br)
- Creto A. Vidal (cvidal@dc.ufc.br)
- Joaquim B. Cavalcante-Neto (joaquimb@dc.ufc.br)
- Yuri Nogueira (yuri@dc.ufc.br)

**Note:** No technical support, consulting, or personal questions via email.

## How to Contribute

Documentation is still in progress. If you'd like to contribute, please read the CONTRIBUTING.md guide first.

## Acknowledgments

AI4U was created at CRab (Computer Graphics, Virtual Reality and Animations) Labs, UFC (Universidade Federal do Ceará). AI4U - Artificial Inteligence For You
