from .utils import stepfv
from .ai4u2unity import create_server
from .agents import BasicController
import ai4u
import gym
import numpy as np
import sys
import json
from .types import SENSOR_SFLOATARRAY, SENSOR_SINTARRAY, SENSOR_SINT, SENSOR_SBOOL, SENSOR_SFLOAT

codetypes = {0: np.float32, 1: None, 2: np.uint8, 3: np.uint8, 4: None, 5: np.float32, 6: np.uint8}

class BasicGymController(BasicController):
    """
    This basic controller only works with Unity or Godot AI4UTesting
    application or similar environments. For a custom controller,
    create an class based on BasicGymController and put it as argument
    of the command gym.make. For example:

    gym.make("AI4UEnv-v0", controller_class=MyController)

    where MyController is the controller class that you create based
    on ai4uagents.BasicGymController.
    """
    def __init__(self):
        """
        Controller constructor don't have arguments.
        """
        super().__init__()
        self._seed = 0
        self.action_space = None
        self.observation_space = None

    def handleNewEpisode(self, info):
        """
        Implement this method if you have an important thing to do 
        after a new episode started.
        """
        print("Begin of the episode....")

    def handleEndOfEpisode(self, info):
        """
        Implement this method if you have an important thing to do 
        after the current ending. May be  that you want create a 
        new episode with request_restart command to agent.
        """
        print("End Of Episode")
        
    def seed(self, s):
        """
        This method prepare the environment with
        random initialization based on seed 's'.
        """
        self._seed = s 
    
    def render(self):
        """
        This method has been maintained to maintain compatibility with 
        the Gym environment standard. It is important that you maintain 
        this method.
        """
        pass

    def close(self):
        """
        Release allocated resources .
        """
        sys.exit(0)

    def convertoboxspace(self, modelinput):
        shape = modelinput["shape"]
        data_dim = len(shape) - 1
        if data_dim == 1:
            size = shape[-1]
            return gym.spaces.Box(modelinput["rangeMin"], modelinput["rangeMax"], shape=(shape[0] * size, ), dtype=codetypes[modelinput["type"]]), False
        if data_dim == 2:
            return gym.spaces.Box(modelinput["rangeMin"], modelinput["rangeMax"], shape=shape, dtype=codetypes[modelinput["type"]]), True
        else:
            raise Exception("Controller configuration: unsuported data dimenstion: ", shape, ". Check your environment configuration. Perception Key: ", modelinput['name'])


    def loadarrayfrominput(self, modelinput, info):
        return  np.array([ info[modelinput['name']] ], dtype=codetypes[modelinput['type']])
    
    def loadimagefrominput(self, modelinput, info):
        vision = np.reshape(info[ modelinput['name'] ], modelinput['shape'])
        return  np.array([vision], dtype=codetypes[modelinput['type']] )

    def dict_input_extractor(self, modelinputs, info):
        inputs = {}
        for i in modelinputs:
            shape = i['shape']
            data_dim = len(shape) - 1
            if data_dim == 1:
                inputs[i['name']] = self.loadarrayfrominput(i, info)
            elif data_dim == 2:
                inputs[i['name']] = self.loadimagefrominput(i, info)
            else:
                raise Exception("Controller configuration: unsuported data dimenstion: ", shape, ". Check agetn's environment configuration by Perception Key: ", i['name'], ".")
        return inputs


    def floatarrayoutput(self, action, outputmodel):
        self.actionName = "move"
        if type(action) != str:
            self.actionArgs = np.array(action).squeeze()
        elif action == 'stop':
            self.actionName = "__stop__"
            self.actionArgs = [0]
        elif action == 'pause':
            self.actionName = "__pause__"
            self.actionArgs = [0]
        elif action == 'resume':
            self.actionName = "__resume__"
            self.actionArgs = [0]

    def onehotoutput(self, action, outputmodel):
        self.floatarrayoutput(self, action, outputmodel)

    def handleConfiguration(self, id, max_step, metadatamodel):
        print("Agent configuration: id=", id, " maxstep=", max_step)
        print("Metadata Model " + metadatamodel)
        self.metadataobj = json.loads(metadatamodel)
        self.inputs = self.metadataobj['inputs']
        self.outputs = self.metadataobj['outputs']
        if len(self.inputs) == 1: #single box input
            self.observation_space, isImage = self.convertoboxspace(self.inputs[0])
            if isImage:
                self.input_extractor = self.loadimagefrominput
            else:
                self.input_extractor = self.loadarrayfrominput
        elif len(self.inputs) > 1: #dictionary input
            dict_space = gym.spaces.Dict()
            for i in self.inputs:
                dict_space[i['name']], isImage = self.convertoboxspace(i)
            self.observation_space = dict_space
            self.input_extractor = self.dict_input_extractor
        
        if len(self.outputs) == 1:
            out = self.outputs[0]
            if out['isContinuous']:
                self.action_space = gym.spaces.Box(low=np.array(out['rangeMin']), high=np.array(out['rangeMax']), dtype=np.float32)
                self.output_controller = self.floatarrayoutput
            else:
                self.action_space = gym.spaces.Discrete(out['shape'][-1])
                self.output_controller = self.onehotoutput
        else:
            raise Exception("Controller configuration: multiple model outputs is not supported.")


    def extractstatefrominputs(self, modelinputs, info):
        if (len(self.inputs) == 1):
            return self.input_extractor(self.inputs[0], info)
        else:
            return self.input_extractor(self.inputs, info)

    def get_state(self, info):
        """
        This method transform AI4U data structure to a
        shape supported by OpenGym based environments.
        """
        #print(info)
        if  type(info) is tuple:
            info = info[0]
    
        return self.extractstatefrominputs(self.inputs, info), info["reward"], info['done'], info

    def reset_behavior(self, info):
        """
        Here you implement whatever is necessary to configure 
        an episode's initial settings and return the first 
        observation that an agent will use to start performing
        actions. In this example, we only extract the initial 
        state from the information sent by the game environment
        implemented in the AI4UTesting code.
        """
        return self.extractstatefrominputs(self.inputs, info)

    def step_behavior(self, action):
        """
        In this method, by changing the values of the 
        actionName and actionArgs attributes,
        you define the action to be performed based on
        the action (action argument) returned by the agent.
        Therefore, this method is called when an agent makes
        a decision, producing the action represented by the
        variable "action".
        """
        for o in self.outputs:
            self.output_controller(action, o)
