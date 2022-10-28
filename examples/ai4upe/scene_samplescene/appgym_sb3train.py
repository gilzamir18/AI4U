import ai4u
from ai4u.controllers import BasicGymController
import AI4UEnv
import gym
import numpy as np
from stable_baselines3 import SAC
from stable_baselines3.sac import MultiInputPolicy

env = gym.make("AI4UEnv-v0")

model = SAC(MultiInputPolicy, env, verbose=1)
print("Training....")
model.learn(total_timesteps=20000, log_interval=4)
model.save("sac_ai4u")
print("Trained...")
del model # remove to demonstrate saving and loading

model = SAC.load("sac_ai4u")

print("Train finished!!!")

