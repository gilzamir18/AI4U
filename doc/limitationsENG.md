# Known Limitations

[Summary](summary.md)

* We only support the SAC algorithm from stable-baselines3. However, users can implement extensions for other frameworks.
* The current version of AI4U only supports or has been tested with three types of policies from stable-baselines3: MlpPolicy, CnnPolicy, and MultiInputPolicy.
* The use of ONNX files directly within Godot is limited to MLP networks with one or many inputs. We have not tested the use of images directly with convolutional networks.
* Using high-resolution images as part of the agent's observation is not recommended, as it results in significant performance loss.
* Performance: The default performance is very low, achieving only 5 fps during agent training (according to stable-baselines3 metrics). To improve this performance, look for options under **Project --> Project Settings --> Physics --> Common** and change the **Physics Ticks per Second** property to a value compatible with your available hardware. On a gaming laptop with an 11th generation Intel processor, GForce 1650 graphics card with 4GB VRAM, and 25 GB of RAM, I set this property to 320 and achieved 30fps during the training phase.
