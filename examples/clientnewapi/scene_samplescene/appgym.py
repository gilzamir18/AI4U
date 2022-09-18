import AI4UEnv
import gym
import numpy as np

env = gym.make("AI4UEnv-v0")
info = env.reset()
done = False 
amap = {'A': [0, -1, 0, 0], 'D': [0, 1, 0, 0], 'W': [1, 0, 0, 0], 'S': [-1, 0, 0, 0], 'U': [0, 0, 1, 0], 'F':[0, 0, 0, 1]}
while  not done:
    action = input("action: ")
    action = amap[action.upper()]
    state, reward, done, info = env.step(action)
    print(state)
    print(reward)
    print(done)