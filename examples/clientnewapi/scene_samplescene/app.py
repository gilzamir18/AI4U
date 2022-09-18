import ai4u
from controller import SimpleController
from ai4u.appserver import startasdaemon


ids = ["0"]
controllers_classes =  [SimpleController]
controller = startasdaemon(ids, controllers_classes)[0]

map = {'A': 1, 'D': 2, 'W': 3, 'S': 4, 'T': 5, 'P': 6, 'R': 7, 'U': 0, 'E': 8}
state = controller.request_reset()
print(state)
r = input("Action? ")
action = map[r.upper()]
while True:
    state = controller.request_step(action)
    print(state)
    r = input("Action ?")
    action = map[r.upper()]
    if action == 8:
        state = controller.request_reset()
    elif action == 5:
        break
controller.close()
