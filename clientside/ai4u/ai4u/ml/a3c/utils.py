import queue
import subprocess
import time
from multiprocessing import Queue
from threading import Thread

import numpy as np
import tensorflow
if tensorflow.__version__ >= "2":
    import tensorflow.compat.v1 as tf
    tf.disable_v2_behavior()
else:
    import tensorflow as tf

class LSTMNet:
    def __init__(self, units=256, num_layers=1):
        self.units = units
        k  = num_layers
        self.state_h = tf.placeholder(tf.float32, (None, k, units) )
        self.state_c = tf.placeholder(tf.float32, (None, k, units) )
        self.num_layers = num_layers
        self.outputs = None
        self.shapes = None
        self.size = 0
        self.layers = []

    def buildlayers(self, features):
        self.outputs = []
        self.shapes = []
        layers = []
        r = self.num_layers
        hidden = None
        if r == 1:
            rnn1_layers = tf.keras.layers.LSTM(self.units, return_sequences=False, return_state=True, name="rnn1")
            hidden, stateh1, statec1 = rnn1_layers(features, initial_state=[self.state_h[:,0,:], self.state_c[:,0,:]])
            layers.append(hidden)
            self.outputs.append(stateh1)
            self.outputs.append(statec1)
            self.shapes.append( (1, self.units) )
            self.size += 1
        else:
            for i in range(r):
                if i < (r-1):
                    rnn_layers = tf.keras.layers.LSTM(self.units, return_sequences=True, return_state=True, name="rnn%d"%(i))
                    features, stateh1, statec1 = rnn_layers(features, initial_state=[self.state_h[:,i,:], self.state_c[:,i,:]])
                    self.outputs.append(stateh1)
                    self.outputs.append(statec1)
                    self.shapes.append( (1, self.units) )
                    self.size += 1
                    layers.append(features)
                else:
                    hidden, stateh1, statec1 = tf.keras.layers.LSTM(self.units, return_state=True, return_sequences=False, name="rnn%d"%(i))(features, initial_state=[self.state_h[:,i,:], self.state_c[:,i,:]])
                    self.outputs.append(stateh1)
                    self.outputs.append(statec1)
                    self.shapes.append( (1, self.units) )
                    self.size += 1
                    layers.append(hidden)
        return hidden, layers


def rewards_to_discounted_returns(rewards, discount_factor):
    returns = np.zeros_like(rewards, dtype=np.float32)
    returns[-1] = rewards[-1]
    for i in range(len(rewards) - 2, -1, -1):
        returns[i] = rewards[i] + discount_factor * returns[i + 1]
    return returns


def get_git_rev():
    try:
        cmd = 'git rev-parse --short HEAD'
        git_rev = subprocess.check_output(cmd.split(' '), stderr=subprocess.PIPE).decode().rstrip()
        return git_rev
    except:
        return 'unkrev'


class MemoryProfiler:
    STOP_CMD = 0

    def __init__(self, pid, log_path):
        self.pid = pid
        self.log_path = log_path
        self.cmd_queue = Queue()
        self.t = None

    def start(self):
        self.t = Thread(target=self.profile)
        self.t.start()

    def stop(self):
        self.cmd_queue.put(self.STOP_CMD)
        self.t.join()

    def profile(self):
        import memory_profiler
        f = open(self.log_path, 'w+')
        while True:
            # 5 samples, 1 second apart
            memory_profiler.memory_usage(self.pid, stream=f, timeout=5, interval=1,
                                         include_children=True)
            f.flush()

            try:
                cmd = self.cmd_queue.get(timeout=0.1)
                if cmd == self.STOP_CMD:
                    f.close()
                    break
            except queue.Empty:
                pass


class Timer:
    """
    A simple timer class.
    * Set the timer duration with the `duration_seconds` argument to the constructor.
    * Start the timer by calling `reset()`.
    * Check whether the timer is done by calling `done()`.
    """

    def __init__(self, duration_seconds):
        self.duration_seconds = duration_seconds
        self.start_time = None

    def reset(self):
        self.start_time = time.time()

    def done(self):
        cur_time = time.time()
        if cur_time - self.start_time > self.duration_seconds:
            return True
        else:
            return False


class TensorFlowCounter:
    """
    Counter implemented as a TensorFlow variable in the provided session's graph.
    Useful if you want the value to feed into some other operation, e.g. learning rate calculation.
    """

    def __init__(self, sess):
        self.sess = sess
        self.value = tf.Variable(0, trainable=False)
        self.increment_by = tf.placeholder(tf.int32)
        self.increment_op = self.value.assign_add(self.increment_by)

    def __int__(self):
        return int(self.sess.run(self.value))

    def increment(self, n=1):
        self.sess.run(self.increment_op, feed_dict={self.increment_by: n})


class RateMeasure:
    def __init__(self):
        self.prev_t = self.prev_value = None

    def reset(self, val):
        self.prev_value = val
        self.prev_t = time.time()

    def measure(self, val):
        val_change = val - self.prev_value
        cur_t = time.time()
        interval = cur_t - self.prev_t
        rate = val_change / interval

        self.prev_t = cur_t
        self.prev_value = val

        return rate
