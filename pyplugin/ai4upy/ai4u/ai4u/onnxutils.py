import torch as th
from torch import nn
from stable_baselines3 import SAC
import gym

class OnnxableSACPolicy(th.nn.Module):
    def __init__(self, actor: th.nn.Module, extractors: nn.ModuleDict):
        super().__init__()
        # Removing the flatten layer because it can't be onnxed
        
        self.extractors = extractors

        self.actor = th.nn.Sequential(
            actor.latent_pi,
            actor.mu,
            # For gSDE
            # th.nn.Hardtanh(min_val=-actor.clip_mean, max_val=actor.clip_mean),
            # Squash the output
            th.nn.Tanh(),
        )

    def forward(self, observations):
        input_tensor = self.extractors(observations)
        # NOTE: You may have to process (normalize) observation in the correct
        #       way before using this. See `common.preprocessing.preprocess_obs`
        return self.actor(input_tensor)


def sac_export_to(path="model", modelname="model.onnx", metadatamodel= None, device="cpu"):
    # Example: model = SAC("MlpPolicy", "Pendulum-v1")
    if metadatamodel is None:
        metadatamodel = {"inputs":[{'name':'input'}], "outputs": []}

    model = SAC.load(path, device=device)


    dummy_inputs = None
    input_names = []
    output_names = []

    onnxable_model = OnnxableSACPolicy(model.policy.actor, model.policy.actor.features_extractor)
    for o in metadatamodel['outputs']:
        output_names.append(o['name'])

    inputs = metadatamodel['inputs']
    if len(metadatamodel['inputs']) > 1:      
        dummy_inputs = {k: th.randn(1, *obs.shape) for k, obs in model.observation_space.items()}
        for i  in inputs:
            input_names.append(i['name'])
        th.onnx.export(
            onnxable_model,
            (dummy_inputs, {}),
            modelname,
            opset_version=9,
            input_names=input_names,
            output_names=output_names
        )
    else:
        observation_size = model.observation_space.shape
        dummy_input = th.randn(1, *observation_size)
        input_names.append(inputs[0]['name'])
        th.onnx.export(
            onnxable_model,
            dummy_input,
            modelname,
            opset_version=9,
            input_names=input_names,
            output_names=output_names
        )
