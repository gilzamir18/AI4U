from .utils import stepfv
from .ai4u2unity import create_server
from .agents import BasicController
import ai4u
import gym
import numpy as np
import sys


IMG_H = 10
IMG_W = 10
IMG_C = 1

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
        self.action_space = gym.spaces.Box(low=np.array([0,-1, 0, 0]),
                               high=np.array([1, 1, 1, 1]),
                               dtype=np.float32)

        #self.observation_space = gym.spaces.Box(low=0, high=255,
        #                                shape=(10, 10, 1), dtype=np.uint8)
        self.observation_space = gym.spaces.Dict(
            {
                "array": gym.spaces.Box(-1, 1, shape=(7,), dtype=np.float32),
                "vision": gym.spaces.Box(0, 1, shape=(IMG_C * IMG_W * IMG_H, ), dtype=np.float32),
            }
        )

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

    def handleConfiguration(self, id, max_step):
        print("Agent configuration: id=", id, " maxstep=", max_step)

    def get_state(self, info):
        """
        This method transform AI4U data structure to a
        shape supported by OpenGym based environments.
        """
        #print(info)
        if  type(info) is tuple:
            info = info[0]
        if ("vision" in info) and ("array" in info):
            vision = info["vision"]
            vision = np.reshape(vision, (IMG_C * IMG_W * IMG_H, )) * 1/256
            array = info['array']
            return {'array': np.array([array], dtype=np.float32), 'vision': np.array([vision], dtype=np.float32)}, info['reward'], info['done'], info
        else:
            return info

    def reset_behavior(self, info):
        """
        Here you implement whatever is necessary to configure 
        an episode's initial settings and return the first 
        observation that an agent will use to start performing
        actions. In this example, we only extract the initial 
        state from the information sent by the game environment
        implemented in the AI4UTesting code.
        """
        vision = info['vision']
        vision = np.reshape(vision, (IMG_C *  IMG_W * IMG_H, )) * 1/256
        array = info['array']
        return {'array': np.array([array], dtype=np.float32), 'vision': np.array([vision], dtype=np.float32)}

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
    