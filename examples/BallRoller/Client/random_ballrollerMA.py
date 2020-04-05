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

envs = EnvironmentManager(1, wrapper=SkipframesWrapper)

envs.openAll()

def agent(env):
    for i in range(50):
        sum_reward = 0
        state = env.step("restart")
        print(state)
        done = state['done']
        while not done:
            frame = None
            action = np.random.choice([0, 1, 2, 3])
            #action = int(input("-----------------------------------> "))
            if  action == 0:
                state = env.step("fx", 1)
            elif action == 1:
                state = env.step("fx", -1)
            elif action == 2:
                state = env.step("fz", 1)
            elif action == 3:
                state = env.step("fz", -1)

            done = state["done"]
            reward = state['reward']
            sum_reward += reward
        print(sum_reward)
        time.sleep(0.01)
    env.close()    

if __name__ == "__main__":
    t1 = threading.Thread(target=agent, args=(envs[0], ))
    #t2 = threading.Thread(target=agent, args=(envs[1], ))
    t1.start()
    #t2.start()
    t1.join()
    #t2.join()