from unitynet import NetCon, get_state, get_image
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

STARTING_TRAINING = 0
SEND_SAMPLE = 1
GET_ACTION = 2

def huber_loss(y, q_value):
    error = K.abs(y - q_value)
    quadratic_part = K.clip(error, 0.0, 1.0)
    linear_part = error - quadratic_part
    loss = K.mean(0.5 * K.square(quadratic_part) + linear_part)
    return loss

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

class Worker:
    def __init__(self):
        self.model =  build_model()
        self.model._make_predict_function()
        self.model._make_test_function()
        self.model._make_train_function()
        self.back_model = build_model()
        self.back_model._make_predict_function()
        self.back_model._make_test_function()
        self.back_model._make_train_function()
        self.session = tf.keras.backend.get_session()
        self.graph = tf.get_default_graph()
        self.back_model.set_weights(self.model.get_weights())
        self.pmem = deque(maxlen=500)
        self.nmem = deque(maxlen=500)
        self.zmem = deque(maxlen=500)
        self.eps = 1.0
        self.eps_min = 0.1
        self.eps_decay = (self.eps-self.eps_min)/(20000.0)  
        self.gamma = 0.99
        self.last_loss = 0.0
        self.batch_size = 16
        self.min_batch_size = 32
        self.done = False
        self.delay = 0.1
        self.action_filter = np.ones(2).reshape(1, 2)
        self.training_step = 0
        self.training = False

    def mem_size(self):
        return len(self.pmem) + len(self.nmem) + len(self.zmem)

    def run_update(self):
        if self.mem_size() < self.min_batch_size or not agent.training:
            return

        samples = []
        samples += random.sample(self.pmem, min(len(self.pmem), self.batch_size))
        samples += random.sample(self.nmem, min(len(self.nmem), self.batch_size))
        samples += random.sample(self.zmem, min(len(self.zmem), self.batch_size))
        random.shuffle(samples)
        inputs = []
        outputs = []
        action_filters = []
        for (initial_state, action, final_state, reward, done) in samples:
            inputs.append(initial_state[0])
            out = np.zeros(2)
            if done:
                out[action] = reward
            else:
                out[action] = reward + self.gamma * np.amax(self.__predict__(self.back_model, [final_state, self.action_filter]))
            outputs.append(out)
            filter = np.zeros(2)
            filter[action] = 1
            action_filters.append(filter)
        
        hist = self.__fit__(self.back_model, [np.array(inputs), np.array(action_filters)], np.array(outputs), len(samples))
 
        self.last_loss = np.mean(hist.history['loss'])
        
        self.__sync_models__()
        self.training_step += 1
        
        if self.delay > 0.0:
            time.sleep(self.delay)

        return self.done

    def add_sample(self, initial_state, action, final_state, reward, done):
        if reward > 0:
            self.pmem.append((initial_state, action, final_state, reward, done))
        elif reward < 0:
            self.nmem.append((initial_state, action, final_state, reward, done))
        else:
            self.zmem.append((initial_state, action, final_state, reward, done))


    def __sync_models__(self):
        with self.session.as_default():
            with self.graph.as_default():
                self.model.set_weights(self.back_model.get_weights())

    def __predict__(self, model, input):
        with self.session.as_default():
            with self.graph.as_default():
                return model.predict(input)[0]

    def __fit__(self, model, inputs, outputs, bsize):
        with self.session.as_default():
            with self.graph.as_default():
                return model.fit(inputs, outputs, epochs=1, batch_size=bsize, verbose=0)

    def predict(self, state, randomic=False):
        if not randomic:                
            if self.eps < self.eps_min:
                self.eps = self.eps_min
            else:
                self.eps -= self.eps_decay
            if np.random.random() < self.eps:
                return np.random.choice([0, 1])
            else:
                decision = self.__predict__(self.model, [state, self.action_filter])
                return np.argmax(decision)
                
        return np.random.choice([0, 1])

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
	global netcon
	return netcon.send(actions['w'])


def Down(event):
	global netcon
	return netcon.send(actions['s'])

def Pause(event):
	global netcon
	return netcon.send(actions['p'])

def Resume(event):
	global netcon
	return netcon.send(actions['c'])


def Restart(event):
	global netcon
	return netcon.send(actions['r'])

def apply(action):
    if action == 0:
        return Up(None)
    elif action == 1:
        return Down(None)

if __name__=="__main__":
    agent = Worker()
    t = Thread(target=run_worker, args=(agent,))
    t.start()
    netcon = NetCon()
    netcon.open(0)
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
        agent.model.save_weights("weights.rnn")
    agent.done = True
    agent.training = False
    netcon.close()
