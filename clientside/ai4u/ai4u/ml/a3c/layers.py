import numpy as np
import tensorflow
import sys
from warnings import warn
import functools
from .network import MemorySlot

if tensorflow.__version__ >= "2":
    import tensorflow.compat.v1 as tf
    tf.disable_v2_behavior()
else:
    import tensorflow as tf


class NTMAddressing:
    counter = 0
    counter_build = 0
    def __init__(self, name):
        self.name = name
        NTMAddressing.counter += 1

    def build(self, input_layer, ntm_layer, idx):
        NTMAddressing.counter_build += 1

class NTMContentAddressing(NTMAddressing):
    def __init__(self, name="NTMCAddr"):
        super(NTMContentAddressing, self).__init__(name)
        self.key_activation = None
        self.strength_activation="sigmoid"


    def build(self, input_layer, ntm_layer, idx=0):
        super().build(input_layer, ntm_layer, idx)
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


class NTMLocationInterAddressing(NTMContentAddressing):
    def __init__(self, name="NTMLAddr"):
        super(NTMLocationInterAddressing, self).__init__(name)
        self.gate_activation="sigmoid"

    def build(self, input_layer, ntm_layer, idx=0):
        w = super().build(input_layer, ntm_layer, idx)
        self.pw_slot = MemorySlot("%s_pw_%d"%(self.name, NTMAddressing.counter_build), (ntm_layer.lines, ), epsilon=ntm_layer.epsilon,  singleslot=True)
        self.pw = self.pw_slot.input
        exp_features = input_layer
        self.gate = tf.keras.layers.Dense(1, activation=self.gate_activation, name=self.name+"_gate")(exp_features)
        self.w = self.gate * w + (1 - self.gate) * self.pw
        self.layers.append(self.gate)
        if ntm_layer.network is not None:
            ntm_layer.network.AddMemorySlot(self.pw_slot)
        return self.w

