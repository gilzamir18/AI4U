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
            action = int (input("action-----------------------------------------"))
            if action == 0:
                state = env.step("fx", 0.1)
            elif action == 1:
                state = env.step("fz", 0.1)
            else:
                state = env.step("noop", 0.0)

            done = state["done"]
            reward = state['reward']
            sum_reward += reward
        print(sum_reward)
env.close()