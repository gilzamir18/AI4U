
from ai4u.utils import stepfv
from ai4u.ai4u2unity import create_server
import ai4u
import gym
import numpy as np

class BasicGymController(ai4u.agents.BasicController):
    def __init__(self):
        super().__init__()
        self._seed = 0
        self.action_space = gym.spaces.Box(low=np.array([0,-1, 0, 0]),
                               high=np.array([1, 1, 1, 1]),
                               dtype=np.float32)

        #self.observation_space = gym.spaces.Box(low=0, high=255,
        #                                shape=(10, 10, 1), dtype=np.uint8)
        self.observation_space = gym.spaces.Dict(
            {
                "array": gym.spaces.Box(-1000, 1000, shape=(3,), dtype=float),
                "vision": gym.spaces.Box(0, 255, shape=(10, 10, 1), dtype=float),
            }
        )

    def handleNewEpisode(self, info):
        #print("Begin of  Episode")
        self.initialState = info

    def handleEndOfEpisode(self, info):
        self.agent.request_restart()
        self.initialState = None
    
    def seed(self, s):
        self._seed = s 
    
    def render(self):
        pass

    def close(self):
        pass

    def handleConfiguration(self, id, max_step):
        print("Agent configuration: id=", id, " maxstep=", max_step)

    def get_state(self, info):
        #print(info)
        if  type(info) is tuple:
            info = info[0]
        if ("vision" in info) and ("array" in info):
            vision = info["vision"]
            vision = np.reshape(vision, (10, 10, 1))
            array = info['array']
            return {'array': np.array([array]), 'vision': np.array([vision])}, info['reward'], info['done'], info
        else:
            return info

    def reset_behavior(self, info):
        vision = info['vision']
        vision = np.reshape(vision, (10, 10, 1))
        array = info['array']
        return {'array': np.array([array]), 'vision': np.array([vision])}

    def step_behavior(self, action):
        self.actionName = "move"
        if type(action) != str:
            self.actionArgs = np.array(action).squeeze() * 20
        elif action == 'stop':
            self.actionName = "__stop__"
            self.actionArgs = [0]
        elif action == 'pause':
            self.actionName = "__pause__"
            self.actionArgs = [0]
        elif action == 'resume':
            self.actionName = "__resume__"
            self.actionArgs = [0]
    