class NTMLocationAddressing(NTMLocationInterAddressing):
    def __init__(self, name="NTMLAddr", shift_size=3):
        assert shift_size > 0, "Error in %s!"
        if shift_size % 2 == 0:
            shift_size += 1
        super(NTMLocationAddressing, self).__init__(name)
        self.shift_activation = "softmax"
        self.shift_size = shift_size
        self.m = ( (self.shift_size+1) // 2 ) - 1

    def build(self, input_layer, ntm_layer, idx=0):
        w = super().build(input_layer, ntm_layer, idx)
        self.shifts = tf.keras.layers.Dense(self.shift_size, activation=self.shift_activation, name=self.name+"_gate")(input_layer)
        self.gamma = tf.keras.layers.Dense(1, activation="relu", name=self.name + "_gamma")(input_layer)
        
        self.layers.append(self.shifts)
        self.layers.append(self.gamma)

        paddings = tf.Variable( tf.zeros( (tf.rank(self.shifts), 2), dtype=tf.int32 ), dtype=tf.int32 )
        npaddings = paddings[-1, 1].assign(ntm_layer.lines - self.shift_size)
        
        self.shifts = tf.pad(self.shifts, npaddings)
        rows  = []
        for i in range(ntm_layer.lines):
            p = (i+1) + ntm_layer.lines - self.m
            rows.append(tf.roll(self.shifts, shift=p, axis=-1))
        mshifts = tf.stack(rows, axis=1)
        #[None, M] -->  [None, M, 1]
        bkp_shape = tf.shape(w)
        w = tf.expand_dims(w, axis=-1)
        #[None, M, M] x [None, M, 1]
        w = tf.matmul(mshifts, w)
        w = tf.reshape(w, bkp_shape)
        self.w = tf.keras.layers.Softmax(name='%s_output'%(self.name))( w * (self.gamma + 1) )
        self.layers.append(self.w)
        if ntm_layer.network is not None:
            self.pw_slot.update.append(self.w)
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
        return self.read_head #shape = (None, TS, N, 1)


class NTMNet(tf.keras.layers.Layer):
  def __init__(self, lines=12, columns=10, writers=None, readers=None, timesteps=1, syncAtEndOfEpisode=True, name="NTMNet"):
    super(NTMNet, self).__init__(name=name)

    if writers is None:
        self.writers = [NTMWriteHead()]
    else:
        self.writers = writers
    if readers is None:
        self.readers = [NTMReadHead()]
    else:
        self.readers = readers

    self.timesteps = timesteps
    self.numwriters = len(self.writers)
    self.numreaders = len(self.readers)
    self.epsilon = 1.0e-12
    self.memory_slot = MemorySlot("%s_memory"%(name), (lines, columns), epsilon=self.epsilon, singleslot=True)
    self.reading_slot = MemorySlot("%s_reading"%(name), (self.numreaders, columns), epsilon=self.epsilon)
    
    self.memory = self.memory_slot.input
    self.reading = self.reading_slot.input
    self.lines = lines
    self.columns = columns
    self.layers = None
    self.syncAtEndOfEpisode = syncAtEndOfEpisode

  def build(self, input_shape):
    pass

  def call(self, inputs, shared_layers=None, network=None):
    self.network = network
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

    for idx, reader in enumerate(self.readers):
        rout = reader.build(features, self, idx)
        self.read_out.append(rout)
        self.reading_slot.update.append(rout)
        self.layers += reader.layers
    for idx, writer in enumerate(self.writers):
        wh = writer.build(features, self, idx)
        self.memory_slot.update.append(wh)
        self.layers += writer.layers

    if network is not None:
        network.AddMemorySlot(self.memory_slot)
        network.AddMemorySlot(self.reading_slot)
        network.syncAtEndOfEpisode = self.syncAtEndOfEpisode

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
    def __init__(self, units=256, num_layers=1, seq_size = 10, return_state=False, syncAtEndOfEpisode=False, initial_value=1.0e-12):
        self.units = units
        k  = num_layers
        name = "LSTMNet"
        self.h_slot = MemorySlot("%s_memory"%(name), (k, units), epsilon=initial_value )
        self.c_slot = MemorySlot("%s_reading"%(name), (k, units), epsilon=initial_value )

        self.state_h = self.h_slot.input
        self.state_c = self.h_slot.input

        self.num_layers = num_layers

        self.outputs = None
        self.shapes = None
        self.return_state = return_state
        self.size = 0
        self.layers = []
        self.initial_value = initial_value
        self.seq_size = seq_size
        self.syncAtEndOfEpisode = syncAtEndOfEpisode

    def __call__(self, features, network):
        return self.buildlayers(features, network)

    def buildlayers(self, features, network, **kwargs):
        if network is not None:
            network.AddMemorySlot(self.h_slot)
            network.AddMemorySlot(self.c_slot)
            network.syncAtEndOfEpisode = self.syncAtEndOfEpisode
            network.timesteps = self.seq_size

        self.outputs = []
        self.shapes = []
        layers = []
        r = self.num_layers
        stateh1 = None
        statec1 = None
        #hidden = tf.expand_dims(features, 0)
        hidden = features
        if r == 1:
            rnn1_layers = tf.keras.layers.RNN(tf.keras.layers.LSTMCell(self.units, name="cell1", **kwargs), return_sequences=False, return_state=True, name="rnn1")
            hidden, stateh1, statec1 = rnn1_layers(hidden, initial_state=[self.state_h[:,0,:], self.state_c[:,0,:]]) 
            #hidden = tf.reshape(hidden, [-1, self.units])
            layers.append(hidden)
            self.h_slot.update.append(stateh1)
            self.c_slot.update.append(statec1)
            self.shapes.append( (1, self.units) )
            self.size += 1
        else:
            for i in range(r):
                if i < (r-1):
                    rnn_layer = tf.keras.layers.RNN(tf.keras.layers.LSTMCell(self.units, name="cell%d"%(i)), return_sequences=True, return_state=True, name="rnn%d"%(i), **kwargs)
                    #op = tf.print(tf.shape(hidden))
                    #with tf.control_dependencies([op]):
                    hidden, stateh1, statec1 = rnn_layer(hidden, initial_state=[self.state_h[:,i,:], self.state_c[:,i,:]])              
                else:
                    rnn_layer = tf.keras.layers.RNN(tf.keras.layers.LSTMCell(self.units, name="cell%d"%(i)), return_state=True, return_sequences=False, name="rnn%d"%(i), **kwargs)
                    hidden, stateh1, statec1 = rnn_layer(hidden, initial_state=[self.state_h[:,i,:], self.state_c[:,i,:]])
                    #hidden = tf.reshape(hidden, [-1, self.units])
                layers.append(hidden)
                self.size += 1    
                self.shapes.append( (1, self.units) )
                self.h_slot.update.append(stateh1)
                self.c_slot.update.append(statec1)

        if self.return_state:
            return hidden, layers, stateh1, statec1
        else:
            return hidden, layers
