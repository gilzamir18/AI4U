import torch as th
from torch import nn
from stable_baselines3 import SAC
import gymnasium as gym
import json
from ai4u.controllers import BasicGymController

def read_json_file(file_name):
    with open(file_name, 'r') as file:
        json_data = file.read()
        python_object = json.loads(json_data)
        return python_object

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


def sac_export_to(path, metadata, modelname="model.onnx", device="cpu"):

    model = SAC.load(path, device=device)
    if metadata is None:
        metadata = {"inputs":[{'name':'input'}], "outputs": []}

    dummy_inputs = None
    input_names = []
    output_names = []

    onnxable_model = OnnxableSACPolicy(model.policy.actor, model.policy.actor.features_extractor)
    for o in metadata['outputs']:
        output_names.append(o['name'])

    inputs = metadata['inputs']
    if len(metadata['inputs']) > 1:      
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
