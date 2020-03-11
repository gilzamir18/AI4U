from unityremote.core import RemoteEnv
from unityremote.utils import image_from_str

def agent():
    env = RemoteEnv(IN_PORT=8081, OUT_PORT=7071)
    env.open(0)
    for i in range(10000000):
        state = env.step("restart")
        done = state['done']
        print('new episode')
        while not done:
            frame = image_from_str(state['state'], 10, 10)
            print(frame)
            action = int(input('action'))
            state = env.step("move", action)
            done = state['done']
            print(state)
    env.close()

agent()