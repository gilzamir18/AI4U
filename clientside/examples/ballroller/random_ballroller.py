from unityremote.core import RemoteEnv
import numpy as np

env = RemoteEnv()
env.open(0)

if __name__ == "__main__":
    for i in range(10):
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
env.close()