from unityremote.core import RemoteEnv
import numpy as np
import time
from threading import Thread
from unityremote.utils import image_from_str


def run_agent(inport, outport):
	env = RemoteEnv(IN_PORT=inport, OUT_PORT=outport)
	env.open(0)

	speed = 100
	angular_speed = 50

	actions = [('walk', 1), ('run', 15), ('walk_in_circle', 1), ('left_turn', 1), ('right_turn', 1), ('up', 1),
				('down', 1), ('jump', True), ('pickup', True), ('pickup', False), ('noop', -1)]

	action_size = len(actions)
		
	for i in range(100000):		
		sum_rewards = 0
		touchID = 0
		energy = 0
		idx = np.random.choice(len(actions))
		for i in range(4):
			env_info = env.step(actions[idx][0], actions[idx][1])
			done = env_info['done']
			if not done:
				env_info = env.step('get_result', -1)
				sum_rewards += env_info['reward']
				touchID = env_info['touchID']
				energy = env_info['energy']
			if done:
				sum_rewards += env_info['reward']
				touchID = env_info['touchID']
				energy = env_info['energy']
				env_info = env.step('restart', -1)
				break
			if touchID != 0:
				if inport == 8080:
					print("Object touched ---------------- ",  touchID)
					print("Reward sum -------------------- ", sum_rewards)
					print("-----------------------------------------------")
					print(env_info['frame'])
	env.close()

def r1():
	run_agent(8080, 7070)	

def r2():
	run_agent(8081, 7071)

def r3():
	run_agent(8082, 7072)

def r4():
	run_agent(8083, 7073)

t1 = Thread(target=r1)
t1.start()
t1.join()

'''
t2 = Thread(target=r2)
t2.start()

t3 = Thread(target=r3)
t3.start()

t4 = Thread(target=r4)
t4.start()

t1.join()
t2.join()
t3.join()
t4.join()
'''




