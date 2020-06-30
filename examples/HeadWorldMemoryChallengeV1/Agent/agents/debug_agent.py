from unityremote.core import RemoteEnv
import numpy as np
from unityremote.utils import image_decode

def agent():
    env = RemoteEnv(IN_PORT=8080, OUT_PORT=7070, host="127.0.0.1")
    env.open(0)
    for i in range(10000000):
        #state = env.step("restart")
        state = env.step("restart")
        prev_energy = state['energy']
        done = state['done']
        print("OK")
        while not done:
            action = int(input('action'))
            #action = np.random.choice([0, 1])
            reward_sum = 0
            touched = np.zeros(8)
            for i in range(8):
                state = env.step("act", action)
                energy = state['energy']
                touched[i] = state['touched']
                reward_sum += state['reward']
                if state['done']:
                    break
            done = state['done']
            prev_energy = energy
            frame = state['frame']
            frame = image_decode(state['frame'], 20, 20)
            print(frame)
            print('Reward: ', reward_sum)
            print('Touched: ', touched)
            print('Signal: ', state['signal'])
            print('Done: ', state['done'])
            prev_energy = energy
        if i >= 2:
            break
    env.close()

agent()