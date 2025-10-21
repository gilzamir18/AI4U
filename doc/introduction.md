# Introduction to AI4U

## Overview

The latest version of AI4U introduces the concept of *training templates*. Currently, we offer templates based on the SAC algorithm from `stable-baselines3`. We will soon expand support to include the *Proximal Policy Optimization (PPO)* algorithm.

The purpose of a template is to simplify the training process by eliminating the need to write a Python script from scratch, provided you use a compatible framework and data structure.

For example, if you are using the SAC algorithm implemented in `stable-baselines3` and your agent's sensors in Godot (via AI4U) generate an observation vector in the format `(N,)` (where `N` is the number of inputs to the model), the template assumes a *Multi-Layer Perceptron (MLP)* architecture will be used. In this scenario, you can train your agent simply by running the following command:

```bash
python -m ai4u_ml.sb3_sac_trainer --policy=MlpPolicy # In some Linux setups, use python3.
```

Similarly, if your agent uses a `Camera3DSensor` or `Camera2DSensor`, the recommended policy is `CnnPolicy`:

```bash
python -m ai4u_ml.sb3_sac_trainer --policy=CnnPolicy # In some Linux setups, use python3.
```

Finally, for agents with multiple input types (such as images and vector data), you should use the `MultiInputPolicy`:

```bash
python -m ai4u_ml.sb3_sac_trainer --policy=MultiInputPolicy # In some Linux setups, use python3.
```

The commands above use default parameter settings and network architectures that are effective in simple scenarios. It is possible to customize parameters such as the number of layers, the number of neurons per layer, and the learning rate by passing them as command-line arguments.

However, for more complex scenarios, such as creating a custom neural network architecture, you will need to write your own Python script. This will depend on the features offered by the underlying framework (in this case, `stable-baselines3`). We provide tutorials that demonstrate how to use AI4U at a lower level:

  * [Low-Level Introduction](https://www.google.com/search?q=lowlevel_introductionPTBR.md)

## Prefabs

In addition to training templates, the current version of AI4U adds prefabricated components (prefabs) to help you get started with the tool.

In the `addons/ai4u/prefabs/3d/agents` directory, you will find three pre-configured agent templates designed to pursue a target in the scene. They all function similarly, differing only in the type of sensor they use. In the agent's script, you need to specify the target (`Goal`) and the `Respawn Options`, which define the locations where the agent should appear at the start or after an environment reset.

The available templates are:

  * **`SingleGoalCnnPolicyAgent.tscn`**: Allows the agent to observe the environment through a low-resolution camera (generating a sequence of 4 images of 61x61 pixels). Trainable with `CnnPolicy`.
  * **`SingleGoalMlpPolicyAgent.tscn`**: Allows the agent to perceive the environment through *raycasting*, which results in a matrix of object codes and distances. Trainable with `MlpPolicy`.
  * **`SingleGoalMultiInputAgent.tscn`**: Combines visual observations (images) with vector data (such as the agent's current position and its relative distance to the target). Trainable with `MultiInputPolicy`.

We also provide an example scene, `Simple3DScene.tscn`, which contains a floor and some prefabs for boxes. You can drag one of the agents into this scene and start the training to observe the learning process.

# Practical Example

Let's set up and train an agent.

1.  **Project Creation**: Create a new project in Godot.
2.  **Add-on Installation**: Download the latest version of AI4U. Unzip the file and copy the `addons/ai4u` and `addons/ai4u_prefabs3d` directories to the root of your new Godot project.
3.  **Opening the Scene**: In the Godot editor, open the example scene located at `addons/ai4u/dotnet/prefabs/3d/scenes/Simple3DScene.tscn`. The scene displays a blue cube and a pink cube on a green plane.
4.  **Adding the Agent**: Drag the `SingleGoalMlpPolicyAgent.tscn` prefab (located in `addons/ai4u/prefabs/3d/agents`) into the scene, placing it on the green plane.
5.  **Compiling the Project**: With the scene set up, you need to compile the C\# project. Go to the **Project \> Tools \> C\#** menu and select **Create C\# solution**. Then, click the hammer icon (Build) in the top-right corner of the editor.
6.  **Configuring the Agent**: Now, let's configure the agent so it knows its objective.
      * Select the agent's node in the **Scene** panel (usually named `Agent`).
      * In the **Inspector** panel (on the right), find the `Goal` property. Drag the pink cube's node (or another object) from the **Scene** panel into this field.
      * Find the `Respawn Options` property. Drag the `respawn` node from the **Scene** panel into this field. This defines where the agent will reappear.

After these configurations, you can start the training from your terminal:

```bash
python -m ai4u_ml.sb3_sac_trainer --policy=MlpPolicy # In some Linux setups, use python3.
```

You can pass other command-line arguments compatible with `stable-baselines3`. To see all available options, run:

```bash
python -m ai4u_ml.sb3_sac_trainer --help
```

For example, a useful argument is `--timesteps`, which limits the total training duration. For this case, 60,000 steps are sufficient:

```bash
python -m ai4u_ml.sb3_sac_trainer --policy=MlpPolicy --timesteps=60000
```

## Monitoring Training with TensorBoard

To monitor the training progress (either in real-time or after completion), run the following command in the same directory:

```bash
tensorboard --logdir .
```

Follow the instructions displayed in the terminal to access the TensorBoard web interface. You will be able to view graphs showing the agent's performance, such as the mean reward, similar to the image below:

![](img/tensorboardex.png)