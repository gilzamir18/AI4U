import ai4u
from controller import SimpleController
from ai4u.appserver import startasdaemon
import numpy as np

FLOOR = 1
WALL = 50
ENEMY = 100
NPC = 150
DIRTY = 200
OTHER = 255

ids = ["0"]
controllers_classes =  [SimpleController]
controller = startasdaemon(ids, controllers_classes)[0]

map = {'W': 0, 'S': 1, 'L': 2, 'A': 3, 'D': 4, 'T': 5, 'P': 6, 'R': 7, 'U': 8}

def parse(state):
    return np.array(state['vision'], dtype=np.uint8).reshape(20, 20), state['reward'], state['done'], {}


grid, score, done, _ = parse(controller.request_reset())

actions = ['W', 'S', 'L', 'A', 'D']

r = np.random.choice(actions)
action = map[r]
total = 0
while not done:
    grid, score,  done, info = parse(controller.request_step(action, False))
    print(grid)
    total += score
    r = np.random.choice(actions)
    action = map[r.upper()]
print("Sum of scores: ", total)

