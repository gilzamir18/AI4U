from unityremote.gmproc.ga3c import A3CMaster, A3CWorker
from unityremote.gmproc import ClientServer
from tensorflow import keras
from tensorflow.keras import layers
from unityremote.core import RemoteEnv, EnvironmentManager
from unityremote.utils import image_decode, image_from_str, get_image
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



IMAGE_SHAPE = (10, 10, 1)
ACTION_SIZE = 4
NB_AGENTS = 2

class AgentWrapper(RemoteEnv):
	def __init__(self):
		super().__init__()

	def step(self, action, value=None):
		return super().step(action, value)
		
	def reset(self):
		state = self.step('restart')
		return state

envs = EnvironmentManager(NB_AGENTS, wrapper=AgentWrapper)

envs.openAll()


class A3CModel(keras.Model):
	def __init__(self):
		super(A3CModel, self).__init__()
		self.norm = keras.layers.Lambda(lambda x: x/5.0, input_shape=IMAGE_SHAPE)

		self.conv1 = tf.keras.layers.Conv2D(16, (1,1), (1,1), activation='relu', name='conv1')

		self.conv2 = keras.layers.Conv2D(16, (3,3), (1,1), activation='relu', name='conv2')

		self.flattened = keras.layers.Flatten()

		self.hidden = keras.layers.Dense(512, activation='relu', name='hidden')

		self.policy_logits = layers.Dense(ACTION_SIZE)
		
		self.values = layers.Dense(1)


	def call(self, inputs):
		x1 = self.norm(inputs)
		x1 = self.conv1(x1)
		x1 = self.conv2(x1)
		x1 = self.flattened(x1)

		features  = self.hidden(x1)

		logits = self.policy_logits(features)
		
		values = self.values(features)
		return logits, values

def env_maker(name):
	return envs[name]

def act(action, env):
	action_id = (0, 1, 2, 3)

	s = env.step('move', action_id[action])

	reward = s['reward']

	time.sleep(0.05)
	return s, reward, s['done'], s

'''
	O método extrai o estado atual de data eo transfroma em um tensor.
	data é o que é retornado pelo RemoteEnv (no caso, de AgentWrapper, que é filha de RemoteEnv).
'''
def prepare_data(data):
	img = image_from_str(data['state'], 10, 10)
	img = img.reshape(1, 10, 10)
	img = np.moveaxis(img, 0, -1)
	img = np.array([img])
	return tf.convert_to_tensor(img, dtype=tf.float32)

'''
	Armazena a transição de um estado em state, dada uma acao action e a recompensa reward.
'''
def make_transition(mem, state, action, reward):
	mem.store(prepare_data(state), action, reward)

'''
Recebe um array de estados (o que é retornado por prepare_data) e o transforma em um tensor.
'''
def prepare_array_data(data):
	return tf.convert_to_tensor(np.vstack(data), dtype=tf.float32)

#Inicializa o modelo
def initialize_model(model, size):
	shape = tuple([1] + list(size))
	x1 = tf.convert_to_tensor( np.random.random(shape), dtype=tf.float32 )
	model(x1)

if __name__=="__main__":

	if os.path.exists('./logs'):
		shutil.rmtree('./logs')
	os.mkdir('logs')

	params = {}
	params['action_size'] = ACTION_SIZE
	params['state_size'] = IMAGE_SHAPE 
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
