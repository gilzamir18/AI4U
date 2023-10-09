import BMEnv
import gym
import numpy as np

env = gym.make("BMEnv-v0")
state = env.reset()
done = False 
amap = {'A': [0, -0.2, 0, 0], 'D': [0, 0.2, 0, 0], 'W': [1, 0, 0, 0], 'S': [-1, 0, 0, 0], 'U': [0, 0, 1, 0], 'F':[0, 0, 0, 1]}

print('''
bemakerConnect Demo.
Use:
W: forward
A: turn left
S: backward
D: turn right
U: Jump
F: JumpForward
L: Print State
CTRL+C: exit.
''')
reward = 0
while  not done:
    action = input("action: ")
    if action.upper() == "L":
        print(state)
        print("Reward: ", reward)
        print("Done: ", done)
    else:
        action = amap[action.upper()]
        state, reward, done, info = env.step(action)
