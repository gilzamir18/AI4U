from unityremote.core import RemoteEnv, EnvironmentManager
import numpy as np
import threading
import time
from gmproc.gmproc import Workers


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
        print(sum_energy)
        time.sleep(0.01)
    env.close()    

if __name__ == "__main__":
	ws = Workers()
	i = 0
	ws.add(0, agent, params=envs[0])
	ws.add(1, agent, params=envs[1])
	ws.add(2, agent, params=envs[2])
	ws.add(3, agent, params=envs[3])
	for _ in range(10):
		ws.run([0, 1, 2, 3])

