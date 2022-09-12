import numpy as np
from .utils import step, stepfv
import random
import time
from threading import Thread
from .workers import AI4UWorker
import sys

class BasicController:    
    def __init__(self):
        self.initialState = None
        self.actionName = "move"
        self.actionArgs = [0, 0, 0, 0]
        self.defaultActionArgs = [0, 0, 0, 0]
        self.lastinfo = None
        self.waitfornextstate = 0.001
        self.waitforinitialstate = 0.01
        self.done = False
        self.agent = None
        self.id = 0
        self.max_steps = 0
        self.newaction = False

    def reset_behavior(self, info):
        self.actionArgs = [0, 0, 0, 0]
        return info

    def request_reset(self):
        #print("Begin Reseting....")
        self.agent.createANewEpisode = True
        self.newaction = True
        while self.initialState is None:
            time.sleep(self.waitforinitialstate)
            self.agent.createANewEpisode = True
        self.done = False
        info = self.initialState
        self.initialState = None
        #print("Reseting Ending... ", info)
        self.paused = False
        return self.reset_behavior(info)

    def handleNewEpisode(self, info):
        self.initialState = info

    def handleEndOfEpisode():
        pass

    def _endOfEpisode(self, info):
        self.handleEndOfEpisode(info)
        self.nextstate = info
        self.done = True
        self.initialState = None
    
    def stop(self):
        self.agent.request_stop()

    def pause(self):
        self.agent.request_pause()
    
    def resume(self):
        self.agent.request_resume()

    def handleConfiguration(self, id, max_step):
        self.id = id
        self.max_steps = max_step

    def step_behavior(self, action):
        """
        Override this method to change step behavior. Never change
        step method directly.
        """
        self.actionName = "move"
        self.actionArgs = [random.choice([0, 500]), 0, random.choice([0, 500])]
        
    def request_step(self, action):
        """
        This method change environment state under action named 'action'.
        Never override this method. If you want change step behavior,
        implements step_behavior method.
        """
        self.nextstate = None
        self.step_behavior(action)
        self.newaction = True
        while (not self.done) and (self.nextstate is None) and (not self.agent.paused):
            time.sleep(self.waitfornextstate)
        if self.agent.paused:
            if self.actionName == '__resume__':
                self.resume()
                return {"paused": False}, 0, False, {'paused': False}
            return {'paused': True}, 0, False, {'paused': True}
        state = self.nextstate
        if not state:
            state = self.agent.lastinfo
        return state, state['reward'], state['done'], {} 

    def getaction(self, info):
        """
        Return a new action based in new info 'info'.
        """
        self.lastinfo = info.copy()
        if (self.actionName == '__pause__') and not self.agent.paused:
            return self.agent._pause() #environment is in action mode
        elif (self.actionName == '__resume__') and self.agent.paused:
            return self.agent._resume() #environment is in envcontrolmode 
        elif (self.actionName == '__stop__'):
            return self.agent._stop()
        self.nextstate = info.copy()
        action = stepfv(self.actionName,  self.actionArgs)
        if self.newaction:
            self.actionArgs = self.defaultActionArgs.copy()
            self.newaction = False
            return action
        else:
            return step("__waitnewaction__")

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
        self.stopped = False
        self.paused = False


    def request_stop(self):
        self.stopped = True
    
    def request_pause(self):
        if not self.stopped:
            self.paused = True

    def request_newepisode(self):
        self.createANewEpisode = True
    
    def request_restart(self):
        if self.stopped:
            self.stopped = False
    
    def request_resume(self):
        if self.paused and not self.stopped:
            self.paused = False

    def _stop(self): 
        """
        Stop agent simulation in Unity.
        """
        self.stopped = True
        self.paused = False
        return step("__stop__")

    def _restart(self):
        """
        Restart agent simulation in Unity.
        """
        self.lastinfo = None
        self.stopped = False
        self.paused = False
        return step("__restart__")
    
    def _pause(self):
        """
        Pause agent simulation in Unity.
        """
        self.paused = True
        return step("__pause__")
    
    def _resume(self):
        """
        Resume agent simulation in Unity.
        """
        self.paused = False
        return step("__resume__")

    def _step(self, info):
        assert info['id'] == self.id, "Error: inconsistent agent identification!"       
        return self.__get_controller().getaction(info)

    def act(self, info):
        if self.newInfo:
            self.lastinfo = None
            self.__get_controller().handleNewEpisode(info)
            self.newInfo = False

        if self.createANewEpisode:
            self.createANewEpisode = False
            self.newInfo = True
            return self._restart()
        
        if self.paused:
            return self._pause()

        if  self.stopped:
            return self._stop()

        if info['done']:
            self.__get_controller()._endOfEpisode(info)
            self.newInfo = True
            self.lastinfo = info
            return self._stop()
        self.steps = self.steps + 1
        return self._step(info)

    def handleEnvCtrl(self, a):
        if 'config' in a:
            self.max_steps = a['max_steps']
            self.id = a['id']
            control = []
            control.append(stepfv('max_steps', [self.max_steps]))
            control.append(stepfv('id', [self.id]))
            self.__get_controller().handleConfiguration(self.id, self.max_step)
            return ("@".join(control))
        if 'wait_command' in a:
            if self.createANewEpisode:
                self.createANewEpisode = False
                self.newInfo = True
                return self._restart()
            if self.paused == False:
                return self._resume()
            elif self.stopped == false:
                self.createANewEpisode = False
                self.newInfo = True
                return self._restart()
        return stepfv('__noop__', [0])

    def __get_controller(self):
        if (self.controller):
            return self.controller
        else:
            print("ERROR(agents.py): agent without controller!")
            sys.exit(0)
