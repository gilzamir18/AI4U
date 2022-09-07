import numpy as np
from .utils import step, stepfv
import random

class BasicAgent:
    rl_env_control = {
        'max_steps': 1000,
        'agent_id': 0
    }

    def __init__(self, id = 0, max_step = 1000):
        self.max_step = max_step
        self.id = id
        self.steps = 0

    def stop(self):
        return step("__stop__")

    def restart(self):
        return step("__restart__")

    def act(self, info):
        if info['done']:
            print("Episode done!")
            return self.stop()
        self.steps = self.steps + 1
        return stepfv('move', [random.choice([0, 30]), 0, random.choice([0, 30])] )

    def handleEnvCtrl(self, a):
        self.max_steps = a['max_steps']
        self.id = a['id']
        control = []
        control.append(stepfv('max_steps', [self.max_steps]))
        control.append(stepfv('id', [self.id]))
        return ("@".join(control))


