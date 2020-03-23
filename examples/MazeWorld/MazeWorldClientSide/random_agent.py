from unityremote.core import RemoteEnv
import numpy as np

from threading import Thread



def run_agent(inport, outport):
	env = RemoteEnv(IN_PORT=inport, OUT_PORT=outport)
	env.open(0)

	speed = 100
	angular_speed = 50

	actions = [('fx', speed), ('fx', -speed), ('fy', speed), ('fy', -speed), ('left_turn', angular_speed), ('left_turn', -angular_speed), ('right_turn', angular_speed), ('right_turn', -angular_speed), ('up',speed),
	            ('down', speed), ('crouch', True), ('crouch', False)]

	action_size = len(actions)
	    
	for i in range(100000):
	    idx = np.random.choice(action_size)
	    env.step(actions[idx][0], actions[idx][1])

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





