import ai4u
from ai4u.controllers import BasicGymController
import AI4UEnv
import gym
import numpy as np
from stable_baselines3 import SAC
from stable_baselines3.sac import MultiInputPolicy

env = gym.make("AI4UEnv-v0")

print('''
AI4U Client Controller
=======================
This example controll a movable character in game (unity or godot).
''')

opt = input("Godot (G), Unity (U) or Current (Any)? ")

if opt.upper() == 'G':
  model = SAC.load("sac_ai4u_godot")
elif opt.upper() == 'U':
  model = SAC.load("sac_ai4u_unity")
else:
  model = SAC.load("sac_ai4u")

obs = env.reset()
reward_sum = 0
while True:
    action, _states = model.predict(obs, deterministic=True)
    obs, reward, done, info = env.step(action)
    reward_sum += reward
    env.render()
    if done:
      print("Testing Reward: ", reward_sum)
      reward_sum = 0
      obs = env.reset()


