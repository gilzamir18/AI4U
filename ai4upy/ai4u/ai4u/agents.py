from .utils import step, stepfv, steps
import random
import time
import sys
from threading import Thread
from queue import Empty, Queue

class BasicController:    
    def __init__(self):
        self.initialState = None
        self.actionName = "__waitnewaction__"
        self.actionArgs = [0, 0, 0, 0]
        self.defaultAction = "__waitnewaction__"
        self.defaultActionArgs = [0, 0, 0, 0]
        self.agent = None
        self.id = 0
        self.newaction = False
        self.fields = None
        self.max_steps = 0
        self.metadatamodel = None
        self.lastinfo = None

    def close(self):
        pass #release resources here

    def handleNewEpisode(self, info):
        pass

    def handleEndOfEpisode(self, info):
        pass
    
    def transform_state(self, info):
        return info

    def handleConfiguration(self, id, max_step, metadatamodel):
        self.id = id
        self.max_steps = max_step
        self.metadatamodel = metadatamodel

    def reset_behavior(self, info):
        self.actionArgs = [0, 0, 0, 0]
        return info

    def step_behavior(self, action):
        """
        Override this method to change step behavior. Never change
        step method directly.
        """
        self.actionName = "move"
        self.actionArgs = [random.choice([0, 500]), 0, random.choice([0, 500])]
        self.fields = []

    def request_reset(self, args=None):
        self.agent.qin.put(["reset"])
        info = None
        tryreset = True
        while tryreset:
            try:
                info = self.agent.qout.get()
                if info[0] == "reset":
                    tryreset = False
                elif info[0] == "halt":
                    break
            except TimeoutError as e:
                print(f"Trying reset again after {self.agent.timeout} seconds!")
                tryreset = True
            except Empty as e:
               print("Empty message in reset. Trying reset again...")
               tryreset = True
            except KeyboardInterrupt as e:
                print("PRESS EXIT")
                sys.exit(0)
    
        if info[0] == "halt":
            sys.exit(0)
        self.restoreDefaultAction()
        return self.reset_behavior(info[1])

    def request_config(self, args=None):
        self.agent.qin.put(["config"])
        msg = None
        trying = True
        while trying:
            try:
                msg = self.agent.qout.get()
                if msg[0] == "config":
                    trying = False
                elif msg[0] == "halt" or msg[0] == "reset":
                    break
            except TimeoutError as e:
                print(f"Trying again after {self.agent.timeout} seconds!")
                trying = True
            except Empty as e:
               print("Empty message. Trying again...")
               trying = True
            except KeyboardInterrupt as e:
                sys.exit(0)
    
        if msg[0] == "halt":
            sys.exit(0)
        elif msg[0] == "reset":
            return self.lastinfo
        return msg[1]

    def request_step(self, action):
        """
        This method change environment state under action named 'action'.
        Never override this method. If you want change step behavior,
        implements step_behavior method.
        """
        self.step_behavior(action)
        action = {}
        action['name'] = self.actionName
        action['args'] = self.actionArgs
        action['fields'] = self.fields
        self.agent.qin.put(['act', action])
        info = None
        try:
            info = self.agent.qout.get(timeout=self.agent.timeout)
        except TimeoutError as e:
            print(f"Step timeout after {self.agent.timeout} seconds!")
            sys.exit(0)
        except KeyboardInterrupt:
            sys.exit(0)
        except Empty as e:
            print("Empty message in request step.")
            info = self.agent.last_info
    
        if info=="halt":
            sys.exit(0)
        self.restoreDefaultAction()
        if not info:
            return self.lastinfo
        self.lastinfo = info[1]
        return info[1]

    def restoreDefaultAction(self):
        self.actionName = "__waitnewaction__"
        self.actionArgs = [0]
        self.fields = None

