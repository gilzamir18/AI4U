import os
os.environ["CUDA_VISIBLE_DEVICES"] = ""

import threading
import multiprocessing
import numpy as np
from queue import Queue
import argparse
from tensorflow import keras
from tensorflow.keras import layers
from .gmproc import ClientServer, ClientWorker, ServerWorker
import tensorflow as tf
import time
import datetime

tf.compat.v1.enable_eager_execution()

class LogManager():
	def __init__(self, base_name, blockLines = 1000):
		self.idx = 0
		self.base_name = base_name
		self.blockLines = blockLines
		self.filename = "%s_%d"%(base_name, self.idx)
		self.count = 0
		self.file = None
		self.enabled = True
		self.verbose = False

	def set_on(self):
		self.enabled = True

	def set_off(self):
		self.enabled = False

	def addline(self, line):
		if self.verbose:
			print(line)

		if self.enabled:
			if self.file is None:
				self.file = open(self.filename, 'w')

			self.file.write(line + '\n')

			self.count += 1

			if self.count >= self.blockLines:
				self.file.close()
				self.idx += 1
				self.filename = "%s_%d"%(self.base_name, self.idx)
				self.file = open(self.filename, 'w')
				self.count = 0

	def close(self):
		if (self.file is not None):
			self.file.close()
			self.reset()

	def reset(self):
		self.idx = 0
		self.count = 0
		if self.file is not None:
			self.file.close()
		self.filename = "%s_%d"%(self.base_name, self.idx)
		self.file = None


class Memory:
	def __init__(self):
		self.states = []
		self.actions = []
		self.rewards = []
		self.count = 0

	def size(self):
		return self.count

	def store(self, state, action, reward):
		self.states.append(state)
		self.actions.append(action)
		self.rewards.append(reward)
		self.count += 1

	def clear(self):
		self.states = []
		self.actions = []
		self.rewards = []
		self.count = 0

class A3CModel(keras.Model):
	def __init__(self, state_size, action_size):
		super(A3CModel, self).__init__()
		self.state_size = state_size
		self.action_size = action_size
		self.dense1 = layers.Dense(10, activation='relu')
		self.policy_logits = layers.Dense(action_size)
		self.dense2 = layers.Dense(10, activation='relu')
		self.values = layers.Dense(1)

	def call(self, inputs):
		# Forward pass
		x = self.dense1(inputs)
		logits = self.policy_logits(x)
		v1 = self.dense2(inputs)
		values = self.values(v1)
		return logits, values


def __initialize_model__(model, state_size):
	model( tf.convert_to_tensor( np.random.random( (1, state_size) ), dtype=tf.float32 ) )

class A3CMaster(ServerWorker):
	def __init__(self):
		super().__init__()
		self.id = "sharednetwork"
		self.model_save_counter = 0


	def start(self, params):

		if 'log_manager' in params:
			self.log_manager = params['log_manager']
		else:
			log_bsize = 1000
			if 'log_bsize' in params:
				log_bsize = params['log_bsize']

			log_verbose = False
			if 'log_verbose' in params:
				log_verbose = params['log_verbose']


			self.log = LogManager("logs/server.log", log_bsize)
			self.log.verbose = log_verbose
		self.model_save_counter = 0
		self.log.addline('starting server %s on time %s'%(self.id, datetime.datetime.now().strftime("%Y%m%d-%H%M%S")))
		self.learning_rate = params['learning_rate']
		self.action_size = params['action_size']
		self.state_size = params['state_size']

		if 'initialize_model' in params:
			self.initialize_model = params['initialize_model']
		else:
			self.initialize_model = __initialize_model__

		if 'model_save_freq' in params:
			self.model_save_freq = params['model_save_freq']
		else:
			self.model_save_freq = 300

		if 'model' in params:
			self.model = params['model']()
		else:
			self.model = A3CModel(self.state_size, self.action_size)

		if 'load_model' in params:
			self.model = tf.keras.models.load_model(params['load_model'])
		
		self.initialize_model(self.model, self.state_size)

		if 'opt' in params:
			self.opt = params['opt']()
		else:
			self.opt = tf.keras.optimizers.Adam(lr=params['learning_rate'])
	
	def process(self, id, msg):
		if msg is not None:
			grads = [None]*len(msg)
			for i in range(len(msg)):
				grads[i] = tf.convert_to_tensor(msg[i], dtype=tf.float32)
			self.opt.apply_gradients(zip(grads, self.model.trainable_weights))
		self.model_save_counter += 1
		if self.model_save_counter % self.model_save_freq == 0:
			self.model.save_weights('model')
			self.model_save_counter = 0
		return self.model.get_weights()


def __act__(action, env):
	state, reward, is_done, info = env.step(action)
	return state, reward, is_done, info

