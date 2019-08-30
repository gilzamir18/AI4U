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
    input1 = layers.Input(shape=(7, ))
    input2 = layers.Input(shape=(4,))
    normalized = layers.Lambda(lambda x: x / 10.0, name='normalization')(input1)
    dense1 = layers.Dense(32, activation='relu')(normalized)
    dense2 = layers.Dense(32, activation='relu')(dense1)
    output = layers.Dense(4)(dense1)
    filtered_output = layers.Multiply()([output, input2])
    model = keras.Model(inputs=[input1, input2], outputs=filtered_output)
    model.compile(optimizer=tf.keras.optimizers.RMSprop(learning_rate=0.0025, rho=0.95, epsilon=0.01), loss=huber_loss)
    return model

def get_state_from_fields(fields):
    return np.array([fields['tx'], fields['tz'], fields['vx'], fields['vz'], fields['x'], fields['y'],  fields['z']])

def run_worker(worker):
    while(True):
        if worker.run_update():
            break

if __name__ == "__main__":
    actions = [("fx", 0.1), ("fx", -0.1) , ("fz", 0.1), ("fz", -0.1)]
    agent = KerasDQNAgent(build_model,  action_size=4, decay_steps=20000)
    agent.eps_min = 0.001;
    t = Thread(target=run_worker, args=(agent,))
    t.start()
    env = RemoteEnv()
    env.open(0)
    fields = None
    total_steps = 0
    for e in range(10000):
        print("Starting new episode ", e)
        fields = env.step("restart")
        done = fields['done']
        reward = 0
        initial_state =  get_state_from_fields(fields).reshape(1, 7)
        steps = 0
        sum_rewards = 0
        while not done:
            if total_steps >= 10000:
                agent.training = True
                action = agent.predict(initial_state)
            else:
                action = agent.predict(initial_state, True)

            reward = 0.0
            
            for _ in range(8):
                fields = env.step(actions[action][0], actions[action][1])
                reward += fields['reward']
                if fields['done']:
                    break

            final_state = get_state_from_fields(fields).reshape(1, 7)

            reward = np.clip(reward, -1.0, 1.0)
            sum_rewards += reward
            done = fields['done']
            if total_steps % 20 == 0:
                print("\nLOSS ", agent.last_loss, "\n")
            
            agent.add_sample(initial_state, action, final_state, reward, done)
            agent.hasnewtrainingdata = True
            
            if steps >= 2000:
                done = True
            if not done:
                initial_state = np.copy(final_state)
           
            steps += 1
            total_steps += 1
        
        print("Sum of rewards ", sum_rewards, ", Steps by episode: ", steps, ", Total Steps: ", total_steps , ", EPS: ", agent.eps, ", TOTAL SAMPLES: ", agent.mem_size())
        if e % 50 == 0:
            agent.model.save_weights("model.h5", save_format='h5')
    
    agent.done = True
    agent.training = False
    env.close()
