import queue
import subprocess
import time
from multiprocessing import Queue
from threading import Thread
import numpy as np
import tensorflow
import sys
from warnings import warn
import functools

if tensorflow.__version__ >= "2":
    import tensorflow.compat.v1 as tf
    tf.disable_v2_behavior()
else:
    import tensorflow as tf

########################################################################################
# This class  implements Neural Turing Machine (A3C version) integrated with our  A3C  #
# implementation.                                                                      #
# Features: content adressing, one write head, one read read, numeric stability.       #
# Use this class in callback function make_inference_network.                          #
#                                                                                      #
# For example: see examples/ntmonsimplestenv.py.
########################################################################################
class NTMNet:
    def __init__(self, lines=12, columns=10, eps=1.0e-12):
        self.memory = tf.placeholder(tf.float32, (None, lines, columns))
        self.reading = tf.placeholder(tf.float32, (None, columns))
        self.lines = lines
        self.columns = columns
        self.layers = []
        self.activation = "sigmoid"
        self.epsilon = eps

    def buildWeightings(self, input_layer, name="ntm_key"):
        self.layers = []
        #flattened = tf.keras.layers.Flatten()(self.memory)
        #exp_features = tf.keras.layers.Concatenate()([input_layer, flattened])
        exp_features = input_layer
        self.key = tf.keras.layers.Dense(self.columns, activation=None, name=name)(exp_features)
        self.strength = tf.keras.layers.Dense(1, activation=self.activation, name=name+"_strength")(exp_features)
        self.layers.append(self.key)
        self.layers.append(self.strength)
        key, _ = tf.linalg.normalize(self.key, axis=1)
        key = tf.reshape(key, [-1, self.columns, 1]) #(-1, M, 1)
        mem, _ = tf.linalg.normalize(self.memory, axis=[-2, -1]) #(-1, N, M)
        K = tf.matmul(mem, key)
        K = tf.reshape(K, (-1, self.lines))
        #print_op = tf.print("key: ", self.key, output_stream=sys.stdout)
        #print_op2 = tf.print("K: ", K, output_stream=sys.stdout)
        #print_op3 = tf.print("M:", self.memory, output_stream=sys.stdout)
        #with tf.control_dependencies([print_op, print_op2, print_op3]):
            #self.w = tf.keras.layers.Softmax(name='ntm_w')(K*self.strength)
        self.w = tf.keras.layers.Softmax(name='%s_output'%(name))(K*self.strength)
        self.layers.append(self.w)
        return self.w

    def buildWriteHead(self, input_layer, id=0, w=None):
        if w is None:
            w = self.w
        self.e = tf.keras.layers.Dense(self.columns, activation='sigmoid', name="ntm_ewrite_%d"%(id))(input_layer)
        self.a = tf.keras.layers.Dense(self.columns, activation='sigmoid', name="ntm_awrite_%d"%(id))(input_layer)
        self.layers.append(self.e)
        self.layers.append(self.a)
        e = tf.expand_dims(self.e, axis=1) #(None, M) => (None, 1, M)
        w = tf.expand_dims(w, axis=-1) #(None, N) => (None, N, 1)
        # w x e => (None, 12, 1) x (None, 1, 10)
        We = tf.matmul(w, e)
        Me = tf.keras.layers.Multiply()([self.memory, We])
        Me = tf.keras.layers.Subtract()([self.memory, Me])
        a = tf.expand_dims(self.a, axis=1)
        Wa = tf.matmul(w, a)
        self.write_head = tf.keras.layers.Add(name='ntm_write_head%d'%(id))([Me, Wa])
        self.layers.append(self.write_head)
        return self.write_head

    def buildReadHead(self, mem=None, w = None, id=0):
        if w is None:
            w = self.w
        if mem is None:
            mem = self.memory
        w = tf.expand_dims(w, axis=-1)
        #memory is (None, N, M)
        prod = tf.multiply(mem, w)
        self.read_head = tf.math.reduce_sum(prod, 1)
        #print_op1 = tf.print("mat = ", prod)
        #print_op2 = tf.print("RS = ", self.read_head)
        #with tf.control_dependencies([print_op1, print_op2]):
        #self.read_head = tf.multiply(self.read_head, 1)
        self.layers.append(self.read_head)
        return self.read_head


