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

envs = EnvironmentManager(4, wrapper=SkipframesWrapper)

envs.openAll()

def agent(env):
    for i in range(10000000):
        sum_energy = 0
        state = env.step("get_status")
        prev_energy = state['energy']
        #print(state)
        done = state['done']
        while not done:
            frame = None
            action = np.random.normal(0.0, 1.0, 12)
            state = env.stepfv("act", action)
            done = state["done"]
            energy = state['energy']
            frame = state['frame']
            touched = state['touched']
            ID = state['id']
            sum_energy += (energy - prev_energy)
            prev_energy = energy
            time.sleep(0.05)
        print(sum_energy)

    env.close()    

if __name__ == "__main__":
    t1 = threading.Thread(target=agent, args=(envs[0], ))
    t2 = threading.Thread(target=agent, args=(envs[1], ))
    t3 = threading.Thread(target=agent, args=(envs[2], ))
    t4 = threading.Thread(target=agent, args=(envs[3], ))
    t1.start()
    t2.start()
    t3.start()
    t4.start()
    t1.join()
    t2.join()
    t3.join()
    t4.join()

