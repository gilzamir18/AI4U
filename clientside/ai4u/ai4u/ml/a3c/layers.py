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


class NTMAddressing:
    def build(self, input_layer, ntm_layer, idx):
        pass

class NTMContentAddressing(NTMAddressing):
    def __init__(self, name="NTMCAddr"):
        super(NTMContentAddressing, self).__init__()
        self.name = name
        self.key_activation = None
        self.strength_activation="sigmoid"

    def build(self, input_layer, ntm_layer, idx=0):
        self.layers = []
        exp_features = input_layer
        self.key = tf.keras.layers.Dense(ntm_layer.columns, activation=self.key_activation, name=self.name)(exp_features)
        self.strength = tf.keras.layers.Dense(1, activation=self.strength_activation, name=self.name+"_strength")(exp_features)
        self.layers.append(self.key)
        self.layers.append(self.strength)
        key, _ = tf.linalg.normalize(self.key, axis=1)
        key = tf.reshape(key, [-1, ntm_layer.columns, 1]) #(-1, M, 1)
        mem, _ = tf.linalg.normalize(ntm_layer.memory, axis=[-2, -1]) #(-1, N, M)
        K = tf.matmul(mem, key)
        K = tf.reshape(K, (-1, ntm_layer.lines))
        self.w = tf.keras.layers.Softmax(name='%s_output'%(self.name))(K*self.strength)
        self.layers.append(self.w)
        return self.w


class NTMWriteHead:
    def __init__(self, name="NTMWRITEH", addressing = None):
        self.name=name
        self.eraser_activation = "sigmoid"
        self.adder_activation = "sigmoid"
        if addressing is None:
            self.addressing = NTMContentAddressing(name="%s_ADDRESSING"%(self.name))
        else:
            self.addressing = addressing

    def build(self, inputs, ntm_layer, idx=0):
        self.layers = []
        self.addressing.name = "%s_%d"%(self.addressing.name, idx)
        self.w = self.addressing.build(inputs, ntm_layer, idx)
        self.e = tf.keras.layers.Dense(ntm_layer.columns, activation=self.eraser_activation, name="%s_eraser_%d"%(self.name, idx))(inputs)
        self.a = tf.keras.layers.Dense(ntm_layer.columns, activation=self.adder_activation, name="%s_adder_%d"%(self.name, idx))(inputs)
        self.layers.append(self.e)
        self.layers.append(self.a)
        e = tf.expand_dims(self.e, axis=1) #(None, M) => (None, 1, M)
        w = tf.expand_dims(self.w, axis=-1) #(None, N) => (None, N, 1)
        # w x e => (None, 12, 1) x (None, 1, 10)
        We = tf.matmul(w, e)
        Me = tf.keras.layers.Multiply()([ntm_layer.memory, We])
        Me = tf.keras.layers.Subtract()([ntm_layer.memory, Me])
        a = tf.expand_dims(self.a, axis=1)
        Wa = tf.matmul(w, a)
        self.write_head = tf.keras.layers.Add(name='%s_write_head_%d'%(self.name, idx))([Me, Wa])
        self.layers.append(self.write_head)
        self.layers += self.addressing.layers
        return self.write_head


class NTMReadHead:
    def __init__(self, name="NTMREADH", addressing = None):
        self.name=name
        if addressing is None:
            self.addressing = NTMContentAddressing(name="%s_ADDRESSING"%(self.name))
        else:
            self.addressing = addressing

    def build(self, inputs, ntm_layer, idx=0):
        mem = ntm_layer.memory
        self.layers = []
        self.addressing.name = "%s_%d"%(self.addressing.name, idx)
        self.w = self.addressing.build(inputs, ntm_layer, idx)
        w = tf.expand_dims(self.w, axis=-1)
        #memory is (None, N, M) and w is (None, M, 1)
        prod = tf.multiply(mem, w)
        self.read_head = tf.math.reduce_sum(prod, 1)
        self.layers.append(self.read_head)
        self.layers += self.addressing.layers
        return self.read_head #shape = (None, N, 1)

class NTMLayer(tf.keras.layers.Layer):
  def __init__(self, lines=12, columns=10, writers=None, readers=None, name="NTMLayer"):
    super(NTMLayer, self).__init__(name=name)

    if writers is None:
        self.writers = [NTMWriteHead()]
    else:
        self.writers = writers
    if readers is None:
        self.readers = [NTMReadHead()]
    else:
        self.readers = readers

    self.numwriters = len(self.writers)
    self.numreaders = len(self.readers)

    self.memory = tf.placeholder(tf.float32, (None, lines, columns))
    self.reading = tf.placeholder(tf.float32, (None, self.numreaders, columns))
    self.lines = lines
    self.columns = columns
    self.layers = None
    self.epsilon = 1.0e-12

  def build(self, input_shape):
    pass

  def call(self, inputs, shared_layers=None):
    last_reading = tf.keras.layers.Flatten()(self.reading)
    features = tf.keras.layers.Concatenate()([inputs, last_reading])
    self.layers = []

    if shared_layers is None:
        hidden1 = tf.keras.layers.Dense(100, activation="tanh", name='%s_hidden1'%(self.name))(features)
        ctr_hidden = tf.keras.layers.Dense(100, activation="tanh", name='%s_ctr_hidden'%(self.name))(hidden1)
        self.layers.append(hidden1)
        self.layers.append(ctr_hidden)
    else:
        for l in shared_layers:
            features = l(features)
            self.layers.append(features)

    self.read_out = []
    self.write_out = []

    for idx, reader in enumerate(self.readers):
        self.read_out.append(reader.build(features, self, idx))
        self.layers += reader.layers
    for idx, writer in enumerate(self.writers):
        self.write_out.append(writer.build(features, self, idx))
        self.layers += writer.layers
    if len(self.read_out) > 1:
        return tf.keras.layers.Concatenate()(self.read_out)
    else:
        return self.read_out[0]

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

