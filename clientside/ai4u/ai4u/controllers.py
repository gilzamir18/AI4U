
from ai4u.utils import stepfv
from ai4u.ai4u2unity import create_server
import ai4u
import gym
import numpy as np

class BasicGymController(ai4u.agents.BasicController):
    def __init__(self):
        super().__init__()
        self._seed = 0
        self.action_space = gym.spaces.Box(low=np.array([-1,-1,-1]),
                               high=np.array([1,1,1]),
                               dtype=np.float32)

        self.observation_space = gym.spaces.Box(low=0, high=255,
                                        shape=(10, 10, 1), dtype=np.uint8)
    def newEpisode(self, info):
        print("Begin of  Episode")
        self.initialState = info

    def endOfEpisode(self, info):
        super().endOfEpisode(info)
        print("End of Episode...")
        self.initialState = None
    
    def seed(self, s):
        self._seed = s 
    
    def render(self, mode):
        pass

    def close(self):
        pass

    def configure(self, id, max_step):
        print("Agent configuration: id=", id, " maxstep=", max_step)

    def toGymState(self, info):
        if "vision" in info and "array" in info:
            vision = info["vision"]
            vision = np.reshape(vision, (10, 10, 1))
            array = info['array']
            return {'array': [array], 'vision': [vision]}, info['reward'], info['done'], info
        else:
            return info

    def step_behavior(self, action):
        self.actionName = "move"
        if type(action) != str:
            self.actionArgs = np.array(action) * 20
        elif action == 'stop':
            self.actionName = "__stop__"
            self.actionArgs = [0]
        elif action == 'pause':
            self.actionName = "__pause__"
            self.actionArgs = [0]
        elif action == 'resume':
            self.actionName = "__resume__"
            self.actionArgs = [0]
