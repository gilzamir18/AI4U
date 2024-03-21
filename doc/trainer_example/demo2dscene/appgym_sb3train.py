import ai4u
from ai4u.controllers import BasicGymController
import AI4UEnv
import gymnasium as gym
import numpy as np
from stable_baselines3 import DQN
from stable_baselines3.dqn import CnnPolicy

env = gym.make("AI4UEnv-v0")

model = DQN(CnnPolicy, env, verbose=1, learning_starts=5000,  target_update_interval=1000, buffer_size=100000, tensorboard_log="DQN")
print("Training....")
model.learn(total_timesteps=2000000, log_interval=4,  tb_log_name='DNQ')
model.save("demo2dmodel")
print("Trained...")
del model # remove to demonstrate saving and loading
print("Train finished!!!")
