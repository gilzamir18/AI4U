from tensorflow.keras import backend as K
import random
from collections import deque
import tensorflow as tf
from tensorflow import keras
from tensorflow.keras import layers
import numpy as np
import time

def huber_loss(y, q_value):
    error = K.abs(y - q_value)
    quadratic_part = K.clip(error, 0.0, 1.0)
    linear_part = error - quadratic_part
    loss = K.mean(0.5 * K.square(quadratic_part) + linear_part)
    return loss


class KerasDQNAgent:
    def __init__(self,  model_builder, action_size = 2, decay_steps = 20000):
        self.model = model_builder()
        self.action_size = action_size
        self.model._make_predict_function()
        self.model._make_test_function()
        self.model._make_train_function()
        self.back_model = model_builder()
        self.back_model._make_predict_function()
        self.back_model._make_test_function()
        self.back_model._make_train_function()
        self.session = tf.keras.backend.get_session()
        self.graph = tf.get_default_graph()
        self.back_model.set_weights(self.model.get_weights())
        self.pmem = deque(maxlen=1000)
        self.nmem = deque(maxlen=1000)
        self.zmem = deque(maxlen=1000)
        self.sync_interval = 1;
        self.eps = 1.0
        self.eps_min = 0.1
        self.eps_decay = (self.eps-self.eps_min)/(20000.0)  
        self.gamma = 0.99
        self.last_loss = 0.0
        self.batch_size = 16
        self.min_batch_size = 32
        self.done = False
        self.delay = 0.1
        self.action_filter = np.ones(action_size).reshape(1, action_size)
        self.training_step = 0
        self.action_list = list(range(action_size))
        self.training = False
        self.hasnewtrainingdata = False;

    def mem_size(self):
        return len(self.pmem) + len(self.nmem) + len(self.zmem)

    def run_update(self):
        if self.mem_size() < self.min_batch_size or not self.training or not self.hasnewtrainingdata:
            return

        samples = []
        if len(self.pmen) > 0:
            samples += random.sample(self.pmem, min(len(self.pmem), self.batch_size))

        if len(self.nmem) > 0:
            samples += random.sample(self.nmem, min(len(self.nmem), self.batch_size))

        if len(self.zmem) > 0:
            samples += random.sample(self.zmem, min(len(self.zmem), self.batch_size))

        random.shuffle(samples)

        inputs = []
        outputs = []
        action_filters = []
        for (initial_state, action, final_state, reward, done) in samples:
            inputs.append(initial_state[0])
            out = np.zeros(self.action_size)
            if done:
                out[action] = reward
            else:
                out[action] = reward + self.gamma * np.amax(self.__predict__(self.back_model, [final_state, self.action_filter]))
            outputs.append(out)
            filter = np.zeros(self.action_size)
            filter[action] = 1
            action_filters.append(filter)
        
        hist = self.__fit__(self.back_model, [np.array(inputs), np.array(action_filters)], np.array(outputs), len(samples))
 
        self.last_loss = np.mean(hist.history['loss'])
  
        if self.training_step % self.sync_interval == 0:
                self.__sync_models__()
       
        self.training_step += 1
        
        if self.delay > 0.0:
            time.sleep(self.delay)
        self.hasnewtrainingdata = False
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
                return np.random.choice(self.action_list)
            else:
                decision = self.__predict__(self.model, [state, self.action_filter])
                return np.argmax(decision)
                
        return np.random.choice(self.action_list)