def __prepare_data__(data):
	return tf.convert_to_tensor(data[None, :], dtype=tf.float32)

def __make_transition__(mem, state, action, reward):
	mem.store(state, action, reward)

def __prepare_array_data__(data):
	return tf.convert_to_tensor(np.vstack(data), dtype=tf.float32)

class A3CWorker(ClientWorker):
	def __init__(self, id):
		super().__init__()
		self.id = id
		self.started = False
		self.total_step = 1
		self.time_counter = 0
		self.ep_steps = 0
		self.ep_loss = 0
		self.is_done = False
		self.global_episodes = 0
		self.mem = Memory()
		self.ep_reward = 0.0
		self.update_counter = 0
		self.ep_score = 0
		self.ep_entropy = 0.0
		self.ep_values = 0.0

	def start(self, params):

		if 'log_manager' in params:
			self.log = params['log_manager']()
		else:
			log_bsize = 1000
			if 'log_bsize' in params:
				log_bsize = params['log_bsize']

			log_verbose = False
			if 'log_verbose' in params:
				log_verbose = params['log_verbose']

			self.log = LogManager('logs/ga3c%d'%(self.id), log_bsize)
			self.log.verbose = log_verbose

		self.log.addline("starting client %s"%(self.id))
		self.sess = tf.compat.v1.Session()
		if 'env_name' in params:
			self.ENV_NAME = params['env_name']
		else:
			self.ENV_NAME = self.id

		self.env = params["env_maker"](self.ENV_NAME)
		self.UPDATE_FREQ = params['update_freq']
		self.action_size = params['action_size']
		self.state_size = params['state_size']


		self.max_n_ep = 1000000
		if 'max_ep' in params:
			self.max_n_ep = params['max_ep']

		if 'debug_freq' in params:
			self.DEBUG_FREQ = params['debug_freq']
		else:
			self.DEBUG_FREQ = 100
			
		if 'make_transition' in params:
			self.make_transition = params['make_transition']
		else:
			self.make_transition = __make_transition__

		if 'prepare_data' in params:
			self.prepare_data = params['prepare_data']
		else:
			self.prepare_data = __prepare_data__


		if 'prepare_array_data' in params:
			self.prepare_array_data = params['prepare_array_data']
		else:
			self.prepare_array_data = __prepare_array_data__


		if 'act' in params:
			self.act = params['act']
		else:
			self.act = __act__

		if 'initialize_model' in params:
			self.initialize_model = params['initialize_model']
		else:
			self.initialize_model = __initialize_model__

		if 'model' in params:
			self.model = params['model']()
		else:
			self.model = A3CModel(self.state_size, self.action_size)

		if 'load_model' in params:
			self.model = tf.keras.models.load_model(params['load_model'])
	
		self.initialize_model(self.model, self.state_size)

		self.value_loss_coef = params['value_loss_coef']
		self.entropy_bonus = params['entropy_bonus']
		self.max_grad_norm = params['max_grad_norm']
		self.gamma = params['gamma']

		if 'opt' in params:
			self.opt = params['opt']()
		else:
			self.opt = tf.keras.optimizers.Adam(lr=params['learning_rate'])

		self.current_state = self.env.reset()

	def get_loss(self):
		loss = self.ep_loss.numpy()
		avg = 0
		if self.ep_steps > 0:
			avg = np.mean(loss)/self.ep_steps
		return avg

	def process(self):
		if self.started:
			while self.time_count < self.UPDATE_FREQ and not self.is_done:
				logits, _ = self.model(self.prepare_data(self.current_state))

				probs = tf.nn.softmax(logits)

				action = np.random.choice(self.action_size, p=probs.numpy()[0])

				new_state, reward, is_done, _ = self.act(action, self.env)
				self.is_done = is_done

				self.ep_reward += reward

				self.make_transition(self.mem, self.current_state, action, reward)

				self.total_step += 1
				self.ep_steps += 1
				self.current_state = new_state
				self.time_count += 1
				# Calculate gradient wrt to local model. We do so by tracking the
				# variables involved in computing the loss by using tf.GradientTape mem.store(current_state, action, reward)
			if self.mem.size() > 0:
				with tf.GradientTape() as tape:
					total_loss = self.compute_loss(is_done, new_state, self.mem, self.gamma)
				self.is_done = is_done
				self.ep_loss += total_loss
				# Calculate local gradients

				grads = tape.gradient(total_loss, self.model.trainable_weights)
				grads, _ = tf.clip_by_global_norm(grads, self.max_grad_norm)

				results = []
				for i in range(len(grads)):
					results.append(grads[i].numpy())
				return results
			else:
				return None
		else:
			return None

	def update(self, neww):
		if neww is not None:
			self.model.set_weights(neww)
			self.started = True

		self.time_count = 0
		self.update_counter += 1
		self.mem.clear()

		if self.is_done:
			self.global_episodes += 1

			self.log.addline('env %d [episodeinfo]: ep_number %d; ep_nstpes: %d; ep_loss %f; ep_entropy %f; ep_values %f; ep_reward_sum %f;\n'
							%(self.id, self.global_episodes, self.ep_steps, self.get_loss(), self.ep_entropy, self.ep_values, self.ep_reward) )

			self.ep_reward = 0.0
			self.ep_score = 0
			self.ep_steps = 0
			self.ep_loss = 0.0
			self.is_done = False
			self.current_state = self.env.reset()

	def finish(self):
		self.log.addline('Client %d finished on time %s'%(self.id, datetime.datetime.now().strftime("%Y%m%d-%H%M%S")))
		self.log.close()

	def done(self):
		return self.global_episodes >= self.max_n_ep

	def compute_loss(self,
				   done,
				   new_state,
				   memory,
				   gamma=0.99):
		if done:
			reward_sum = 0.  # terminal
		else:
			reward_sum = self.model(self.prepare_data(self.current_state))[-1].numpy()[0]

		# Get discounted rewards
		discounted_rewards = []
		for reward in memory.rewards[::-1]:  # reverse buffer r
			reward_sum = reward + gamma * reward_sum
			discounted_rewards.append(reward_sum)
		discounted_rewards.reverse()

		logits, values = self.model(self.prepare_array_data(memory.states))
		# Get our advantages
		advantage = tf.convert_to_tensor(np.array(discounted_rewards)[:, None],
								dtype=tf.float32) - values
		# Value loss
		value_loss = advantage ** 2

		# Calculate our policy loss
		policy = tf.nn.softmax(logits)
		entropy = tf.nn.softmax_cross_entropy_with_logits(labels=policy, logits=logits)

		policy_loss = tf.nn.sparse_softmax_cross_entropy_with_logits(labels=memory.actions,
																	 logits=logits)

		policy_loss *= tf.compat.v2.stop_gradient(advantage)

		self.ep_entropy += np.mean(entropy.numpy())
		self.ep_values += np.mean(values.numpy())
		
		policy_loss -= self.entropy_bonus * entropy
		total_loss = tf.reduce_mean((self.value_loss_coef * value_loss + policy_loss))
		return total_loss


