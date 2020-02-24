from unityremote.gmproc.ga3c import A3CMaster, A3CWorker
from unityremote.gmproc import ClientServer
from tensorflow import keras
from tensorflow.keras import layers
from unityremote.core import RemoteEnv, EnvironmentManager
from unityremote.utils import image_decode, get_image
import numpy as np
import threading
import time
from unityremote.gmproc import Workers
from collections import deque
import tensorflow as tf
import cv2
import base64
import os
import shutil



IMAGE_SHAPE = (20, 20, 4)
ARRAY_SIZE = 2
ACTION_SIZE = 5
NB_AGENTS = 4

class AgentWrapper(RemoteEnv):
	def __init__(self):
		super().__init__()
		self.buf = deque(maxlen=4)
		mem = np.zeros(shape=(20, 20))
		self.buf.append(mem)
		self.buf.append(mem)
		self.buf.append(mem)
		self.buf.append(mem)
		
		
		self.proprioceptions = np.zeros(ARRAY_SIZE)

	def step(self, action, value=None):
		state = None
		for i in range(8):
			state = super().step(action, value)
			if state['done']:
				break

		imgdata = image_decode(state['frame'], 20, 20)

		self.buf.append(imgdata)
		state['frame'] = np.array(self.buf, dtype=np.float32)
		state['frame'] = np.moveaxis(state['frame'], 0, -1)
		state['frame'] = state['frame'].reshape(1, 20, 20, 4)
		self.proprioceptions[0] = state['touched']
		self.proprioceptions[1] = state['energy']
		state['proprioception'] = np.array(self.proprioceptions, np.float32).reshape(1, len(self.proprioceptions))
		return state

	def reset(self):
		state = self.step('restart')
		self.energy = state['energy']
		return state

envs = EnvironmentManager(NB_AGENTS, wrapper=AgentWrapper)

envs.openAll()


class A3CModel(keras.Model):
	def __init__(self):
		super(A3CModel, self).__init__()
		self.norm = keras.layers.Lambda(lambda x: x/20.0, input_shape=IMAGE_SHAPE)

		self.conv1 = tf.keras.layers.Conv2D(16, (1,1), (1,1), activation='relu', name='conv1')

		self.conv2 = keras.layers.Conv2D(16, (3,3), (1,1), activation='relu', name='conv2')

		self.dense1 = layers.Dense(10, activation='relu')

		self.flattened = keras.layers.Flatten()

		self.expanded_features = keras.layers.Concatenate()

		self.hidden = keras.layers.Dense(512, activation='relu', name='hidden')

		self.policy_logits = layers.Dense(ACTION_SIZE)

		#self.dense2 = layers.Dense(10, activation='relu')
		
		self.values = layers.Dense(1)


	def call(self, inputs):
		x1 = self.norm(inputs[0])
		x1 = self.conv1(x1)
		x1 = self.conv2(x1)
		x1 = self.flattened(x1)
		
		x2 = self.dense1(inputs[1])

		features  = self.expanded_features([x1, x2])
		features  = self.hidden(features)

		logits = self.policy_logits(features)
		
		values = self.values(features)
		return logits, values

def env_maker(name):
	return envs[name]

def act(action, env):

	action_id = (0, 1, 3, 4, 8)

	s = env.step('act', action_id[action])

	delta = s['energy'] - env.energy
	reward = 0
	if s['energy'] > 300:
		if delta > 0:
			reward = -delta
	else:
		reward = delta
		if reward < 0:
			reward = 0;

	env.energy = s['energy']

	time.sleep(0.05)
	return s, reward, s['done'], s

def prepare_data(data):
	return (    tf.convert_to_tensor(data['frame']),
				tf.convert_to_tensor(data['proprioception']) )

def make_transition(mem, state, action, reward):
	if mem.size()==0:
		mem.states.append([])
		mem.states.append([])

	mem.states[0].append(state['frame'])
	mem.states[1].append(state['proprioception'])
	mem.actions.append(action)
	mem.rewards.append(reward)
	mem.count += 1

def prepare_array_data(data):
	return ( tf.convert_to_tensor(np.vstack(data[0]), dtype=tf.float32),
			 tf.convert_to_tensor(np.vstack(data[1]), dtype=tf.float32)
		 	)

def initialize_model(model, size):
	shape = tuple([1] + list(size[0]))
	x1 = tf.convert_to_tensor( np.random.random(shape), dtype=tf.float32 )
	x2 = tf.convert_to_tensor( np.random.random( (1, size[1]) ), dtype=tf.float32 )
	model( (x1, x2) )

if __name__=="__main__":

	if os.path.exists('./logs'):
		shutil.rmtree('./logs')
	os.mkdir('logs')

	params = {}
	params['action_size'] = ACTION_SIZE
	params['state_size'] = (IMAGE_SHAPE, ARRAY_SIZE) 
	params['update_freq'] = 30
	params['entropy_bonus'] = 0.01
	params['value_loss_coef'] = 0.5
	params['learning_rate'] = 0.0001
	params['max_grad_norm'] = 5
	params['gamma'] = 0.99
	params['model'] = A3CModel
	params['env_maker'] = env_maker
	params['act'] = act
	params['prepare_data'] = prepare_data
	params['make_transition'] = make_transition
	params['prepare_array_data'] = prepare_array_data
	params['initialize_model'] = initialize_model
	params['debug_freq'] = 10
	params['log_bsize'] = 1000
	params['log_verbose'] = True

	cs = ClientServer(A3CMaster)
	cs.new_workers(NB_AGENTS, A3CWorker, params=params)
	cs.run(params=params)
