###################################################
#Simple use example of the AI4U algorithms.       #
# This example show how creating a simple         #
# environment GenericEnvironment (AI4UGenEnv).    #
# In this example, i applied Neural Turing Machine#
# (NTM) on simple reinforcement learning environ- #
# ment.                                           #
# Gilzamir Gomes (C) 2021 - gilzamir@outlook.com  #
###################################################
import gym
from gym import spaces
from ai4u.ml.a3c.train import run as run_train
from ai4u.ml.a3c.run_checkpoint import run as run_test
from ai4u.utils import environment_definitions
import AI4UGenEnv
from ai4u.ml.a3c.utils import NTMNet
import argparse
#import simplest
import numpy as np
from collections import deque

##############################################################################################################
#Callback for model construction.
#@param obs_shape = neural network input shape.
#@param n_actions = number of actions.
#@param debug: flag for debug activation.
#@param extra_input_shape: when neural network receives two type of inputs.
#@param network: a Network wrapper (necessary for lstm and ntm layers). 
# If you use a LSTM network named lstm, use network.setLSTMLayer(lstm).
# If you use a NTM network named ntm, use network.setNTMLayer(ntm).
#---------------------------------------------------------------------------------
# Requeirements:
# AI4UGenEnv:
# enter in clientside/generic_gym and run this command:
# > pip install -e .
##############################################################################################################
def make_inference_network(obs_shape, n_actions, debug=False, extra_inputs_shape=None, network=None):
    import tensorflow as tf
    from ai4u.ml.a3c.multi_scope_train_op import make_train_op 
    from ai4u.ml.a3c.utils_tensorflow import make_grad_histograms, make_histograms, make_rmsprop_histograms, \
        logit_entropy, make_copy_ops

    ntm = NTMNet(lines=3, columns=2, eps=1.0e-12)
    
    #INPUT+MEMORY
    observations = tf.placeholder(tf.float32, [None] + list(obs_shape))
    features = tf.keras.layers.Concatenate()([observations, ntm.reading])    
    
    #HEAD SHARED LAYERS
    hidden1 = tf.keras.layers.Dense(100, activation=tf.nn.relu, name='hidden1')(features)
    ctr_hidden = tf.keras.layers.Dense(100, activation=tf.nn.relu, name='ctr-hidden')(hidden1)

    #WEIGHTING - FOCUSING
    write_w = ntm.buildWeightings(ctr_hidden, name="ntm_write_key")
    read_w  = ntm.buildWeightings(ctr_hidden, name="ntm_read_key")

    #READ HEAD
    readHead = ntm.buildReadHead(w=read_w)
    
    #WRITE HEAD
    writeHead = ntm.buildWriteHead(ctr_hidden, w=write_w)

    #CONTROLLER DECISION MODULE
    exp_features = tf.keras.layers.Concatenate()([observations, readHead])
    hidden2 = tf.keras.layers.Dense(100, activation=tf.nn.relu, name='hidden2')(exp_features)
    hidden3 = tf.keras.layers.Dense(100, activation=tf.nn.relu, name='hidden3')(hidden2)
    action_logits = tf.keras.layers.Dense(n_actions, activation=None, name='action_logits')(hidden3)
    action_probs = tf.nn.softmax(action_logits)

    #print_op = tf.print(readHead)
    #with tf.control_dependencies([print_op]):
    values = tf.keras.layers.Dense(1, activation=None, name='value')(hidden3)
    # Shape is currently (?, 1)
    # Convert to just (?)
    values = values[:, 0]
    layers = [ctr_hidden, hidden1, hidden2] + ntm.layers 

    if network is not None:
        network.setNTMLayer(ntm)

    return observations, action_logits, action_probs, values, layers