class Runner:
	def __init__(self, id=0, params=None):
		if params is None:
			params = {}
		self.id = id
		self.start(params)
	
	def start(self, params):
		self.sess = tf.compat.v1.Session()
		if 'env_name' in params:
			self.ENV_NAME = params['env_name']
		else:
			self.ENV_NAME = self.id

		self.env = params["env_maker"](self.ENV_NAME)
		self.action_size = params['action_size']
		self.state_size = params['state_size']

		if 'prepare_data' in params:
			self.prepare_data = params['prepare_data']
		else:
			self.prepare_data = __prepare_data__

		if 'prepare_array_data' in params:
			self.prepare_array_data = params['prepare_array_data']
		else:
			self.prepare_array_data = __prepare_array_data__

		if 'act' in params:
			self.act = params['act']
		else:
			self.act = __act__

		if 'initialize_model' in params:
			self.initialize_model = params['initialize_model']
		else:
			self.initialize_model = __initialize_model__

		if 'model' in params:
			self.model = params['model']()
		else:
			self.model = A3CModel(self.state_size, self.action_size)
		
		if 'load_model' in params:
			self.model = tf.keras.models.load_model(params['load_model'])

		self.initialize_model(self.model, self.state_size)
		self.current_state = self.env.reset()

	def get_model_output(self, current_state):
		return self.model(self.prepare_data(current_state))
		
	def get_actions_prob(self, logits):
		return tf.nn.softmax(logits)

	def act(self, current_state):
		logits, values = self.get_model_output(current_state)
		probs = self.get_actions_prob(logits)
		return np.random.choice(self.action_size, p=probs.numpy()[0])

	def run(self, max_ep_len = 1000, rendering=True, delay = 0.017):
		is_done = False
		ep_reward = 0
		steps = 0
		if rendering:
			self.env.render()
		while not is_done and steps < max_ep_len:
			logits, _ = self.model(self.prepare_data(self.current_state))
			probs = tf.nn.softmax(logits)
			action = np.random.choice(self.action_size, p=probs.numpy()[0])
			new_state, reward, is_done, _ = self.act(action, self.env)
			self.current_state = new_state
			ep_reward += reward
			steps += 1
			time.sleep(delay)
			if rendering:
				self.env.render()
		return ep_reward


