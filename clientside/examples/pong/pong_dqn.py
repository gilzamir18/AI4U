from unityremote.core import RemoteEnv, get_image
from unityremote.ml.dqn import KerasDQNAgent, huber_loss
from PIL import Image
import io
import time
import tensorflow as tf
from tensorflow import keras
from tensorflow.keras import layers
import numpy as np
from collections import deque
import random
from threading import Thread
from tensorflow.keras import backend as K


def build_model():
    input1 = layers.Input(shape=(84, 84, 4))
    input2 = layers.Input(shape=(2,))
    normalized = layers.Lambda(lambda x: x / 255.0, name='normalization')(input1)
    conv1 = layers.Conv2D(16, (3, 3), strides=(1, 1), activation='relu')(normalized)
    conv2 = layers.Conv2D(32, (4, 4), strides=(2, 2), activation='relu')(conv1)
    flatten = layers.Flatten()(conv2)
    dense1 = layers.Dense(256, activation='relu')(flatten)
    output = layers.Dense(2)(dense1)
    filtered_output = layers.Multiply()([output, input2])
    model = keras.Model(inputs=[input1, input2], outputs=filtered_output)
    model.compile(optimizer=tf.keras.optimizers.RMSprop(learning_rate=0.00025, rho=0.95, epsilon=0.01), loss=huber_loss)
    return model


def run_worker(worker):
    while(True):
        if worker.run_update():
            break

actions = {
	'w':("Up"),
	's':("Down"),
	'p':("PauseGame"),
	'c':("ResumeGame"),
	'r':("RestartGame")
}

def Up(event):
	global env
	return env.step(actions['w'])


def Down(event):
	global env
	return env.step(actions['s'])

def Pause(event):
	global env
	return env.step(actions['p'])

def Resume(event):
	global env
	return env.step(actions['c'])


def Restart(event):
	global env
	return env.step(actions['r'])

def apply(action):
    if action == 0:
        return Up(None)
    elif action == 1:
        return Down(None)

if __name__=="__main__":
    agent = KerasDQNAgent(build_model)
    t = Thread(target=run_worker, args=(agent,))
    t.start()
    env = RemoteEnv()
    env.open(0)
    fields = None
    total_steps = 0
    for e in range(100000000):
        print("Starting new episode ", e)
        state = Restart(None)
        seq = deque(maxlen=4)
        frame = get_image(state['frame'])
        done = state['done']
        reward = state['reward']
        for _ in range(4):
            seq.append(frame)
        initial_state = np.array(seq).reshape(1, 84, 84, 4)
        steps = 0
        sum_rewards = 0
        while not done:
            if total_steps > 5000:
                agent.training = True
                action = agent.predict(initial_state)
            else:
                action = agent.predict(initial_state, True)

            reward = 0.0
            for f in range(4):
                fields = apply(action)
                reward += fields['reward']
                if fields['done']:
                    break
            reward = np.clip(reward, -1, 1)
            sum_rewards += reward
            frame = get_image(fields['frame'])
            seq.append(frame)
            final_state = np.array(seq).reshape(1, 84, 84, 4)
            done = fields['done']
            if reward != 0:
                agent.add_sample(initial_state, action, final_state, reward, done)
            if not done:
                initial_state = np.copy(final_state)
            steps += 1
            total_steps += 1
        print("Sum of rewards ", sum_rewards, ", Steps by episode: ", steps, ", EPS: ", agent.eps, ", TOTAL SAMPLES: ", agent.mem_size())
        if e % 50 == 0:
            agent.model.save_weights("weights.rnn")
    agent.done = True
    agent.training = False
    env.close()
