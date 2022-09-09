from ai4u.agents import BasicAgent
from ai4u.utils import stepfv
from threading import Thread
from ai4u.ai4u2unity import create_server
import ai4u
from controller import SimpleController
from ai4u.appserver import startasdaemon, reset


ids = ["0"]
controllers_classes =  [SimpleController]
controller = startasdaemon(ids, controllers_classes)[0]

map = {'A': 1, 'D': 2, 'W': 3, 'S': 4, 'P': 5, 'R': 6, 'U': 0}

state = reset("0")
print(state)
r = input("Action? ")
action = map[r.upper()]
while action != 5:
    state = controller.step(action)
    print(state)
    r = input("Action ?")
    action = map[r.upper()]
    while action == 6:
        reset("0")
        r = input("Action? ")
        action = map[r.upper()]
