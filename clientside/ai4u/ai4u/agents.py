import numpy as np
from .utils import step, stepfv
import random
import time
from threading import Thread
from .workers import AI4UWorker

class BasicController:    
    def __init__(self):
        self.initialState = None
        self.actionName = "move"
        self.actionArgs = [0, 0, 0]
        self.defaultActionArgs = [0, 0, 0]

    def reset(self):
        print("Begin Reseting....")
        AI4UWorker.agent.createANewEpisode = True
        while self.initialState is None:
            time.sleep(1)
        info = self.initialState
        self.initialState = None
        print("Reseting Ending... ", info)
        return info

    def newEpisode(self, info):
        self.initialState = info

    def endOfEpisode(self):
        print("End of Episode...")
        self.initialState = None
    
    def configure(self, id, max_step):
        print("Agent configuration: id=", id, " maxstep=", max_step)

    def step(self, action):
        self.actionName = "move"
        self.actionArgs = [random.choice([0, 30]), 0, random.choice([0, 30])] 

    def get_action(self, info):
        action = stepfv(self.actionName,  self.actionArgs)
        self.actionArgs = self.defaultActionArgs.copy()
        return action

class BasicAgent:
    rl_env_control = {
        'max_steps': 1000,
        'agent_id': 0
    }
    
    def __init__(self):
        self.max_step = 0
        self.id = 0
        self.steps = 0
        self.createANewEpisode = False
        self.controller = BasicController()
        self.newInfo = True

    def stop(self): 
        """
        Stop agent simulation in Unity.
        """
        return step("__stop__")

    def restart(self):
        """
        Restart agent simulation in Unity.
        """
        return step("__restart__")

    def endOfEpisode(self):
        """
        Callback function for end of episode detected in Unity environment.
        """
        return self.stop() #by default, end simulation

    def step(self, info):
        """
        Callback function that returns an action. Use methdos stepfv or step
        from ai4u.utils to format an action.
        """
        if (self.controller):
            return self.controller.get_action(info)
        else:
            return stepfv( 'move', [random.choice([0, 30]), 0, random.choice([0, 30])] )

    def act(self, info):
        if self.newInfo:
            if self.controller:
                t = Thread(target=self.controller.newEpisode, args=[info])
                t.start()
            self.newInfo = False

        if self.createANewEpisode:
            self.createANewEpisode = False
            self.newInfo = True
            return self.restart()
        if info['done']:
            if self.controller:
                t = Thread(target=self.controller.endOfEpisode)
                t.start()
            self.newInfo = True
            return self.endOfEpisode()
        self.steps = self.steps + 1
        return self.step(info)

    def handleEnvCtrl(self, a):
        self.max_steps = a['max_steps']
        self.id = a['id']
        control = []
        control.append(stepfv('max_steps', [self.max_steps]))
        control.append(stepfv('id', [self.id]))
        if self.controller:
            t = Thread(target=self.controller.configure, args=[self.id, self.max_step])
            t.start()
        return ("@".join(control))
