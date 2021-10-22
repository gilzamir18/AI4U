import gym
from ai4u.core import RemoteEnv;
from ai4u.utils import image_from_str, str_as_dictlist, environment_definitions
import numpy as np
from stable_baselines3 import SAC
import AI4UGym
from AI4UGym import BasicAgent
from collections import deque

SEQ_SIZE = 4
JUMP_POWER = 10
ACTION_SIZE = 6
LINEAR_SIZE = 12
IMG_SIZE = (20, 20, SEQ_SIZE)
SKIP_FRAMES = 8

def parse_state(info, collision_array=None):
	if collision_array is None:
		collision_array = np.zeros(8)
	linear_input = np.zeros(LINEAR_SIZE, np.float32)
	
	my_position = info['my_position']
	linear_input[0] = my_position[0]
	linear_input[1] = my_position[1]
	linear_input[2] = my_position[2]
	linear_input[3] = 1.0 if info['is_on_floor'] else 0.0
	idx = 4
	for collision in collision_array:
		linear_input[idx] = collision
		idx += 1
		if idx >= LINEAR_SIZE:
			idx = 4
			break

	array = info["raycasting"].split(":")
	img = image_from_str(array[0], 20, 20, np.float32)
	depth = image_from_str(array[1], 20, 20, np.float32)
	return linear_input, img, depth

def make_env_def():
	environment_definitions['observation_space'] = gym.spaces.Dict({
            "imageSequence": gym.spaces.Box(0, 255, shape=IMG_SIZE),
            "linearInput": gym.spaces.Box(-1, 1, shape=(SEQ_SIZE, LINEAR_SIZE, ))
        })

	environment_definitions['action_shape'] = (ACTION_SIZE,)
	environment_definitions['action_space'] = gym.spaces.Box(np.array([0,0,0,0,0,0]), np.array([2,2,1,1,10,10]))  # walking, bacward, left, right, jump, jump forward
	environment_definitions['input_port'] = 8080
	environment_definitions['output_port'] = 8081
	environment_definitions['agent'] = Agent
	environment_definitions['host'] = '127.0.0.1'
	BasicAgent.environment_definitions = environment_definitions

class Agent(BasicAgent):
	def __init__(self):
		BasicAgent.__init__(self)
		self.seq = deque(maxlen=SEQ_SIZE)
		self.lseq = deque(maxlen=SEQ_SIZE)

	def __make_state__(imageseq):
		frameseq = np.array(imageseq, dtype=np.float32)
		frameseq = np.moveaxis(frameseq, 0, -1)
		return frameseq

	def reset(self, env):
		env_info = env.remoteenv.step("restart")
		
		for i in range(np.random.choice(15)):
			env_info = env.remoteenv.stepfv('character', [0, 0, 0, 0, 0, JUMP_POWER])
		
		li, img, dpt = parse_state(env_info)

		for _ in range(SEQ_SIZE):
			self.seq.append(img)
			self.lseq.append(np.zeros(LINEAR_SIZE, np.float32))
	
		return {'imageSequence': Agent.__make_state__(self.seq), 'linearInput': np.zeros( (SEQ_SIZE, LINEAR_SIZE), np.float32)}

	def __step(self, env, action):
		reward_sum = 0
		done = False
		collision_array = np.zeros(8, np.float32)
		data = {}
		for i in range(SKIP_FRAMES):
			data = env.remoteenv.stepfv('character', action)
			if -0.5 in data:
				collision_array[i] += 0.33
			if 0.5 in data:
				collision_array[i] += 0.66
			reward_sum += data['reward']
			done  = data['done']
			if (done):
				break
		li, img, dpt = parse_state(data, collision_array)
		self.seq.append(img)
		self.lseq.append(li)
		linearSequence = np.array(self.lseq, np.float32)
		return {'imageSequence':Agent.__make_state__(self.seq), 'linearInput': linearSequence}, reward_sum, done, data

	def act(self, env, action, info=None):
		return self.__step(env, action)



make_env_def()

env = gym.make("AI4U-v0")
env.configure(environment_definitions)
model = SAC("MultiInputPolicy", env, verbose=1)
model.learn(total_timesteps=10000, log_interval=4)
model.save("sac_ai4u")

del model # remove to demonstrate saving and loading

model = SAC.load("sac_ai4u")

obs = env.reset()
while True:
    action, _states = model.predict(obs, deterministic=True)
    obs, reward, done, info = env.step(action)
    env.render()
    if done:
      obs = env.reset()