class BasicAgent:
    rl_env_control = {
        'max_steps': 1000,
        'agent_id': 0
    }

    def __init__(self, qin:Queue, qout:Queue, waittime:float=0.01, timeout:float=10):
        self.max_step = 0
        self.id = 0
        self.controller = BasicController()
        self.qin = qin
        self.qout = qout
        self.request_reset = False
        self.request_action = False
        self.request_config = False
        self.waittime = waittime
        self.action = None
        self.initial_state = None #initial state is a new state after reseting.
        self.new_state = False #new state is used to indicate state arriving after action ran.
        self.endOfEpisode = False #this flag indicate the end of episode.
        self.halt = False
        self.timeout = timeout
        self.last_info = None
        t = Thread(target=self.cmdserver)
        t.start()

    def cmdserver(self):
        while True:
            try:
                if self.halt:
                    sys.exit(0)
                cmd = None
                cmd = self.qin.get()
                if cmd[0] == "reset":
                    self.request_reset = True
                    with self.qin.mutex:
                        self.qin.queue.clear()
                elif cmd[0] == "act":
                    self.action = cmd[1]
                    self.request_action = True
                elif cmd[0] == "config":
                    with self.qin.mutex:
                        self.qin.queue.clear()
                    self.request_config = True

                time.sleep(self.waittime)
            except KeyboardInterrupt as e:
                print("Exiting...")
                sys.exit(0)

    def act(self, info):
        self.last_info = info

        if self.request_config:
            self.request_config = False
            return step("__noop__", [0])

        if self.request_reset:
            self.initial_state = None
            self.request_reset = False
            self.endOfEpisode = False
            self.new_state = False
            return step("__reset__", [0])
        
        if self.request_action:
            self.request_action = False
            self.new_state = True
            if "fields" in self.action:
                return steps(self.action['name'], self.action['args'], self.action['fields'])
            else:
                return step(self.action['name'], self.action['args'])

        if self.initial_state is None and not info['done']: #first action after reseting
            self.initial_state = info
            self.endOfEpisode = False
            with self.qout.mutex:
                self.qout.queue.clear()
            self.__get_controller().handleNewEpisode(info)
            self.qout.put(("reset", info))
            
        if self.new_state and not info['done']:
            self.new_state = False
            #if not self.qout.full():
            self.qout.put( ("step", info) )

        if info['done']:
            self.new_state = False
            self.initial_state = None
            if not self.endOfEpisode:
                self.endOfEpisode = True
                self.qout.put( ("step", info) )
                self.__get_controller().handleEndOfEpisode(info)
        
        return step("__waitnewaction__", [0])

    def handleEnvCtrl(self, a):
        if 'config' in a:
            self.max_steps = a['max_steps']
            self.id = a['id']
            self.modelmetadata = a['modelmetadata']
            control = []
            control.append(stepfv('max_steps', [self.max_steps]))
            control.append(stepfv('id', [self.id]))
            self.__get_controller().handleConfiguration(self.id, self.max_step, self.modelmetadata)
            print("-------------------------------------------------------------------------")
            print(a)
            if "__reset__" in a:
                self.qout.put( ("reset", a) )
            else:
                self.qout.put( ("config", a) )
            return ("@".join(control))
        if '__stop__' in a:
            print("--*-- GAME IS CLOSED --*--")
            print(f"This agent will kill in {self.timeout} seconds...")
            self.qout.put("halt")
            self.qin.put("halt")
            sys.exit()
        if 'wait_command' in a:
            if self.request_reset:
                self.initial_state = None
                self.request_reset = False
                self.endOfEpisode = False
                self.new_state = False
                return step("__reset__", [0])
            elif self.request_config:
                self.request_config  = False
                self.qout.put( ("config", a) )
                return step("__start_agent__", [0])
        return stepfv('__noop__', [0])

    def __get_controller(self):
        if (self.controller):
            return self.controller
        else:
            print("ERROR(agents.py): agent without controller!")
            sys.exit(0)
