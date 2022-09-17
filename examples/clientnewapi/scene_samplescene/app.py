from ai4u.agents import BasicAgent
from ai4u.utils import stepfv
from ai4u.ai4u2unity import create_server
import ai4u
from controller import SimpleController
from ai4u.appserver import startasdaemon


ids = ["0"]
controllers_classes =  [SimpleController]
controller = startasdaemon(ids, controllers_classes)[0]

map = {'A': 1, 'D': 2, 'W': 3, 'S': 4, 'T': 5, 'P': 6, 'R': 7, 'U': 0}
state = controller.request_reset()
print(state)
r = input("Action? ")
action = map[r.upper()]
while action != 5:
    state = controller.request_step(action)
    print(state)
    r = input("Action ?")
    action = map[r.upper()]
