import ai4u
from ai4u.controllers import BasicGymController
import AI4UEnv
import gym
import numpy as np
from stable_baselines3 import SAC
from stable_baselines3.sac import MultiInputPolicy
from controller import DonutGymController

env = gym.make("AI4UEnv-v0", controller_class=DonutGymController)

model = SAC(MultiInputPolicy, env, verbose=1)
print("Training....")
model.learn(total_timesteps=500000, log_interval=10)
model.save("sac_ai4u")
print("Trained...")
del model # remove to demonstrate saving and loading

model = SAC.load("sac_ai4u")

print("Train finished!!!")

