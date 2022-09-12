from ai4u.agents import BasicAgent
from ai4u.utils import stepfv
from threading import Thread
from ai4u.ai4u2unity import create_server
import ai4u
from controller import SimpleController
from ai4u.controllers import BasicGymController
import AI4UEnv
import gym
import numpy as np
from stable_baselines3 import SAC
from stable_baselines3.sac import MultiInputPolicy

env = gym.make("AI4UEnv-v0")

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


