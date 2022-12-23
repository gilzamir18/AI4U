import torch as th
from torch import nn
from stable_baselines3 import SAC
import gym

class OnnxablePolicy(th.nn.Module):
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

# Example: model = SAC("MlpPolicy", "Pendulum-v1")
model = SAC.load("sac_ai4u.zip", device="cpu")

onnxable_model = OnnxablePolicy(model.policy.actor, model.policy.actor.features_extractor)

dummy_inputs = {k: th.randn(1, *obs.shape) for k, obs in model.observation_space.items()}

th.onnx.export(
    onnxable_model,
    (dummy_inputs, {}),
    "my_sac_actor.onnx",
    opset_version=9,
    input_names=["input"],
)
