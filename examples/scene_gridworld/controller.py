from ai4u.utils import stepfv
from ai4u.ai4u2unity import create_server
import ai4u
from ai4u.agents import BasicController

class SimpleController(BasicController):
    def __init__(self):
        super().__init__()

    def handleNewEpisode(self, info):
        print("Begin of  Episode")

    def handleEndOfEpisode(self, info):
        print("End of Episode...")
    
    def configure(self, id, max_step):
        print("Agent configuration: id=", id, " maxstep=", max_step)

    def step_behavior(self, action):
        self.actionName = "grid"
        if action == 0:
            self.actionArgs = [0]
        elif action == 1:
            self.actionArgs = [1]
        elif action == 2:
            self.actionArgs = [2]
        elif action == 3:
            self.actionArgs = [3]
        elif action == 4:
            self.actionArgs = [4]
        elif action == 5:
            self.actionName = "__stop__"
            self.actionArgs = [0]
        elif action == 6:
            self.actionName = "__pause__"
            self.actionArgs = [0]
        elif action == 7:
            self.actionName = "__resume__"
            self.actionArgs = [0]
        elif action == 8:
            self.actionName = "__restart__"
            self.actionArgs = [0]