#It is a environment wrapper for simple non-graphics environments.
class Wrapper:
  def __init__(self):
    # Define action and observation space
    # They must be gym.spaces objects
    # Example when using discrete actions:
    self.action_space = spaces.Discrete(2)
    # Example for using image as input:
    self.state_size = 10
    self.hist_size = 10
    self.observation_space = spaces.Box(low=0, high=1,
                                        shape=(self.hist_size, ), dtype=np.uint8)
    self.state = [0]*self.state_size
    self.obs = deque(maxlen=self.hist_size)
    self.rewards = 0
    self.pos = 0
    self.steps = 0

  #setup method defines a environment initial configurations.
  #here, resources can be created or main configurations defined.
  #@param env: object that encapsulate environment properties.
  #@param defs: dictionary containing pairs <property_name, property_value>.
  def setup(self, env, defs):
    if 'state_shape' in defs:
        self.observation_space = spaces.Box(low=0, high=1, shape=defs['state_shape'], dtype=np.uint8)
        self.hist_size = defs['state_shape'][-1]
        self.obs = deque(maxlen=self.hist_size)

        if 'env_size' in defs:
            self.state_size = defs['env_size']
            self.state = [0]*self.state_size

        self.rewards = 0
        self.pos = 0
        self.steps = 0
    if 'action_shape' in defs:
        self.action_space = spaces.Discrete(defs['action_shape'][0])

    env.observation_space = self.observation_space
    env.action_space = self.action_space

  #This method get an action from policy and defines how this actin is applied on envioronment.
  #In this method, user can defining skipframe strategies, for example. But, this example implements 
  #a environment directly.
  #@param action: an action code.
  #@param info: dictionary containing extra policy informations, as action propability distribution.
  def act(self, action, info=None):
    reward = 0
    if action == 1:
        self.pos = min(self.pos + 1, len(self.state)-1)
    done = False
    if self.steps >= self.state_size:
        done = True
        if self.state[self.pos] == self.state[0] and self.pos > 0:
            reward = 10
    self.steps += 1
    o = np.array([self.state[self.pos]], dtype=np.float32)
    self.obs[min(self.hist_size-1, self.state_size - self.pos - 1)] = o
    o = np.array(self.obs).reshape( self.observation_space.shape )
    return o, reward, done, {}

  #This method define a initial seed for internal random generator.
  #@param value: seed value.
  def seed(self, value):
    return value

  #This is a first method called in an episode.
  def reset(self):
    self.obs = deque(maxlen=self.hist_size)
    pos0 = np.random.choice([-1, 1])
    if pos0 == -1:
        self.state = [1]*self.state_size
    else:
        self.state = [-1]*self.state_size
    self.state[0] = pos0
    idx = np.random.choice(len(self.state)-1)
    if idx == 0:
        idx = 1
    self.state[idx] = pos0
    self.pos = 0
    self.steps = 0
    o = np.array([self.state[0]], dtype=np.float32)
    for _ in range(self.hist_size-1):
        self.obs.append(np.zeros(o.shape))
    self.obs.append(o)
    o = np.array(self.obs).reshape(self.observation_space.shape)
    return  o# reward, done, info can't be included

  #This method produces an output for human reading.
  #@param mode: see opengym documentation.
  def render(self, mode='human'):
    print(self.state)

  #This method release opened resources before episode ending.
  def close (self):
    pass


def parse_args():
    parser = argparse.ArgumentParser()
    parser.add_argument("--run",
                        choices=['train', 'test'],
                        default='train')
    parser.add_argument("--id", default='0')
    parser.add_argument('--path', default='.')
    parser.add_argument('--load_ckpt')
    parser.add_argument('--preprocessing', choices=['generic', 'user_defined'])
    parser.add_argument("--n_steps", type=float, default=10e6)
    parser.add_argument("--steps_per_update", type=int, default=5)
    return parser.parse_args()

def make_env_def():
        environment_definitions['state_shape'] = (1, )
        environment_definitions['action_shape'] = (2,)
        environment_definitions['env_size'] = 3
        environment_definitions['make_inference_network'] = make_inference_network
        environment_definitions['wrapper'] = Wrapper

def train():
        args = ['--n_workers=4', '--steps_per_update=5', 'AI4UGenEnv-v0']
        make_env_def()
        run_train(environment_definitions, args)

def test(path):
        args = ['AI4UGenEnv-v0', path,]
        make_env_def()
        run_test(environment_definitions, args)


if __name__ == '__main__':
   args = parse_args()
   if args.run == "train":
        train()
   elif args.run == "test":
        #test(args.path)
        make_env_def()
        env = gym.make("AI4UGenEnv-v0")
        env.configure(environment_definitions, 1)
        obs = env.reset()
        print("OBS: ", obs)
        done = False
        while not done:
            a = int(input("Action: "))
            obs, reward, done, info = env.step(a, {})
            print("OBS: ", obs)
            print("RWD: ", reward)
            print("DN: ", done)
            env.render()
