import gymnasium as gym
import numpy as np
from stable_baselines3 import SAC
from stable_baselines3.sac import MultiInputPolicy
import ai4u
from ai4u.controllers import BasicGymController
from ai4u.onnxutils import sac_export_to, read_json_file
import AI4UEnv
import gymnasium as gym


env = gym.make("AI4UEnv-v0")

print('''
bemaker Client Controller
=======================
This example controll a movable character in game.
''')
model = SAC.load("ai4u_model")

metadatamodel = read_json_file('model.json')
sac_export_to("ai4u_model", metadatamodel)

obs, info = env.reset()

reward_sum = 0
while True:
    action, _states = model.predict(obs, deterministic=True)
    obs, reward, done, truncated, info = env.step(action)
    reward_sum += reward
    env.render()
    if done:
      print("Testing Reward: ", reward_sum)
      reward_sum = 0
      obs, info = env.reset()
