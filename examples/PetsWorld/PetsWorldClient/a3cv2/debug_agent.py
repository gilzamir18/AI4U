from unityremote.core import RemoteEnv
from unityremote.utils import image_decode

def agent():
    env = RemoteEnv(IN_PORT=8080, OUT_PORT=7070)
    env.open(0)
    for i in range(10000000):
        sum_energy = 0
        state = env.step("restart")
        prev_energy = state['energy']
        print(prev_energy)
        done = state['done']
        while not done:
            frame = image_decode(state['frame'], 20, 20)
            print(frame)
            action = int(input('action'))
            for i in range(8):
                state = env.step("act", action)
            state = env.step('get_status')
            done = state["done"]
            energy = state['energy']
            frame = state['frame']
            touched = state['touched']
            ID = state['id']
            delta = (energy - prev_energy)
            print(delta)
            sum_energy += delta
            prev_energy = energy
        print(sum_energy)
    env.close()

agent()