import ai4u
import AI4UEnv
import gym
import numpy as np
from stable_baselines3 import SAC
from stable_baselines3.sac import MultiInputPolicy
from controller import DonutGymController

env = gym.make("AI4UEnv-v0", controller_class=DonutGymController)
model = SAC.load("sac_ai4u")

print("Testing...")
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


