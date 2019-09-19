from unityremote.core import RemoteEnv
import numpy as np

env = RemoteEnv()
env.open(0)

if __name__ == "__main__":
    for i in range(10000):
        c = np.random.choice([0, 1, 2, 3])
        if  c == 0:
            env.stepfv("move", [2.0, 0.0])
        elif c == 1:
            env.stepfv("move", [0.0, 2.0])
        elif c == 2:
            env.stepfv("move", [-2.0, 0.0])
        elif c == 3:
            env.stepfv("move", [0.0, -2.0])
        state = env.step("NoOp")
        print(state)
env.close()
