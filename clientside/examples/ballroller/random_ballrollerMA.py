from unityremote.core import RemoteEnv, EnvironmentManager
import numpy as np
import threading
import time


class SkipframesWrapper(RemoteEnv):
    def step(self, action, value=None):
        state = None
        for i in range(4):
            state = super().step(action, value)
        return state

envs = EnvironmentManager(2, wrapper=SkipframesWrapper)

envs.openAll()

def agent(env):
    for i in range(50):
        sum_reward = 0
        state = env.step("restart")
        print(state)
        done = state['done']
        while not done:
            frame = None
            if np.random.choice([0, 1]) == 0:
                state = env.step("fx", 0.1)
            else:
                state = env.step("fz", 0.1)
            done = state["done"]
            reward = state['reward']
            sum_reward += reward
        print(sum_reward)
        time.sleep(0.01)
    env.close()    

if __name__ == "__main__":
    t1 = threading.Thread(target=agent, args=(envs[0], ))
    t2 = threading.Thread(target=agent, args=(envs[1], ))
    t1.start()
    t2.start()
    t1.join()
    t2.join()