########################################################################################
# This class  wraps tensorflow.keras.layers.LSTM for facilitating integration of LSTM  #
# with our A3C implementation.                                                         #
# Features: multiple layers support and easy interface.                                #
# Use this class in callback function make_inference_network.                          #
#                                                                                      #
# For example: see lstmnetonsimplestenv.py.                                            #
########################################################################################
class LSTMNet:
    def __init__(self, units=256, num_layers=1, return_state=False, initial_value=0.01):
        self.units = units
        k  = num_layers
        self.state_h = tf.placeholder(tf.float32, (None, k, units) )
        self.state_c = tf.placeholder(tf.float32, (None, k, units) )
        self.num_layers = num_layers
        self.outputs = None
        self.shapes = None
        self.return_state = return_state
        self.size = 0
        self.layers = []
        self.initial_value = initial_value

    def __call__(self, features):
        return buildlayers(self, features)

    def buildlayers(self, features, **kwargs):
        self.outputs = []
        self.shapes = []
        layers = []
        r = self.num_layers
        hidden = None
        stateh1 = None
        statec1 = None
        if r == 1:
            rnn1_layers = tf.keras.layers.RNN(tf.keras.layers.LSTMCell(self.units, name="cell1", **kwargs), return_sequences=False, return_state=True, name="rnn1")
            hidden, stateh1, statec1 = rnn1_layers(features, initial_state=[self.state_h[:,0,:], self.state_c[:,0,:]])
            layers.append(hidden)
            self.outputs.append(stateh1)
            self.outputs.append(statec1)
            self.shapes.append( (1, self.units) )
            self.size += 1
        else:
            for i in range(r):
                if i < (r-1):
                    rnn_layers = tf.keras.layers.RNN(tf.keras.layers.LSTMCell(self.units, name="cell%d"%(i)), return_sequences=True, return_state=True, name="rnn%d"%(i), **kwargs)
                    features, stateh1, statec1 = rnn_layers(features, initial_state=[self.state_h[:,i,:], self.state_c[:,i,:]])
                    self.outputs.append(stateh1)
                    self.outputs.append(statec1)
                    self.shapes.append( (1, self.units) )
                    self.size += 1
                    layers.append(features)
                else:
                    hidden, stateh1, statec1 = tf.keras.layers.RNN(tf.keras.layers.LSTMCell(self.units, name="cell%d"%(i)), return_state=True, return_sequences=False, name="rnn%d"%(i), **kwargs)(features, initial_state=[self.state_h[:,i,:], self.state_c[:,i,:]])
                    self.outputs.append(stateh1)
                    self.outputs.append(statec1)
                    self.shapes.append( (1, self.units) )
                    self.size += 1
                    layers.append(hidden)
        if self.return_state:
            return hidden, layers, stateh1, statec1
        else:
            return hidden, layers

# This class  wraps tensorflow.keras.layers.LSTMCell for facilitating integration of LSTM
# with our A3C implementation. It is deprecated, use LSTMNet instead it!
class LSTMCell:
    def __init__(self, units=256, num_layers=1, return_state=False, initial_value = 0.0):
        warn("This class is deprecated! Use LSTMNet instead it!")
        self.units = units
        k  = num_layers
        self.state_h = tf.placeholder(tf.float32, (None, k, units) )
        self.state_c = tf.placeholder(tf.float32, (None, k, units) )
        self.num_layers = num_layers
        self.outputs = None
        self.shapes = None
        self.return_state = return_state
        self.size = 0
        self.initial_value = initial_value
        self.layers = []

    def buildlayers(self, features, activation='tanh', recurrent_activation='tanh'):
        self.outputs = []
        self.shapes = []
        layers = []
        r = self.num_layers
        hidden = features
        stateh1 = None
        statec1 = None
        if r == 1:
            hidden, (stateh1, statec1) = tf.keras.layers.LSTMCell(self.units, recurrent_activation=recurrent_activation, activation=activation, name="cell1")(hidden, states=[self.state_h[:,0,:], self.state_c[:,0,:]])
            layers.append(hidden)
            self.outputs.append(stateh1)
            self.outputs.append(statec1)
            self.shapes.append( (1, self.units) )
            self.size += 1
        else:
            for i in range(r):
                if i < (r-1):
                    hidden, (stateh1, statec1) = tf.keras.layers.LSTMCell(self.units, recurrent_activation=recurrent_activation, activation=activation,  name="cell%d"%(i))(hidden, states=[self.state_h[:,i,:], self.state_c[:,i,:]])
                    self.outputs.append(stateh1)
                    self.outputs.append(statec1)
                    self.shapes.append( (1, self.units) )
                    self.size += 1
                else:
                    hidden, (stateh1, statec1) = tf.keras.layers.LSTMCell(self.units, recurrent_activation=recurrent_activation, activation=activation, name="cell%d"%(i))(hidden, states=[self.state_h[:,i,:], self.state_c[:,i,:]])
                    self.outputs.append(stateh1)
                    self.outputs.append(statec1)
                    self.shapes.append( (1, self.units) )
                    self.size += 1
                layers.append(hidden)
        if self.return_state:
            return hidden, layers, stateh1, statec1
        else:
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



