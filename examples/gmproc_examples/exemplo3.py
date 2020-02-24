import os
os.environ["CUDA_VISIBLE_DEVICES"] = ""

import threading
import multiprocessing
import numpy as np
from queue import Queue
import argparse
import matplotlib.pyplot as plt
from tensorflow import keras
from tensorflow.keras import layers
from unityremote.gmproc import ClientServer, ClientWorker, ServerWorker
import tensorflow as tf
import time
import gym

tf.compat.v1.enable_eager_execution()

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

class MyModel(keras.Model):
	def __init__(self, state_size, action_size):
		super(MyModel, self).__init__()
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

class MyServer(ServerWorker):
	def __init__(self):
		super().__init__()
		ENV_NAME = "CartPole-v0"
		env = gym.make(ENV_NAME)
		self.state_size = env.observation_space.shape[0]
		self.action_size = env.action_space.n
		self.id = "sharednetwork"
		self.model = MyModel(self.state_size, self.action_size)
		self.model(tf.convert_to_tensor(np.random.random((1, self.state_size)), dtype=tf.float32))
		self.opt = tf.keras.optimizers.Adam(lr=0.001)

	def start(self, params):
		print('starting server %s'%(self.id))

	def process(self, id, msg):
		if msg is not None:
			grads = [None]*len(msg)
			for i in range(len(msg)):
				grads[i] = tf.convert_to_tensor(msg[i], dtype=tf.float32)
			self.opt.apply_gradients(zip(grads, self.model.trainable_weights))
		return self.model.get_weights()

class Client(ClientWorker):
	def __init__(self, id):
		super().__init__()
		self.ENV_NAME = "CartPole-v0"
		self.UPDATE_FREQ = 30
		self.env = gym.make(self.ENV_NAME)
		self.id = id
		self.state_size = self.env.observation_space.shape[0]
		self.action_size = self.env.action_space.n
		self.model = MyModel(self.state_size, self.action_size)
		self.model(tf.convert_to_tensor(np.random.random((1, self.state_size)), dtype=tf.float32))
		self.opt = tf.keras.optimizers.Adam(lr=0.001)
		self.started = False
		self.sess = tf.compat.v1.Session()
		self.total_step = 1
		self.time_counter = 0
		self.ep_steps = 0
		self.ep_loss = 0
		self.is_done = False
		self.global_episodes = 0
		self.mem = Memory()
		self.ep_reward = 0.0
		self.avg_reward = 0.0
		self.update_counter = 0
		self.avg_counter = 0
		self.avg_score = 0
		self.ep_score = 0

	def start(self, params):
		print("starting client %s"%(self.id))

	def print_loss(self):
		loss = self.ep_loss.numpy()
		avg = np.mean(loss)/self.ep_steps
		print("Loss: ", avg)

	def process(self):
		if self.started:
			while self.time_count < self.UPDATE_FREQ and not self.is_done:
				logits, _ = self.model(tf.convert_to_tensor(self.current_state[None, :], dtype=tf.float32))

				probs = tf.nn.softmax(logits)

				action = np.random.choice(self.action_size, p=probs.numpy()[0])

				new_state, reward, is_done, _ = self.env.step(action)
				self.is_done = is_done
				if is_done:
					reward = -1
				else:
					self.ep_score += 1
				
				self.ep_reward += reward
				
				self.mem.store(self.current_state, action, reward)
				self.total_step += 1
				self.ep_steps += 1
				self.current_state = new_state
				self.time_count += 1
				
				# Calculate gradient wrt to local model. We do so by tracking the
				# variables involved in computing the loss by using tf.GradientTape mem.store(current_state, action, reward)
			if self.mem.size() > 0:
				with tf.GradientTape() as tape:
					total_loss = self.compute_loss(is_done, new_state, self.mem, 0.99)
				self.is_done = is_done
				self.ep_loss += total_loss
				# Calculate local gradients

				grads = tape.gradient(total_loss, self.model.trainable_weights)
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
			self.current_state = self.env.reset()
		self.time_count = 0
		self.update_counter += 1
		self.mem.clear()
		if self.update_counter %  100 == 0:
			print("%d: "%(self.update_counter), end='')
			self.print_loss()
			if self.avg_counter > 0:
				print("AVG REWARD SUM, SCORE: %f, %f"%(self.avg_reward/self.avg_counter, self.avg_score/self.avg_counter))
			self.avg_reward = 0.0
			self.avg_score = 0
			self.avg_counter = 0

		if self.is_done:
			self.avg_counter += 1
			self.avg_reward += self.ep_reward
			self.avg_score += self.ep_score
			self.global_episodes += 1
			self.ep_reward = 0.0
			self.ep_score = 0
			self.ep_steps = 0
			self.ep_loss = 0.0
			self.is_done = False
			self.current_state = self.env.reset()

	def finish(self):
		print("Training Finished!", end='')

	def done(self):
		return self.global_episodes > 1000000

	def compute_loss(self,
				   done,
				   new_state,
				   memory,
				   gamma=0.99):
		if done:
			reward_sum = 0.  # terminal
		else:
			reward_sum = self.model(
				tf.convert_to_tensor(new_state[None, :],
					dtype=tf.float32))[-1].numpy()[0]

		# Get discounted rewards
		discounted_rewards = []
		for reward in memory.rewards[::-1]:  # reverse buffer r
			reward_sum = reward + gamma * reward_sum
			discounted_rewards.append(reward_sum)
		discounted_rewards.reverse()

		logits, values = self.model(
			tf.convert_to_tensor(np.vstack(memory.states),
								 dtype=tf.float32))
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
		
		policy_loss -= 0.01 * entropy
		total_loss = tf.reduce_mean((0.5 * value_loss + policy_loss))
		return total_loss


if __name__=="__main__":
	cs = ClientServer(MyServer)
	cs.new_workers(4, Client)
	cs.run()
