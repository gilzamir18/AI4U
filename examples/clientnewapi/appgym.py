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

env = gym.make("AI4UEnv-v0")
info = env.reset()
done = False 
amap = {'A': [0, -1, 0, 0], 'D': [0, 1, 0, 0], 'W': [1, 0, 0, 0], 'S': [0, -1, 0, 0], 'T': 'stop', 'P': 'pause', 'R': 'resume', 'U': [0, 0, 1, 0], 'F':[0, 0, 0, 1]}
while  not done:
    action = input("action: ")
    action = amap[action.upper()]
    if type(action) is not str:
        state, reward, done, info = env.step(action)
    else:
        env.step(action)
