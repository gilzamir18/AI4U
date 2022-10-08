import ai4u
from controller import SimpleController
from ai4u.appserver import startasdaemon


ids = ["0"]
controllers_classes =  [SimpleController]
controller = startasdaemon(ids, controllers_classes)[0]

map = {'A': 1, 'D': 2, 'W': 3, 'S': 4, 'T': 5, 'P': 6, 'R': 7, 'U': 0, 'E': 8}
state = controller.request_reset()


print('''
AI4UConnect Demo.
Use:
W: forward
A: turn left
S: backward
D: turn right
U: Jump
F: JumpForward
L: Print State
T: exit
E: Restart
''')
r = input("Action? ")
while True:
    if r.upper() == "L":
        print(state)
    else:
        action = map[r.upper()]
        if action == 8:
            state = controller.request_reset()
        elif action == 5:
            break
        else:
            state = controller.request_step(action)
    r = input("Action ?")
controller.close()
