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
        self.lastinfo = None
        self.waitfornextstate = 0.001
        self.waitforinitialstate = 0.01
        self.done = False
        self.agent = None
        self.id = 0
        self.max_steps = 0

    def reset_behavior(self):
        self.actionArgs = [0, 0, 0]

    def reset(self):
        print("Begin Reseting....")
        self.reset_behavior()
        self.done = False
        self.agent.createANewEpisode = True
        while self.initialState is None:
            time.sleep(self.waitforinitialstate)
        info = self.initialState
        self.initialState = None
        print("Reseting Ending... ", info)
        return info

    def newEpisode(self, info):
        self.initialState = info

    def endOfEpisode(self, info):
        self.nextstate = info
        self.done = True
        self.initialState = None
    
    def configure(self, id, max_step):
        self.id = id
        self.max_steps = max_step

    def step_behavior(self, action):
        """
        Override this method to change step behavior. Never change
        step method directly.
        """
        self.actionName = "move"
        self.actionArgs = [random.choice([0, 500]), 0, random.choice([0, 500])]
        

    def step(self, action):
        """
        This method change environment state under action named 'action'.
        Never override this method. If you want change step behavior,
        implements step_behavior method.
        """
        self.nextstate = None
        self.step_behavior(action)
        while not self.done and self.nextstate is None:
            time.sleep(self.waitfornextstate)
        #rewards = AI4UWorker.agent.steps_reward
        #AI4UWorker.agent.steps_reward = 0
        state = self.nextstate
        if not state:
            state = self.agent.lastinfo
        return state, state['reward'], state['done'], {} 

    def getaction(self, info):
        """
        Return a new action based in new info 'info'.
        """
        self.lastinfo = info.copy()
        self.nextstate = info.copy()
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
        self.lastinfo = None

    def stop(self): 
        """
        Stop agent simulation in Unity.
        """
        return step("__stop__")

    def restart(self):
        """
        Restart agent simulation in Unity.
        """
        self.lastinfo = None
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
        assert info['id'] == self.id, "Error: inconsistent agent identification!"
        if (self.controller):
            return self.controller.getaction(info)
        else:
            return stepfv( 'move', [random.choice([0, 30]), 0, random.choice([0, 30])] )

    def act(self, info):
        if self.newInfo:
            self.lastinfo = None
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
                t = Thread(target=self.controller.endOfEpisode, args=[info])
                t.start()
            self.newInfo = True
            self.lastinfo = info
            return self.endOfEpisode()
        self.steps = self.steps + 1
        return self.step(info)

    def handleEnvCtrl(self, a):
        if 'config' in a:
            self.max_steps = a['max_steps']
            self.id = a['id']
            control = []
            control.append(stepfv('max_steps', [self.max_steps]))
            control.append(stepfv('id', [self.id]))
            if self.controller:
                t = Thread(target=self.controller.configure, args=[self.id, self.max_step])
                t.start()
            return ("@".join(control))
        if 'wait_command' in a:
            if self.createANewEpisode:
                self.createANewEpisode = False
                self.newInfo = True
                return self.restart()
        return stepfv('noop', [0])
    